// Decompiled with JetBrains decompiler
// Type: CppTripleSlash.TripleSlashCompletionSource
// Assembly: CppTripleSlash, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8749b42b75b9d42b
// MVID: C925F0DE-498F-48DC-BF66-69CEA90E2E64
// Assembly location: C:\Users\David\Downloads\CppTripleSlash.dll

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CppCommentsCompletion
{
    public class CppCommentsCompletionSource : ICompletionSource, IDisposable
    {
        private List<Completion> m_compList = new List<Completion>();
        private TripleSlashCompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private bool m_isDisposed;
        private ToolsOptions.CommentTypeEnum commentyType = ToolsOptions.CommentTypeEnum.NativeDoxygen;

        public CppCommentsCompletionSource(TripleSlashCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            this.m_sourceProvider = sourceProvider;
            this.m_textBuffer = textBuffer;
            ImageSource iconSource = (ImageSource)null;
            try
            {
                iconSource = this.m_sourceProvider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
            }
            catch
            {
            }

            object obj;
            (VSPackageMain.instance as IVsPackage).GetAutomationObject("C++.Autocomplete comments", out obj);

            ToolsOptions options = obj as ToolsOptions;
            if (options != null)
            {
                commentyType = options.CommentType;
            }

            switch (commentyType)
            {
                case ToolsOptions.CommentTypeEnum.VisualStudioXML:
                    this.m_compList.Add(new Completion("<!-->", "<!---->", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<![CDATA[>", "<![CDATA[]]>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<c>", "<c></c>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<code>", "<code></code>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<example>", "<example></example>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<exception>", "<exception cref=\"\"></exception>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<include>", "<include file='' path='[@name=\"\"]'/>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<list>", "<list></list>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<para>", "<para></para>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<param>", "<param name=\"\"></param>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<paramref>", "<paramref name=\"\"/>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<permission>", "<permission cref=\"\"></permission>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<remarks>", "<remarks></remarks>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<returns>", "<returns></returns>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<see>", "<see cref=\"\"/>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<seealso>", "<seealso cref=\"\"/>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<typeparam>", "<typeparam name=\"\"></typeparam>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<typeparamref>", "<typeparamref name=\"\"/>", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("<value>", "<value></value>", string.Empty, iconSource, string.Empty));
                    break;
                case ToolsOptions.CommentTypeEnum.JavaStyle:
                    this.m_compList.Add(new Completion("@see", "@see", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("@seealso", "@seealso", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("@param", "@param", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("@return", "@return", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("@brief", "@brief", string.Empty, iconSource, string.Empty));
                    break;
                case ToolsOptions.CommentTypeEnum.NativeDoxygen:
                case ToolsOptions.CommentTypeEnum.QtStyle:
                    this.m_compList.Add(new Completion("\\see", "\\see", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("\\seealso", "\\seealso", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("\\param", "\\param", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("\\return", "\\return", string.Empty, iconSource, string.Empty));
                    this.m_compList.Add(new Completion("\\brief", "\\brief", string.Empty, iconSource, string.Empty));
                    break;
            }
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            try
            {
                if (this.m_isDisposed)
                    return;
                SnapshotPoint? triggerPoint = session.GetTriggerPoint(this.m_textBuffer.CurrentSnapshot);
                if (!triggerPoint.HasValue || (this.m_textBuffer.ContentType.TypeName != "C/C++" || !triggerPoint.Value.GetContainingLine().GetText().TrimStart().StartsWith("///")))
                    return;
                CompletionSet completionSet = new CompletionSet("TripleSlashCompletionSet", "TripleSlashCompletionSet", this.FindTokenSpanAtPosition(session.GetTriggerPoint(this.m_textBuffer), session), (IEnumerable<Completion>)this.m_compList, Enumerable.Empty<Completion>());
                completionSets.Add(completionSet);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (this.m_isDisposed)
                return;
            GC.SuppressFinalize((object)this);
            this.m_isDisposed = true;
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            try
            {
                SnapshotPoint snapshotPoint = session.TextView.Caret.Position.BufferPosition - 1;
                return snapshotPoint.Snapshot.CreateTrackingSpan((int)snapshotPoint, 1, SpanTrackingMode.EdgeInclusive);
            }
            catch
            {
            }
            return (ITrackingSpan)null;
        }
    }
}
