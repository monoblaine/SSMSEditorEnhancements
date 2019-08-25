//ş
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SSMSEditorEnhancements {
    internal static class Util {
        public static EnvDTE80.DTE2 GetDTE2 () {
            return (EnvDTE80.DTE2) Package.GetGlobalService(typeof(SDTE));
        }

        public static Boolean TryGetTextView (this EnvDTE80.DTE2 dte2, out IVsTextView textView) {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dte2.ActiveDocument == null) {
                textView = null;

                return false;
            }

            var aDocIsOpen = VsShellUtilities.IsDocumentOpen(
                provider: new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) dte2),
                fullPath: dte2.ActiveDocument.FullName,
                logicalView: Guid.Empty,
                hierarchy: out var uiHierarchy,
                itemID: out var itemID,
                windowFrame: out var windowFrame
            );

            if (!aDocIsOpen) {
                textView = null;

                return false;
            }

            textView = VsShellUtilities.GetTextView(windowFrame);

            return true;
        }

        public static IWpfTextView GetWpfTextView (this IVsTextView vsTextView) {
            IWpfTextView wpfTextView = null;

            if (vsTextView is IVsUserData userData) {
                IWpfTextViewHost viewHost;
                var guidViewHost = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;

                userData.GetData(ref guidViewHost, out var holder);

                viewHost = (IWpfTextViewHost) holder;
                wpfTextView = viewHost.TextView;
            }

            return wpfTextView;
        }
    }
}
