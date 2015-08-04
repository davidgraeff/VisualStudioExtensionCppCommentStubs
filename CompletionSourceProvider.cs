// Decompiled with JetBrains decompiler
// Type: CppTripleSlash.TripleSlashCompletionSourceProvider
// Assembly: CppTripleSlash, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8749b42b75b9d42b
// MVID: C925F0DE-498F-48DC-BF66-69CEA90E2E64
// Assembly location: C:\Users\David\Downloads\CppTripleSlash.dll

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CppCommentsCompletion
{
  [Name("xml doc comment completion")]
  [Export(typeof (ICompletionSourceProvider))]
  [ContentType("code")]
  public class TripleSlashCompletionSourceProvider : ICompletionSourceProvider
  {
    [Import]
    internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

    [Import]
    internal IGlyphService GlyphService { get; set; }

    public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
    {
      return (ICompletionSource) new CppCommentsCompletionSource(this, textBuffer);
    }
  }
}
