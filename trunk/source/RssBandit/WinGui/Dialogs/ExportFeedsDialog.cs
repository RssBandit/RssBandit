#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Drawing;using System.Collections;using System.Windows.Forms;using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui.Dialogs{	/// <summary>	/// ExportFeedsDialog.	/// </summary>	public partial class ExportFeedsDialog : DialogBase
	{
		internal TreeView treeExportFeedsSelection;
		internal RadioButton radioFormatNative;
		internal RadioButton radioFormatOPML;
		internal CheckBox checkFormatOPMLIncludeCats;
		internal CheckBox checkFormatNativeFull;
		internal ComboBox cboFeedSources;
				/// <summary>		/// Constructor is private because we always want to populate the tree		/// </summary>		private ExportFeedsDialog()		{			//			// Required for Windows Form Designer support			//			InitializeComponent();			ApplyComponentTranslations();		}
		
		public ExportFeedsDialog(TreeFeedsNodeBase exportRootNode, Font treeFont, ImageList treeImages)
			: this()
		{
			this.treeExportFeedsSelection.Font = treeFont;
			this.treeExportFeedsSelection.ImageList = treeImages;
			TreeHelper.CopyNodes(exportRootNode, this.treeExportFeedsSelection, true);
			if (this.treeExportFeedsSelection.Nodes.Count > 0)
			{
				this.treeExportFeedsSelection.Nodes[0].Expand();
			}
		}

		#region Localization
		
		protected override void InitializeComponentTranslation()
		{
			base.InitializeComponentTranslation();
			
			this.Text = DR.ExportFeedsDialog_Text;
			btnSubmit.Text = DR.ExportFeedsDialog_btnOK_Text;
			toolTip.SetToolTip(btnSubmit, DR.ExportFeedsDialog_btnOK_Tip);

			grpFeedSource.Text = DR.ExportFeedsDialog_grpFromFeedSource_Text;
			labelForCheckedTree.Text = DR.ExportFeedsDialog_labelForCheckedTree_Text;
			
			grpFormat.Text = DR.ExportFeedsDialog_labelFormats_Text;
			radioFormatNative.Text = DR.ExportFeedsDialog_radioFormatNative_Text;
			toolTip.SetToolTip(radioFormatNative, DR.ExportFeedsDialog_radioFormatNative_Tip);
			radioFormatOPML.Text = DR.ExportFeedsDialog_radioFormatOPML_Text;
			toolTip.SetToolTip(radioFormatOPML, DR.ExportFeedsDialog_radioFormatOPML_Tip);
			checkFormatNativeFull.Text = DR.ExportFeedsDialog_checkFormatNativeFull_Text;
			checkFormatOPMLIncludeCats.Text = DR.ExportFeedsDialog_checkFormatOPMLIncludeCats_Text;
		}
		#endregion
		public ArrayList GetSelectedFeedUrls() {			ArrayList result = new ArrayList(100);			if (this.treeExportFeedsSelection.Nodes.Count > 0) {				ArrayList nodes = new ArrayList(100);				TreeHelper.GetCheckedNodes(this.treeExportFeedsSelection.Nodes[0], nodes);				foreach (TreeNode n in nodes) {					if (n.Tag != null)						result.Add(n.Tag);				}			}			return result;		}		/// <summary>		/// Clean up any resources being used.		/// </summary>		protected override void Dispose( bool disposing )		{			if( disposing )			{				if(components != null)				{					components.Dispose();				}			}			base.Dispose( disposing );		}
		private void OnTreeAfterCheck(object sender, TreeViewEventArgs e) {
			if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse) {
				TreeHelper.PerformOnCheckStateChanged(e.Node);
			}
		}

		private void OnRadioFormatCheckedChanged(object sender, System.EventArgs e) {
			checkFormatNativeFull.Enabled = radioFormatNative.Checked;
			checkFormatOPMLIncludeCats.Enabled = radioFormatOPML.Checked;
		}	}}