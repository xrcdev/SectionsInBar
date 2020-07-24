using EnvDTE;
using Microsoft;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Text;

namespace ShowSelectionLength
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class CommandHandler : IWpfTextViewCreationListener
    {
        private IWpfTextView _textView;
        private static DTE _dte;

        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_dte == null)
            {
                _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;
                Assumes.Present(_dte);
            }

            _textView = textView;
            textView.Selection.SelectionChanged += SelectionChanged;

            textView.Closed += TextView_Closed;
            //textView.Caret.PositionChanged += CaretPositionChanged;
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            var textView = (IWpfTextView)sender;
            textView.Selection.SelectionChanged -= SelectionChanged;
            textView.Closed -= TextView_Closed;
            //textView.Caret.PositionChanged -= CaretPositionChanged;

        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                var caretText = (ITextCaret)sender;

                var toShowText = caretText.Position;


            }).FileAndForget(nameof(ShowSelectionLength));
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                var selection = (ITextSelection)sender;

                //if (selection.IsEmpty)
                //{
                //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                //    _dte.StatusBar.Clear();
                //}
                //else
                //{
                StringBuilder sbBuilder = new StringBuilder(80);

                int length = 0;
                var startLine = selection.Start.Position.GetContainingLine();
                var endLine = selection.End.Position.GetContainingLine();

                //int caretPositionAfterInput = selection.TextView.Caret.Position.BufferPosition;
                foreach (SnapshotSpan snapshotSpan in selection.SelectedSpans)
                {
                    if (sbBuilder.Length == 0)
                    {
                        sbBuilder.Append(
                            $"行:{startLine.LineNumber + 1} 列:{snapshotSpan.Start.Position - startLine.Start.Position + 1}");
                    }
                    length += snapshotSpan.Length;
                }

                if (length > 0)
                {
                    //if (startLine.LineNumber == endLine.LineNumber)
                    //{
                    //    sbBuilder.Append($" sel({length})");
                    //}
                    //else
                    //{
                    sbBuilder.Append($" 选中:{length} | {endLine.LineNumber - startLine.LineNumber + 1}");
                    //}
                }
                var txt = _dte.StatusBar.Text;
                if (_dte.StatusBar.Text.Contains("行:"))
                {
                    var idx = _dte.StatusBar.Text.IndexOf("行:");
                    txt = _dte.StatusBar.Text.Substring(0, idx);
                }

                sbBuilder.Insert(0, txt.PadRight(50, (char)32));
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _dte.StatusBar.Text = sbBuilder.ToString();
                sbBuilder.Clear();
                //}

            }).FileAndForget(nameof(ShowSelectionLength));
        }
    }
}
