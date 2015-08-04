//------------------------------------------------------------------------------
// <copyright file="ExecuteDoxygenMenuItem.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Diagnostics;

namespace CppCommentsCompletion
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExecuteDoxygenMenuItem
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("34dc5f92-6738-4520-83ae-0d71279200a1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly VSPackageMain package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteDoxygenMenuItem"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ExecuteDoxygenMenuItem(VSPackageMain package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.ExecuteDoxygenCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExecuteDoxygenMenuItem Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private System.IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(VSPackageMain package)
        {
            Instance = new ExecuteDoxygenMenuItem(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteDoxygenCallback(object sender, EventArgs e)
        {
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            string solutionDir = dte.Solution.FullName;
            if (solutionDir == "") return;
            solutionDir = System.IO.Path.GetDirectoryName(solutionDir);
            if (solutionDir == "") return;
            
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = "doxygen.exe";
                process.StartInfo.Arguments = solutionDir + "\\DoxyFile";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.Start();
                process.WaitForExit();// Waits here for the process to exit.
            } catch (Exception)
            {

            }

            string message = string.Format(CultureInfo.CurrentCulture, "SolutionDir: {0})", solutionDir);
            string title = "Execute Doxygen";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
