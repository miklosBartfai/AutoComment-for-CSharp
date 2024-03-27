using System.Linq;
using Microsoft.VisualStudio.Text;

namespace AutoCommentExtension
{
    [Command(PackageIds.AddAll)]
    internal sealed class AddAllCommand : BaseCommand<AddAllCommand>
    {
        public AddAllCommand()
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
                && General.Instance.RunOnSaveCommand == AutoCommentCommand.AutoCommentAll
                && path.EndsWith(".cs"))
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    try
                    {
                        await CommentAsync();
                        var document = await VS.Documents.GetActiveDocumentViewAsync();
                        document.Document.Save();
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

            await CleanCommentsAsync(document);
            await AddMissingCommand.EditDocumentWithOptionsAsync(document, options);
        }

        private static async Task CleanCommentsAsync(DocumentView document)
        {
            var lines = document.TextBuffer.CurrentSnapshot.Lines;

            using var edit = document.TextBuffer.CreateEdit();

            var lineIndex = 1;
            var lineCount = lines.Count();

            foreach (var line in lines)
            {
                var text = line.GetText();

                if (XmlComment.IsXmlComment(text))
                {
                    var extent = Span.FromBounds(line.Start, line.EndIncludingLineBreak);
                    edit.Delete(extent);
                }

                await VS.StatusBar.ShowProgressAsync($"Cleaning.. {lineIndex}/{lineCount}", lineIndex++, lineCount);
            }

            edit.Apply();
        }
    }
}
