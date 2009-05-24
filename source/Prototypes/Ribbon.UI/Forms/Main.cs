#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using Ribbon.WindowsApplication;

namespace RssBandit.UI.Forms
{
	/// <summary>
	/// Summary description for Main.
	/// </summary>
	internal partial class Main : System.Windows.Forms.Form
	{
		
		public Main()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		private void OnFormClosed(object sender, System.EventArgs e) {
		
		}

		private void OnFormLoad(object sender, System.EventArgs e) {
			this.BackColor = Office2007ColorTable.Colors.StatusBarGradientLight;
			
			// Don't fire any toolbar events during initialization
			this.ultraToolbarsManager.EventManager.AllEventsEnabled = false;

			// Set-up color scheme options for use in the ApplicationMenu on the Ribbon
			ListTool list = (ListTool)this.ultraToolbarsManager.Tools["cmdColorSchemeList"];
			list.ListToolItems["Blue"].Value = Office2007ColorScheme.Blue;
			list.ListToolItems["Black"].Value = Office2007ColorScheme.Black;
			list.ListToolItems["Silver"].Value = Office2007ColorScheme.Silver;
			list.SelectedItemIndex = list.ListToolItems["Blue"].Index;

			this.ultraToolbarsManager.EventManager.AllEventsEnabled = true;

		}

		private void OnToolbarToolClick(object sender, ToolClickEventArgs e) {
			switch (e.Tool.Key) {
				case "cmdNewFeedSubscription":
					using (IG_Experiments f = new IG_Experiments())
						f.ShowDialog(this);
					break;
				case "cmdApplyFormatting":    // ButtonTool
					// Place code here
					break;

				case "cmdDefaultStylesheetGalery":    // PopupGalleryTool
					// Place code here
					break;

				case "cmdCustomStylesheetGalery":    // PopupGalleryTool
					// Place code here
					break;

				case "cmdAppExit":    // ButtonTool
					this.Close();
					break;

				case "cmdColorSchemeList":    // ListTool
					ListToolItem item = ((ListTool)e.Tool).SelectedItem;
					if(item != null)
						Office2007ColorTable.ColorScheme = (Office2007ColorScheme)item.Value;
					break;

				case "ColorSchemeLabel":    // LabelTool
					// Place code here
					break;

			}

		}

		private void OnBeforeNodeActivate(object sender, Infragistics.Win.UltraWinTree.CancelableNodeEventArgs e)
		{
			foreach (RibbonTab ctg in ultraToolbarsManager.Ribbon.Tabs)
				if (ctg.ContextualTabGroup != null)
					ctg.Visible = false;
			if (e.TreeNode.Tag.ToString() == "F")
				ultraToolbarsManager.Ribbon.Tabs["ribFeeds"].Visible = true;
			if (e.TreeNode.Tag.ToString() == "C")
				ultraToolbarsManager.Ribbon.Tabs["ribCategory"].Visible = true;
		}

	}
}