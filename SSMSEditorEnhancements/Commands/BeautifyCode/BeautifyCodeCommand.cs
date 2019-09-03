//ş
using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;

namespace SSMSEditorEnhancements.Commands.BeautifyCode {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BeautifyCodeCommand {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const Int32 CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("752d7738-8d2b-44e2-be4c-39239de4facb");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeautifyCodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BeautifyCodeCommand (AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);

            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BeautifyCodeCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync (AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in BeautifyCodeCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            Instance = new BeautifyCodeCommand(package, commandService);
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

            ReplaceSquareBracketsWithDoubleQuotes(dte2);
        }

        private void ReplaceSquareBracketsWithDoubleQuotes (DTE2 dte2) {
            ThreadHelper.ThrowIfNotOnUIThread();

            var options = (Int32) (
                EnvDTE.vsFindOptions.vsFindOptionsRegularExpression |
                EnvDTE.vsFindOptions.vsFindOptionsMatchCase |
                EnvDTE.vsFindOptions.vsFindOptionsMatchInHiddenText
            );

            void regexReplace (String target, String replacement) {
                dte2.DTE.Find.FindReplace(
                    Action: EnvDTE.vsFindAction.vsFindActionReplaceAll,
                    FindWhat: target,
                    vsFindOptionsValue: options,
                    ReplaceWith: replacement,
                    Target: EnvDTE.vsFindTarget.vsFindTargetCurrentDocument,
                    ResultsLocation: EnvDTE.vsFindResultsLocation.vsFindResultsNone
                );
            }

            void regexReplaceWithLowerCasedString (String str) {
                regexReplace("^" + str, str.ToLowerInvariant());
            }

            regexReplace(@"[\[\]]", "\"");
            regexReplace(@"(\r\n)+$(?![\r\n])", "\r\n");
            regexReplaceWithLowerCasedString("ALTER");
            regexReplaceWithLowerCasedString("CREATE");
            regexReplaceWithLowerCasedString("DELETE");
            regexReplaceWithLowerCasedString("INSERT");
            regexReplaceWithLowerCasedString("SELECT");

            if (!dte2.TryGetTextView(out var textView)) {
                return;
            }

            RemoveTrailingWhitespace(textView.GetWpfTextView().TextBuffer);
        }

        private static void RemoveTrailingWhitespace (ITextBuffer buffer) {
            using (var edit = buffer.CreateEdit()) {
                foreach (var line in edit.Snapshot.Lines) {
                    var text = line.GetText();
                    var length = text.Length;

                    while (--length >= 0 && Char.IsWhiteSpace(text[length]));

                    if (length < text.Length - 1) {
                        var start = line.Start.Position;

                        edit.Delete(start + length + 1, text.Length - length - 1);
                    }
                }

                edit.Apply();
            }
        }
    }
}
