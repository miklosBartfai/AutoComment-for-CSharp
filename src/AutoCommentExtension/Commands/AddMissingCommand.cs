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

        private void DocumentSaved(string x)
        {
            if (General.Instance.RunOnSave
                && General.Instance.RunOnSaveCommand == AutoCommentCommand.AutoCommentMissing)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    try
                    {
                        await CommentAsync();
                        var doc = await VS.Documents.GetActiveDocumentViewAsync();

                        if (doc != null)
                        {
                            doc.Document.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        var e = ex;
                    }
                }).FireAndForget();
            }
        }

        private async Task CommentAsync()
        {
            var doc = await VS.Documents.GetActiveDocumentViewAsync();

            if (doc == null)
            {
                return;
            }

            var contentType = doc.TextBuffer.ContentType;

            if (contentType.TypeName != ContentTypes.CSharp)
            {
                return;
            }

            var options = await General.GetLiveInstanceAsync();

            if (options == null)
            {
                return;
            }

            var lines = doc.TextBuffer.CurrentSnapshot.Lines;

            using (var edit = doc.TextBuffer.CreateEdit())
            {
                var lineIndex = 1;
                var lineCount = lines.Count();
                var prevLineWasComment = false;

                foreach (var line in lines)
                {
                    var text = line.GetText();

                    if (XmlComment.IsXmlComment(text))
                    {
                        prevLineWasComment = true;
                    }
                    else
                    {
                        if (!prevLineWasComment)
                        {
                            var comment = XmlComment.GetComment(text, options);

                            if (comment != null)
                            {
                                edit.Insert(line.Start, comment);
                            }
                        }
                        else
                        {
                            prevLineWasComment = false;
                        }
                    }

                    await VS.StatusBar.ShowProgressAsync($"Step {lineIndex}/{lineCount}", lineIndex++, lineCount);
                }

                edit.Apply();
            }
        }
    }
}
