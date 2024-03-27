using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace AutoCommentExtension
{
    [Command(PackageIds.AddMissing)]
    internal sealed class AddMissingCommand : BaseCommand<AddMissingCommand>
    {
        public AddMissingCommand()
        {
            VS.Events.DocumentEvents.Saved += DocumentSaved;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommentAsync();
        }

        private void DocumentSaved(string path)
        {
            if (General.Instance.RunOnSave
                && General.Instance.RunOnSaveCommand == AutoCommentCommand.AutoCommentMissing
                && path.EndsWith(".cs"))
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    try
                    {
                        await CommentAsync();
                        var document = await VS.Documents.GetActiveDocumentViewAsync();
                        document?.Document.Save();
                    }
                    catch (Exception ex)
                    {
                        var e = ex;
                    }
                }).FireAndForget();
            }
        }

        private static async Task CommentAsync()
        {
            var document = await VS.Documents.GetActiveDocumentViewAsync();

            if (document == null)
            {
                return;
            }

            var contentType = document.TextBuffer.ContentType;

            if (contentType.TypeName != ContentTypes.CSharp)
            {
                return;
            }

            var options = await General.GetLiveInstanceAsync();

            if (options == null)
            {
                return;
            }

            await EditDocumentWithOptionsAsync(document, options);
        }

        public static async Task EditDocumentWithOptionsAsync(DocumentView document, General options)
        {
            var lines = document.TextBuffer.CurrentSnapshot.Lines;

            using var edit = document.TextBuffer.CreateEdit();

            var lineIndex = 1;
            var lineCount = lines.Count();
            var prevLineWasComment = false;
            var prevLineWasAttribute = false;
            var firstAttributeLine = lines.FirstOrDefault();
            var prevLineWasPartialResult = false;
            var partialResults = new List<string>();
            var firstPartialResultLine = lines.FirstOrDefault();
            var newLine = string.Empty;

            foreach (var line in lines)
            {
                var text = line.GetText();

                if (XmlComment.IsXmlComment(text))
                {
                    prevLineWasComment = true;
                    prevLineWasAttribute = false;
                    prevLineWasPartialResult = false;
                }
                else if (XmlComment.IsAttribute(text))
                {
                    if (!prevLineWasAttribute)
                    {
                        firstAttributeLine = line;
                    }

                    prevLineWasAttribute = true;
                    prevLineWasPartialResult = false;
                }
                else
                {
                    if (!prevLineWasComment)
                    {
                        (string Text, bool IsPartial, string NewLine) comment = default;

                        if (!prevLineWasPartialResult)
                        {
                            comment = XmlComment.GetComment(text, options);
                        }
                        else
                        {
                            comment = XmlComment.GetPartialComment(text, options, newLine);
                        }

                        if (comment.Text != null)
                        {
                            if (!comment.IsPartial)
                            {
                                var finalText = string.Empty;
                                var insertLine = line;

                                if (prevLineWasPartialResult)
                                {
                                    partialResults.Add(comment.Text);
                                    finalText = partialResults[0];
                                    var additionalParameters = string.Join(string.Empty, partialResults.Skip(1));
                                    finalText = finalText.Replace("{additional parameters}", additionalParameters);
                                    insertLine = firstPartialResultLine;
                                }
                                else
                                {
                                    finalText = comment.Text;
                                }

                                if (prevLineWasAttribute)
                                {
                                    insertLine = firstAttributeLine;
                                }

                                edit.Insert(insertLine.Start, finalText);

                                prevLineWasAttribute = false;
                                prevLineWasPartialResult = false;
                            }
                            else
                            {
                                if (!prevLineWasPartialResult)
                                {
                                    firstPartialResultLine = line;
                                    partialResults = new();
                                }

                                prevLineWasPartialResult = true;
                                partialResults.Add(comment.Text);
                                newLine = comment.NewLine;
                            }
                        }
                    }
                    else
                    {
                        prevLineWasAttribute = false;
                        prevLineWasPartialResult = false;
                    }

                    prevLineWasComment = false;
                }

                await VS.StatusBar.ShowProgressAsync($"Generating.. {lineIndex}/{lineCount}", lineIndex++, lineCount);
            }

            edit.Apply();
        }
    }
}
