using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CppCommentsCompletion
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ToolsOptions : DialogPage
    {
        public enum CommentTypeEnum { NativeDoxygen, JavaStyle, QtStyle, VisualStudioXML };
        private CommentTypeEnum _commentType;

        [Category("C++")]
        [DisplayName("Comment Type")]
        [Description("This extension supports multiple comment types for auto completion")]
        public CommentTypeEnum CommentType
        {
            get { return _commentType; }
            set { _commentType = value; }
        }
    }
}
