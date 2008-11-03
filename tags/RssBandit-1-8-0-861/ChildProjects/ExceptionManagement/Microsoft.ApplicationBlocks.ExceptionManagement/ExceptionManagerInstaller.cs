using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Resources;
using System.Reflection;

namespace Microsoft.ApplicationBlocks.ExceptionManagement
{
	/// <summary>
	/// Installer class used to create two event sources for the 
	/// Exception Management Application Block to function correctly.
	/// </summary>
	[RunInstaller(true)]
	public class ExceptionManagerInstaller : System.Configuration.Install.Installer
	{
		private System.Diagnostics.EventLogInstaller exceptionManagerEventLogInstaller;
		private System.Diagnostics.EventLogInstaller exceptionManagementEventLogInstaller;
		
		private static ResourceManager resourceManager = new ResourceManager(typeof(ExceptionManager).Namespace + ".ExceptionManagerText",Assembly.GetAssembly(typeof(ExceptionManager)));
		
		/// <summary>
		/// Constructor with no params.
		/// </summary>
		public ExceptionManagerInstaller()
		{
			// Initialize variables.
			InitializeComponent();
		}

		/// <summary>
		/// Initialization function to set internal variables.
		/// </summary>
		private void InitializeComponent()
		{
			this.exceptionManagerEventLogInstaller = new System.Diagnostics.EventLogInstaller();
			this.exceptionManagementEventLogInstaller = new System.Diagnostics.EventLogInstaller();
			// 
			// exceptionManagerEventLogInstaller
			// 
			this.exceptionManagerEventLogInstaller.Log = "Application";
			this.exceptionManagerEventLogInstaller.Source = resourceManager.GetString("RES_EXCEPTIONMANAGER_INTERNAL_EXCEPTIONS");
			// 
			// exceptionManagementEventLogInstaller
			// 
			this.exceptionManagementEventLogInstaller.Log = "Application";
			this.exceptionManagementEventLogInstaller.Source = resourceManager.GetString("RES_EXCEPTIONMANAGER_PUBLISHED_EXCEPTIONS");

			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.exceptionManagerEventLogInstaller,
																					  this.exceptionManagementEventLogInstaller});
		}
	}
}

