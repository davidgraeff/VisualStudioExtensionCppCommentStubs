// Decompiled with JetBrains decompiler
// Type: CppTripleSlash.TripleSlashCompletionHandlerProvider
// Assembly: CppTripleSlash, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8749b42b75b9d42b
// MVID: C925F0DE-498F-48DC-BF66-69CEA90E2E64
// Assembly location: C:\Users\David\Downloads\CppTripleSlash.dll

using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace CppCommentsCompletion
{
  [Name("C++ Triple Slash Completion Handler")]
  [Export(typeof (IVsTextViewCreationListener))]
  [TextViewRole("EDITABLE")]
  [ContentType("code")]
  public class TripleSlashCompletionHandlerProvider : IVsTextViewCreationListener
  {
    [Import]
    public IVsEditorAdaptersFactoryService AdapterService;

    [Import]
    public ICompletionBroker CompletionBroker { get; set; }

    [Import]
    public SVsServiceProvider ServiceProvider { get; set; }

    public void VsTextViewCreated(IVsTextView textViewAdapter)
    {
      try
      {
        IWpfTextView textView = this.AdapterService.GetWpfTextView(textViewAdapter);
        if (textView == null)
          return;
        Func<TripleSlashCompletionCommandHandler> creator = (Func<TripleSlashCompletionCommandHandler>) (() => new TripleSlashCompletionCommandHandler(textViewAdapter, textView, this, this.ServiceProvider));
        textView.Properties.GetOrCreateSingletonProperty<TripleSlashCompletionCommandHandler>(creator);
      }
      catch
      {
      }
    }
  }
}
