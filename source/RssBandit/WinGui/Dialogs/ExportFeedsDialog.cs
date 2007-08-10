#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion
using System.Drawing;using System.Collections;using System.Windows.Forms;using RssBandit.WinGui.Controls;
using Infragistics.Win.UltraWinTree;namespace RssBandit.WinGui.Forms{	/// <summary>	/// ExportFeedsDialog.	/// </summary>	public class ExportFeedsDialog : System.Windows.Forms.Form	{        private System.Windows.Forms.Button btnOk;        private System.Windows.Forms.Button btnCancel;		private System.Windows.Forms.ToolTip toolTip1;		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelForCheckedTree;
		internal System.Windows.Forms.TreeView treeExportFeedsSelection;
		private System.Windows.Forms.Label labelFormats;
		internal System.Windows.Forms.RadioButton radioFormatNative;
		internal System.Windows.Forms.RadioButton radioFormatOPML;
		internal System.Windows.Forms.CheckBox checkFormatOPMLIncludeCats;
		internal System.Windows.Forms.CheckBox checkFormatNativeFull;		private System.ComponentModel.IContainer components;				/// <summary>		/// Constructor is private because we always want to populate the tree		/// </summary>		private ExportFeedsDialog()		{			//			// Required for Windows Form Designer support			//			InitializeComponent();		}				public ExportFeedsDialog(TreeFeedsNodeBase exportRootNode, Font treeFont, ImageList treeImages):this()	{			this.treeExportFeedsSelection.Font = treeFont;			this.treeExportFeedsSelection.ImageList = treeImages;			TreeHelper.CopyNodes(exportRootNode, this.treeExportFeedsSelection, true);			if (this.treeExportFeedsSelection.Nodes.Count > 0) {
				this.treeExportFeedsSelection.Nodes[0].Expand();
			}		}				public ArrayList GetSelectedFeedUrls() {			ArrayList result = new ArrayList(100);			if (this.treeExportFeedsSelection.Nodes.Count > 0) {				ArrayList nodes = new ArrayList(100);				TreeHelper.GetCheckedNodes(this.treeExportFeedsSelection.Nodes[0], nodes);				foreach (TreeNode n in nodes) {					if (n.Tag != null)						result.Add((string)n.Tag);				}			}			return result;		}		/// <summary>		/// Clean up any resources being used.		/// </summary>		protected override void Dispose( bool disposing )		{			if( disposing )			{				if(components != null)				{					components.Dispose();				}			}			base.Dispose( disposing );		}		#region Windows Form Designer generated code		/// <summary>		/// Required method for Designer support - do not modify		/// the contents of this method with the code editor.		/// </summary>		private void InitializeComponent()		{			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExportFeedsDialog));
			this.labelForCheckedTree = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label4 = new System.Windows.Forms.Label();
			this.treeExportFeedsSelection = new System.Windows.Forms.TreeView();
			this.labelFormats = new System.Windows.Forms.Label();
			this.radioFormatNative = new System.Windows.Forms.RadioButton();
			this.radioFormatOPML = new System.Windows.Forms.RadioButton();
			this.checkFormatNativeFull = new System.Windows.Forms.CheckBox();
			this.checkFormatOPMLIncludeCats = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// labelForCheckedTree
			// 
			this.labelForCheckedTree.AccessibleDescription = resources.GetString("labelForCheckedTree.AccessibleDescription");
			this.labelForCheckedTree.AccessibleName = resources.GetString("labelForCheckedTree.AccessibleName");
			this.labelForCheckedTree.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelForCheckedTree.Anchor")));
			this.labelForCheckedTree.AutoSize = ((bool)(resources.GetObject("labelForCheckedTree.AutoSize")));
			this.labelForCheckedTree.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelForCheckedTree.Dock")));
			this.labelForCheckedTree.Enabled = ((bool)(resources.GetObject("labelForCheckedTree.Enabled")));
			this.labelForCheckedTree.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelForCheckedTree.Font = ((System.Drawing.Font)(resources.GetObject("labelForCheckedTree.Font")));
			this.labelForCheckedTree.Image = ((System.Drawing.Image)(resources.GetObject("labelForCheckedTree.Image")));
			this.labelForCheckedTree.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelForCheckedTree.ImageAlign")));
			this.labelForCheckedTree.ImageIndex = ((int)(resources.GetObject("labelForCheckedTree.ImageIndex")));
			this.labelForCheckedTree.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelForCheckedTree.ImeMode")));
			this.labelForCheckedTree.Location = ((System.Drawing.Point)(resources.GetObject("labelForCheckedTree.Location")));
			this.labelForCheckedTree.Name = "labelForCheckedTree";
			this.labelForCheckedTree.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelForCheckedTree.RightToLeft")));
			this.labelForCheckedTree.Size = ((System.Drawing.Size)(resources.GetObject("labelForCheckedTree.Size")));
			this.labelForCheckedTree.TabIndex = ((int)(resources.GetObject("labelForCheckedTree.TabIndex")));
			this.labelForCheckedTree.Text = resources.GetString("labelForCheckedTree.Text");
			this.labelForCheckedTree.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelForCheckedTree.TextAlign")));
			this.toolTip1.SetToolTip(this.labelForCheckedTree, resources.GetString("labelForCheckedTree.ToolTip"));
			this.labelForCheckedTree.Visible = ((bool)(resources.GetObject("labelForCheckedTree.Visible")));
			// 
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.toolTip1.SetToolTip(this.btnOk, resources.GetString("btnOk.ToolTip"));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.toolTip1.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// treeExportFeedsSelection
			// 
			this.treeExportFeedsSelection.AccessibleDescription = resources.GetString("treeExportFeedsSelection.AccessibleDescription");
			this.treeExportFeedsSelection.AccessibleName = resources.GetString("treeExportFeedsSelection.AccessibleName");
			this.treeExportFeedsSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("treeExportFeedsSelection.Anchor")));
			this.treeExportFeedsSelection.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("treeExportFeedsSelection.BackgroundImage")));
			this.treeExportFeedsSelection.CheckBoxes = true;
			this.treeExportFeedsSelection.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("treeExportFeedsSelection.Dock")));
			this.treeExportFeedsSelection.Enabled = ((bool)(resources.GetObject("treeExportFeedsSelection.Enabled")));
			this.treeExportFeedsSelection.Font = ((System.Drawing.Font)(resources.GetObject("treeExportFeedsSelection.Font")));
			this.treeExportFeedsSelection.ImageIndex = ((int)(resources.GetObject("treeExportFeedsSelection.ImageIndex")));
			this.treeExportFeedsSelection.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("treeExportFeedsSelection.ImeMode")));
			this.treeExportFeedsSelection.Indent = ((int)(resources.GetObject("treeExportFeedsSelection.Indent")));
			this.treeExportFeedsSelection.ItemHeight = ((int)(resources.GetObject("treeExportFeedsSelection.ItemHeight")));
			this.treeExportFeedsSelection.Location = ((System.Drawing.Point)(resources.GetObject("treeExportFeedsSelection.Location")));
			this.treeExportFeedsSelection.Name = "treeExportFeedsSelection";
			this.treeExportFeedsSelection.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("treeExportFeedsSelection.RightToLeft")));
			this.treeExportFeedsSelection.SelectedImageIndex = ((int)(resources.GetObject("treeExportFeedsSelection.SelectedImageIndex")));
			this.treeExportFeedsSelection.Size = ((System.Drawing.Size)(resources.GetObject("treeExportFeedsSelection.Size")));
			this.treeExportFeedsSelection.TabIndex = ((int)(resources.GetObject("treeExportFeedsSelection.TabIndex")));
			this.treeExportFeedsSelection.Text = resources.GetString("treeExportFeedsSelection.Text");
			this.toolTip1.SetToolTip(this.treeExportFeedsSelection, resources.GetString("treeExportFeedsSelection.ToolTip"));
			this.treeExportFeedsSelection.Visible = ((bool)(resources.GetObject("treeExportFeedsSelection.Visible")));
			this.treeExportFeedsSelection.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeAfterCheck);
			// 
			// labelFormats
			// 
			this.labelFormats.AccessibleDescription = resources.GetString("labelFormats.AccessibleDescription");
			this.labelFormats.AccessibleName = resources.GetString("labelFormats.AccessibleName");
			this.labelFormats.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelFormats.Anchor")));
			this.labelFormats.AutoSize = ((bool)(resources.GetObject("labelFormats.AutoSize")));
			this.labelFormats.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelFormats.Dock")));
			this.labelFormats.Enabled = ((bool)(resources.GetObject("labelFormats.Enabled")));
			this.labelFormats.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelFormats.Font = ((System.Drawing.Font)(resources.GetObject("labelFormats.Font")));
			this.labelFormats.Image = ((System.Drawing.Image)(resources.GetObject("labelFormats.Image")));
			this.labelFormats.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFormats.ImageAlign")));
			this.labelFormats.ImageIndex = ((int)(resources.GetObject("labelFormats.ImageIndex")));
			this.labelFormats.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelFormats.ImeMode")));
			this.labelFormats.Location = ((System.Drawing.Point)(resources.GetObject("labelFormats.Location")));
			this.labelFormats.Name = "labelFormats";
			this.labelFormats.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelFormats.RightToLeft")));
			this.labelFormats.Size = ((System.Drawing.Size)(resources.GetObject("labelFormats.Size")));
			this.labelFormats.TabIndex = ((int)(resources.GetObject("labelFormats.TabIndex")));
			this.labelFormats.Text = resources.GetString("labelFormats.Text");
			this.labelFormats.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFormats.TextAlign")));
			this.toolTip1.SetToolTip(this.labelFormats, resources.GetString("labelFormats.ToolTip"));
			this.labelFormats.Visible = ((bool)(resources.GetObject("labelFormats.Visible")));
			// 
			// radioFormatNative
			// 
			this.radioFormatNative.AccessibleDescription = resources.GetString("radioFormatNative.AccessibleDescription");
			this.radioFormatNative.AccessibleName = resources.GetString("radioFormatNative.AccessibleName");
			this.radioFormatNative.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioFormatNative.Anchor")));
			this.radioFormatNative.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioFormatNative.Appearance")));
			this.radioFormatNative.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioFormatNative.BackgroundImage")));
			this.radioFormatNative.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatNative.CheckAlign")));
			this.radioFormatNative.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioFormatNative.Dock")));
			this.radioFormatNative.Enabled = ((bool)(resources.GetObject("radioFormatNative.Enabled")));
			this.radioFormatNative.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioFormatNative.FlatStyle")));
			this.radioFormatNative.Font = ((System.Drawing.Font)(resources.GetObject("radioFormatNative.Font")));
			this.radioFormatNative.Image = ((System.Drawing.Image)(resources.GetObject("radioFormatNative.Image")));
			this.radioFormatNative.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatNative.ImageAlign")));
			this.radioFormatNative.ImageIndex = ((int)(resources.GetObject("radioFormatNative.ImageIndex")));
			this.radioFormatNative.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioFormatNative.ImeMode")));
			this.radioFormatNative.Location = ((System.Drawing.Point)(resources.GetObject("radioFormatNative.Location")));
			this.radioFormatNative.Name = "radioFormatNative";
			this.radioFormatNative.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioFormatNative.RightToLeft")));
			this.radioFormatNative.Size = ((System.Drawing.Size)(resources.GetObject("radioFormatNative.Size")));
			this.radioFormatNative.TabIndex = ((int)(resources.GetObject("radioFormatNative.TabIndex")));
			this.radioFormatNative.Text = resources.GetString("radioFormatNative.Text");
			this.radioFormatNative.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatNative.TextAlign")));
			this.toolTip1.SetToolTip(this.radioFormatNative, resources.GetString("radioFormatNative.ToolTip"));
			this.radioFormatNative.Visible = ((bool)(resources.GetObject("radioFormatNative.Visible")));
			this.radioFormatNative.CheckedChanged += new System.EventHandler(this.OnRadioFormatCheckedChanged);
			// 
			// radioFormatOPML
			// 
			this.radioFormatOPML.AccessibleDescription = resources.GetString("radioFormatOPML.AccessibleDescription");
			this.radioFormatOPML.AccessibleName = resources.GetString("radioFormatOPML.AccessibleName");
			this.radioFormatOPML.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioFormatOPML.Anchor")));
			this.radioFormatOPML.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioFormatOPML.Appearance")));
			this.radioFormatOPML.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioFormatOPML.BackgroundImage")));
			this.radioFormatOPML.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatOPML.CheckAlign")));
			this.radioFormatOPML.Checked = true;
			this.radioFormatOPML.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioFormatOPML.Dock")));
			this.radioFormatOPML.Enabled = ((bool)(resources.GetObject("radioFormatOPML.Enabled")));
			this.radioFormatOPML.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioFormatOPML.FlatStyle")));
			this.radioFormatOPML.Font = ((System.Drawing.Font)(resources.GetObject("radioFormatOPML.Font")));
			this.radioFormatOPML.Image = ((System.Drawing.Image)(resources.GetObject("radioFormatOPML.Image")));
			this.radioFormatOPML.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatOPML.ImageAlign")));
			this.radioFormatOPML.ImageIndex = ((int)(resources.GetObject("radioFormatOPML.ImageIndex")));
			this.radioFormatOPML.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioFormatOPML.ImeMode")));
			this.radioFormatOPML.Location = ((System.Drawing.Point)(resources.GetObject("radioFormatOPML.Location")));
			this.radioFormatOPML.Name = "radioFormatOPML";
			this.radioFormatOPML.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioFormatOPML.RightToLeft")));
			this.radioFormatOPML.Size = ((System.Drawing.Size)(resources.GetObject("radioFormatOPML.Size")));
			this.radioFormatOPML.TabIndex = ((int)(resources.GetObject("radioFormatOPML.TabIndex")));
			this.radioFormatOPML.TabStop = true;
			this.radioFormatOPML.Text = resources.GetString("radioFormatOPML.Text");
			this.radioFormatOPML.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioFormatOPML.TextAlign")));
			this.toolTip1.SetToolTip(this.radioFormatOPML, resources.GetString("radioFormatOPML.ToolTip"));
			this.radioFormatOPML.Visible = ((bool)(resources.GetObject("radioFormatOPML.Visible")));
			this.radioFormatOPML.CheckedChanged += new System.EventHandler(this.OnRadioFormatCheckedChanged);
			// 
			// checkFormatNativeFull
			// 
			this.checkFormatNativeFull.AccessibleDescription = resources.GetString("checkFormatNativeFull.AccessibleDescription");
			this.checkFormatNativeFull.AccessibleName = resources.GetString("checkFormatNativeFull.AccessibleName");
			this.checkFormatNativeFull.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkFormatNativeFull.Anchor")));
			this.checkFormatNativeFull.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkFormatNativeFull.Appearance")));
			this.checkFormatNativeFull.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkFormatNativeFull.BackgroundImage")));
			this.checkFormatNativeFull.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatNativeFull.CheckAlign")));
			this.checkFormatNativeFull.Checked = true;
			this.checkFormatNativeFull.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkFormatNativeFull.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkFormatNativeFull.Dock")));
			this.checkFormatNativeFull.Enabled = ((bool)(resources.GetObject("checkFormatNativeFull.Enabled")));
			this.checkFormatNativeFull.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkFormatNativeFull.FlatStyle")));
			this.checkFormatNativeFull.Font = ((System.Drawing.Font)(resources.GetObject("checkFormatNativeFull.Font")));
			this.checkFormatNativeFull.Image = ((System.Drawing.Image)(resources.GetObject("checkFormatNativeFull.Image")));
			this.checkFormatNativeFull.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatNativeFull.ImageAlign")));
			this.checkFormatNativeFull.ImageIndex = ((int)(resources.GetObject("checkFormatNativeFull.ImageIndex")));
			this.checkFormatNativeFull.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkFormatNativeFull.ImeMode")));
			this.checkFormatNativeFull.Location = ((System.Drawing.Point)(resources.GetObject("checkFormatNativeFull.Location")));
			this.checkFormatNativeFull.Name = "checkFormatNativeFull";
			this.checkFormatNativeFull.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkFormatNativeFull.RightToLeft")));
			this.checkFormatNativeFull.Size = ((System.Drawing.Size)(resources.GetObject("checkFormatNativeFull.Size")));
			this.checkFormatNativeFull.TabIndex = ((int)(resources.GetObject("checkFormatNativeFull.TabIndex")));
			this.checkFormatNativeFull.Text = resources.GetString("checkFormatNativeFull.Text");
			this.checkFormatNativeFull.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatNativeFull.TextAlign")));
			this.toolTip1.SetToolTip(this.checkFormatNativeFull, resources.GetString("checkFormatNativeFull.ToolTip"));
			this.checkFormatNativeFull.Visible = ((bool)(resources.GetObject("checkFormatNativeFull.Visible")));
			// 
			// checkFormatOPMLIncludeCats
			// 
			this.checkFormatOPMLIncludeCats.AccessibleDescription = resources.GetString("checkFormatOPMLIncludeCats.AccessibleDescription");
			this.checkFormatOPMLIncludeCats.AccessibleName = resources.GetString("checkFormatOPMLIncludeCats.AccessibleName");
			this.checkFormatOPMLIncludeCats.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkFormatOPMLIncludeCats.Anchor")));
			this.checkFormatOPMLIncludeCats.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkFormatOPMLIncludeCats.Appearance")));
			this.checkFormatOPMLIncludeCats.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkFormatOPMLIncludeCats.BackgroundImage")));
			this.checkFormatOPMLIncludeCats.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatOPMLIncludeCats.CheckAlign")));
			this.checkFormatOPMLIncludeCats.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkFormatOPMLIncludeCats.Dock")));
			this.checkFormatOPMLIncludeCats.Enabled = ((bool)(resources.GetObject("checkFormatOPMLIncludeCats.Enabled")));
			this.checkFormatOPMLIncludeCats.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkFormatOPMLIncludeCats.FlatStyle")));
			this.checkFormatOPMLIncludeCats.Font = ((System.Drawing.Font)(resources.GetObject("checkFormatOPMLIncludeCats.Font")));
			this.checkFormatOPMLIncludeCats.Image = ((System.Drawing.Image)(resources.GetObject("checkFormatOPMLIncludeCats.Image")));
			this.checkFormatOPMLIncludeCats.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatOPMLIncludeCats.ImageAlign")));
			this.checkFormatOPMLIncludeCats.ImageIndex = ((int)(resources.GetObject("checkFormatOPMLIncludeCats.ImageIndex")));
			this.checkFormatOPMLIncludeCats.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkFormatOPMLIncludeCats.ImeMode")));
			this.checkFormatOPMLIncludeCats.Location = ((System.Drawing.Point)(resources.GetObject("checkFormatOPMLIncludeCats.Location")));
			this.checkFormatOPMLIncludeCats.Name = "checkFormatOPMLIncludeCats";
			this.checkFormatOPMLIncludeCats.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkFormatOPMLIncludeCats.RightToLeft")));
			this.checkFormatOPMLIncludeCats.Size = ((System.Drawing.Size)(resources.GetObject("checkFormatOPMLIncludeCats.Size")));
			this.checkFormatOPMLIncludeCats.TabIndex = ((int)(resources.GetObject("checkFormatOPMLIncludeCats.TabIndex")));
			this.checkFormatOPMLIncludeCats.Text = resources.GetString("checkFormatOPMLIncludeCats.Text");
			this.checkFormatOPMLIncludeCats.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkFormatOPMLIncludeCats.TextAlign")));
			this.toolTip1.SetToolTip(this.checkFormatOPMLIncludeCats, resources.GetString("checkFormatOPMLIncludeCats.ToolTip"));
			this.checkFormatOPMLIncludeCats.Visible = ((bool)(resources.GetObject("checkFormatOPMLIncludeCats.Visible")));
			// 
			// ExportFeedsDialog
			// 
			this.AcceptButton = this.btnOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.checkFormatOPMLIncludeCats);
			this.Controls.Add(this.checkFormatNativeFull);
			this.Controls.Add(this.radioFormatOPML);
			this.Controls.Add(this.radioFormatNative);
			this.Controls.Add(this.labelFormats);
			this.Controls.Add(this.treeExportFeedsSelection);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.labelForCheckedTree);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ExportFeedsDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.ResumeLayout(false);

		}		#endregion	
		private void OnTreeAfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse) {
				TreeHelper.PerformOnCheckStateChanged(e.Node);
			}
		}

		private void OnRadioFormatCheckedChanged(object sender, System.EventArgs e) {
			checkFormatNativeFull.Enabled = radioFormatNative.Checked;
			checkFormatOPMLIncludeCats.Enabled = radioFormatOPML.Checked;
		}	}}