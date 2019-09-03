//ş
using System;
using System.ComponentModel.Design;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;

namespace SSMSEditorEnhancements.Commands.RemoveTrailingSpacesAndSaveDocument {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RemoveTrailingSpacesAndSaveDocumentCommand {
        private static readonly UTF8Encoding _utf8WithoutBom = new UTF8Encoding(false);

        /// <summary>
        /// Command ID.
        /// </summary>
        public const Int32 CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("752d7738-8d2b-44e2-be4c-39239de4facb");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveTrailingSpacesAndSaveDocumentCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RemoveTrailingSpacesAndSaveDocumentCommand (AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);

            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RemoveTrailingSpacesAndSaveDocumentCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync (AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in RemoveTrailingSpacesAndSaveDocumentCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            Instance = new RemoveTrailingSpacesAndSaveDocumentCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute (Object sender, EventArgs e) {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte2 = Util.GetDTE2();

            if (!dte2.TryGetTextView(out var textView)) {
                return;
            }

            textView
                .GetWpfTextView()
                .TextBuffer
                .Properties
                .GetProperty<ITextDocument>(typeof(ITextDocument))
                .Encoding = new UTF8Encoding(false);

            dte2.ActiveDocument.Save();
        }
    }
}
