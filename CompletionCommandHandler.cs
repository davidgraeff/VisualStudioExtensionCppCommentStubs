// Decompiled with JetBrains decompiler
// Type: CppTripleSlash.TripleSlashCompletionCommandHandler
// Assembly: CppTripleSlash, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8749b42b75b9d42b
// MVID: C925F0DE-498F-48DC-BF66-69CEA90E2E64
// Assembly location: C:\Users\David\Downloads\CppTripleSlash.dll

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CppCommentsCompletion
{
    public class TripleSlashCompletionCommandHandler : IOleCommandTarget
    {
        public const string CppTypeName = "C/C++";
        private IOleCommandTarget m_nextCommandHandler;
        private IWpfTextView m_textView;
        private TripleSlashCompletionHandlerProvider m_provider;
        private ICompletionSession m_session;
        private DTE m_dte;
        //private IVsTextView textViewAdapter;
        //private IWpfTextView textView;
        //private TripleSlashCompletionHandlerProvider tripleSlashCompletionHandlerProvider;
        //private SVsServiceProvider serviceProvider;

        private ToolsOptions.CommentTypeEnum commentyType = ToolsOptions.CommentTypeEnum.NativeDoxygen;
        private int auto_complete_start_char;
        private int auto_complete_end_char;

        public TripleSlashCompletionCommandHandler(IVsTextView textViewAdapter, IWpfTextView textView, TripleSlashCompletionHandlerProvider provider, SVsServiceProvider serviceProvider)
        {
            this.m_textView = textView;
            this.m_provider = provider;
            this.m_dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            if (textViewAdapter == null || textView == null || (textView.TextBuffer == null || !(textView.TextBuffer.ContentType.TypeName == "C/C++")))
                return;

            object obj;
            (VSPackageMain.instance as IVsPackage).GetAutomationObject("C++.Autocomplete comments", out obj);
            
            ToolsOptions options = obj as ToolsOptions;
            if (options != null)
            {
                commentyType = options.CommentType;
            }

            switch (commentyType)
            {
                case ToolsOptions.CommentTypeEnum.JavaStyle:
                    auto_complete_start_char = '@';
                    auto_complete_end_char = 0;
                    break;
                case ToolsOptions.CommentTypeEnum.NativeDoxygen:
                case ToolsOptions.CommentTypeEnum.QtStyle:
                    auto_complete_start_char = '\\';
                    auto_complete_end_char = 0;
                    break;
                case ToolsOptions.CommentTypeEnum.VisualStudioXML:
                    auto_complete_start_char = '<';
                    auto_complete_end_char = '>';
                    break;
            }

            
            textViewAdapter.AddCommandFilter((IOleCommandTarget)this, out this.m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return this.m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            try
            {
                if (VsShellUtilities.IsInAutomationFunction((System.IServiceProvider)this.m_provider.ServiceProvider) || this.m_dte == null)
                    return this.m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                char c = char.MinValue;
                if (pguidCmdGroup == VSConstants.VSStd2K && (int)nCmdID == 1)
                    c = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

                if (c == '/')
                {
                    string text = this.m_textView.TextSnapshot.GetLineFromPosition(this.m_textView.Caret.Position.BufferPosition.Position).GetText();
                    if ((text + "/").Trim() == "///")
                    {
                        string str = text.Replace(text.TrimStart(), "");
                        TextSelection textSelection = this.m_dte.ActiveDocument.Selection as TextSelection;
                        int line = textSelection.ActivePoint.Line;
                        int lineCharOffset = textSelection.ActivePoint.LineCharOffset;
                        textSelection.LineDown(false, 1);
                        textSelection.EndOfLine(false);
                        CodeElement codeElement1 = (CodeElement)null;
                        FileCodeModel fileCodeModel = this.m_dte.ActiveDocument.ProjectItem.FileCodeModel;
                        if (fileCodeModel != null)
                            codeElement1 = fileCodeModel.CodeElementFromPoint((TextPoint)textSelection.ActivePoint, vsCMElement.vsCMElementFunction);

                        textSelection.MoveToLineAndOffset(line, lineCharOffset);
                        switch(commentyType)
                        {
                            case ToolsOptions.CommentTypeEnum.JavaStyle:
                                textSelection.Insert(createJavaStyleComment(codeElement1, str));
                                break;
                            case ToolsOptions.CommentTypeEnum.NativeDoxygen:
                            case ToolsOptions.CommentTypeEnum.QtStyle:
                                textSelection.Insert(createNativeDoxygenComment(codeElement1, str));
                                break;
                            case ToolsOptions.CommentTypeEnum.VisualStudioXML:
                                textSelection.Insert(createXMLComment(codeElement1, str));
                                break;
                        }
                        textSelection.MoveToLineAndOffset(line, lineCharOffset);
                        textSelection.LineDown();
                        textSelection.EndOfLine();
                        return 0;
                    }
                }

                if (this.m_session != null && !this.m_session.IsDismissed)
                {
                    if ((int)nCmdID == 3 || (int)nCmdID == 4 || (auto_complete_end_char != 0 && c == auto_complete_end_char))
                    {
                        if (this.m_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                        {
                            string displayText = this.m_session.SelectedCompletionSet.SelectionStatus.Completion.DisplayText;
                            this.m_session.Commit();
                            TextSelection textSelection = this.m_dte.ActiveDocument.Selection as TextSelection;
                            switch (displayText)
                            {
                                case "<!-->":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<![CDATA[>":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<c>":
                                    textSelection.CharLeft(false, 4);
                                    break;
                                case "<code>":
                                    textSelection.CharLeft(false, 7);
                                    break;
                                case "<example>":
                                    textSelection.CharLeft(false, 10);
                                    break;
                                case "<exception>":
                                    textSelection.CharLeft(false, 14);
                                    break;
                                case "<include>":
                                    textSelection.CharLeft(false, 21);
                                    break;
                                case "<list>":
                                    textSelection.CharLeft(false, 7);
                                    break;
                                case "<para>":
                                    textSelection.CharLeft(false, 7);
                                    break;
                                case "<param>":
                                    textSelection.CharLeft(false, 10);
                                    break;
                                case "<paramref>":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<permission>":
                                    textSelection.CharLeft(false, 15);
                                    break;
                                case "<remarks>":
                                    textSelection.CharLeft(false, 10);
                                    break;
                                case "<returns>":
                                    textSelection.CharLeft(false, 10);
                                    break;
                                case "<see>":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<seealso>":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<typeparam>":
                                    textSelection.CharLeft(false, 14);
                                    break;
                                case "<typeparamref>":
                                    textSelection.CharLeft(false, 3);
                                    break;
                                case "<value>":
                                    textSelection.CharLeft(false, 8);
                                    break;
                            }
                            return 0;
                        }
                        this.m_session.Dismiss();
                    }
                }
                else if (pguidCmdGroup == VSConstants.VSStd2K && (int)nCmdID == 3) // Make the next line also a comment by pressing enter if the current one is a comment
                {
                    string text = this.m_textView.TextSnapshot.GetLineFromPosition(this.m_textView.Caret.Position.BufferPosition.Position).GetText();
                    if (text.TrimStart().StartsWith("///"))
                    {
                        (this.m_dte.ActiveDocument.Selection as TextSelection).Insert("\r\n" + text.Replace(text.TrimStart(), "") + "/// ", 1);
                        return 0;
                    }
                }

                if (c == auto_complete_start_char)
                {
                    if (this.m_textView.TextSnapshot.GetLineFromPosition(this.m_textView.Caret.Position.BufferPosition.Position).GetText().TrimStart().StartsWith("///") && (this.m_session == null || this.m_session.IsDismissed) && this.TriggerCompletion())
                    {
                        this.m_session.SelectedCompletionSet.SelectBestMatch();
                        this.m_session.SelectedCompletionSet.Recalculate();
                        return 0;
                    }
                }
                else if ((nCmdID == 2 || nCmdID == 6 || char.IsLetter(c)) && (this.m_session != null && !this.m_session.IsDismissed))
                {
                    this.m_session.SelectedCompletionSet.SelectBestMatch();
                    this.m_session.SelectedCompletionSet.Recalculate();
                    return 0;
                }
                return this.m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            catch
            {
            }
            return -2147467259;
        }

        private String createXMLComment(CodeElement codeElement, string lineOffset)
        {
            StringBuilder stringBuilder = new StringBuilder("/ <summary>\r\n" + lineOffset + "/// \r\n" + lineOffset + "/// </summary>");

            if (codeElement != null && codeElement is CodeFunction)
            {
                CodeFunction codeFunction = codeElement as CodeFunction;
                foreach (CodeElement codeElementChild in codeElement.Children)
                {
                    CodeParameter codeParameter = codeElementChild as CodeParameter;
                    if (codeParameter != null)
                        stringBuilder.AppendFormat("\r\n" + lineOffset + "/// <param name=\"{0}\"></param>", codeParameter.FullName);
                }

                if (codeFunction.Type.AsString != "void")
                    stringBuilder.AppendFormat("\r\n" + lineOffset + "/// <returns></returns>");

            }

            return stringBuilder.ToString();
        }

        private String createNativeDoxygenComment(CodeElement codeElement, string lineOffset)
        {
            StringBuilder stringBuilder = new StringBuilder("/ \r\n" + lineOffset + "/// \r\n" + lineOffset + "/// ");

            if (codeElement != null && codeElement is CodeFunction)
            {
                CodeFunction codeFunction = codeElement as CodeFunction;
                foreach (CodeElement codeElementChild in codeElement.Children)
                {
                    CodeParameter codeParameter = codeElementChild as CodeParameter;
                    if (codeParameter != null)
                        stringBuilder.AppendFormat("\r\n" + lineOffset + "/// \\param {0} ", codeParameter.FullName);
                }

                if (codeFunction.Type.AsString != "void")
                    stringBuilder.AppendFormat("\r\n" + lineOffset + "/// \\return ");

            }

            return stringBuilder.ToString();
        }

        private String createJavaStyleComment(CodeElement codeElement, string lineOffset)
        {
            StringBuilder stringBuilder = new StringBuilder("/ \r\n" + lineOffset + "/// \r\n" + lineOffset + "/// ");

            if (codeElement != null && codeElement is CodeFunction)
            {
                CodeFunction codeFunction = codeElement as CodeFunction;
                foreach (CodeElement codeElementChild in codeElement.Children)
                {
                    CodeParameter codeParameter = codeElementChild as CodeParameter;
                    if (codeParameter != null)
                        stringBuilder.AppendFormat("\r\n" + lineOffset + "/// @param {0} ", codeParameter.FullName);
                }

                if (codeFunction.Type.AsString != "void")
                    stringBuilder.AppendFormat("\r\n" + lineOffset + "/// @return ");

            }

            return stringBuilder.ToString();
        }

        private bool TriggerCompletion()
        {
            try
            {
                if (this.m_session != null)
                    return false;
                SnapshotPoint? point = this.m_textView.Caret.Position.Point.GetPoint((Predicate<ITextBuffer>)(textBuffer => !textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
                if (!point.HasValue)
                    return false;
                this.m_session = this.m_provider.CompletionBroker.CreateCompletionSession((ITextView)this.m_textView, point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive), true);
                this.m_session.Dismissed += new EventHandler(this.OnSessionDismissed);
                this.m_session.Start();
                return true;
            }
            catch
            {
            }
            return false;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            if (this.m_session == null)
                return;
            this.m_session.Dismissed -= new EventHandler(this.OnSessionDismissed);
            this.m_session = (ICompletionSession)null;
        }
    }
}
