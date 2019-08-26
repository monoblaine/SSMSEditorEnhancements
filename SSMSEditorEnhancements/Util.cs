//ş
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
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

            windowFrame.GetCodeWindow().Value.GetLastActiveView(out textView);

            return true;
        }

        /// <summary>
        /// https://github.com/VsVim/VsVim/blob/d7b3e1a79a6d06cdae5e0334e09f9bbf5388e7df/Src/VsVimShared/Extensions.cs#L500
        /// </summary>
        /// <param name="vsWindowFrame"></param>
        /// <returns></returns>
        public static Result<IVsCodeWindow> GetCodeWindow (this IVsWindowFrame vsWindowFrame) {
            var iid = typeof(IVsCodeWindow).GUID;
            var ptr = IntPtr.Zero;

            try {
                var hr = vsWindowFrame.QueryViewInterface(ref iid, out ptr);

                if (ErrorHandler.Failed(hr)) {
                    return Result.CreateError(hr);
                }

                return Result.CreateSuccess((IVsCodeWindow) Marshal.GetObjectForIUnknown(ptr));
            }
            catch (Exception e) {
                // Venus will throw when querying for the code window
                return Result.CreateError(e);
            }
            finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.Release(ptr);
                }
            }
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

        public static IEditorOperationsFactoryService GetEditorOperations (this EnvDTE80.DTE2 dte2) {
            ThreadHelper.ThrowIfNotOnUIThread();

            var sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) dte2);
            var model = (IComponentModel) sp.GetService(typeof(SComponentModel));

            return model.GetService<IEditorOperationsFactoryService>();
        }

        public static ITextStructureNavigatorSelectorService GetTextStructureNavigatorSelector (this EnvDTE80.DTE2 dte2) {
            ThreadHelper.ThrowIfNotOnUIThread();

            var sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) dte2);
            var model = (IComponentModel) sp.GetService(typeof(SComponentModel));

            return model.GetService<ITextStructureNavigatorSelectorService>();
        }

        public static void EndOfWord (this EnvDTE80.DTE2 dte2, Boolean extendSelection) {
            if (!dte2.TryGetTextView(out var vsTextView)) {
                return;
            }

            var wpfTextView = vsTextView.GetWpfTextView();

            if (!extendSelection && !wpfTextView.Selection.IsEmpty) {
                wpfTextView.Selection.Select(wpfTextView.Selection.ActivePoint, wpfTextView.Selection.ActivePoint);
            }

            var caret = wpfTextView.Caret;
            var point = caret.Position.BufferPosition;

            if (point.Position == wpfTextView.TextSnapshot.Length) {
                return;
            }

            var operations = dte2.GetEditorOperations().GetEditorOperations(wpfTextView);

            if (point == caret.ContainingTextViewLine.End) {
                operations.MoveToStartOfNextLineAfterWhiteSpace(extendSelection);
                return;
            }

            var navigator = dte2.GetTextStructureNavigatorSelector().GetTextStructureNavigator(wpfTextView.TextBuffer);
            var extent = navigator.GetExtentOfWord(point);

            for (Int32 i = point; i < extent.Span.End && i < caret.ContainingTextViewLine.End; i++) {
                operations.MoveToNextCharacter(extendSelection);
            }
        }

        public static void StartOfWord (this EnvDTE80.DTE2 dte2, Boolean extendSelection) {
            if (!dte2.TryGetTextView(out var vsTextView)) {
                return;
            }

            var wpfTextView = vsTextView.GetWpfTextView();

            if (!extendSelection && !wpfTextView.Selection.IsEmpty) {
                wpfTextView.Selection.Select(wpfTextView.Selection.ActivePoint, wpfTextView.Selection.ActivePoint);
            }

            var caret = wpfTextView.Caret;
            var operations = dte2.GetEditorOperations().GetEditorOperations(wpfTextView);

            if (caret.InVirtualSpace) {
                operations.MoveToEndOfLine(extendSelection);
                return;
            }

            var point = caret.Position.BufferPosition;

            if (point.Position == 0) {
                return;
            }

            var navigator = dte2.GetTextStructureNavigatorSelector().GetTextStructureNavigator(wpfTextView.TextBuffer);

            if (point == caret.ContainingTextViewLine.Start) {
                operations.MoveLineUp(extendSelection);
                operations.MoveToLastNonWhiteSpaceCharacter(extendSelection);

                if (navigator.GetExtentOfWord(caret.Position.BufferPosition).IsSignificant) {
                    operations.MoveToNextCharacter(extendSelection);
                }

                return;
            }

            var extent = navigator.GetExtentOfWord(point - 1);

            for (Int32 i = point; i > extent.Span.Start; i--) {
                operations.MoveToPreviousCharacter(extendSelection);
            }
        }
    }
}
