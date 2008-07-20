

namespace RssBandit.WinGui.Dialogs
{
	partial class ExportFeedsDialog
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportFeedsDialog));
            this.labelForCheckedTree = new System.Windows.Forms.Label();
            this.radioFormatNative = new System.Windows.Forms.RadioButton();
            this.radioFormatOPML = new System.Windows.Forms.RadioButton();
            this.treeExportFeedsSelection = new System.Windows.Forms.TreeView();
            this.checkFormatNativeFull = new System.Windows.Forms.CheckBox();
            this.checkFormatOPMLIncludeCats = new System.Windows.Forms.CheckBox();
            this.grpFeedSource = new System.Windows.Forms.GroupBox();
            this.cboFeedSources = new System.Windows.Forms.ComboBox();
            this.grpFormat = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.grpFeedSource.SuspendLayout();
            this.grpFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(392, 249);
            // 
            // horizontalEdge
            // 
            this.horizontalEdge.Location = new System.Drawing.Point(-1, 237);
            this.horizontalEdge.Size = new System.Drawing.Size(502, 2);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(292, 249);
            // 
            // labelForCheckedTree
            // 
            this.labelForCheckedTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelForCheckedTree.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelForCheckedTree.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelForCheckedTree.Location = new System.Drawing.Point(8, 59);
            this.labelForCheckedTree.Name = "labelForCheckedTree";
            this.labelForCheckedTree.Size = new System.Drawing.Size(231, 16);
            this.labelForCheckedTree.TabIndex = 1;
            this.labelForCheckedTree.Text = "Select the feed(s) to &export:";
            this.labelForCheckedTree.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // radioFormatNative
            // 
            this.radioFormatNative.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioFormatNative.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioFormatNative.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioFormatNative.Location = new System.Drawing.Point(7, 23);
            this.radioFormatNative.Name = "radioFormatNative";
            this.radioFormatNative.Size = new System.Drawing.Size(202, 24);
            this.radioFormatNative.TabIndex = 0;
            this.radioFormatNative.Text = "&Native";
            this.radioFormatNative.CheckedChanged += new System.EventHandler(this.OnRadioFormatCheckedChanged);
            // 
            // radioFormatOPML
            // 
            this.radioFormatOPML.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioFormatOPML.Checked = true;
            this.radioFormatOPML.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioFormatOPML.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioFormatOPML.Location = new System.Drawing.Point(7, 78);
            this.radioFormatOPML.Name = "radioFormatOPML";
            this.radioFormatOPML.Size = new System.Drawing.Size(202, 24);
            this.radioFormatOPML.TabIndex = 2;
            this.radioFormatOPML.TabStop = true;
            this.radioFormatOPML.Text = "&OPML";
            this.radioFormatOPML.CheckedChanged += new System.EventHandler(this.OnRadioFormatCheckedChanged);
            // 
            // treeExportFeedsSelection
            // 
            this.treeExportFeedsSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeExportFeedsSelection.CheckBoxes = true;
            this.treeExportFeedsSelection.Indent = 19;
            this.treeExportFeedsSelection.ItemHeight = 16;
            this.treeExportFeedsSelection.Location = new System.Drawing.Point(8, 78);
            this.treeExportFeedsSelection.Name = "treeExportFeedsSelection";
            this.treeExportFeedsSelection.Size = new System.Drawing.Size(234, 130);
            this.treeExportFeedsSelection.TabIndex = 2;
            this.treeExportFeedsSelection.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeAfterCheck);
            // 
            // checkFormatNativeFull
            // 
            this.checkFormatNativeFull.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkFormatNativeFull.Checked = true;
            this.checkFormatNativeFull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFormatNativeFull.Enabled = false;
            this.checkFormatNativeFull.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkFormatNativeFull.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkFormatNativeFull.Location = new System.Drawing.Point(27, 44);
            this.checkFormatNativeFull.Name = "checkFormatNativeFull";
            this.checkFormatNativeFull.Size = new System.Drawing.Size(182, 36);
            this.checkFormatNativeFull.TabIndex = 1;
            this.checkFormatNativeFull.Text = "Include item &read states";
            // 
            // checkFormatOPMLIncludeCats
            // 
            this.checkFormatOPMLIncludeCats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkFormatOPMLIncludeCats.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkFormatOPMLIncludeCats.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkFormatOPMLIncludeCats.Location = new System.Drawing.Point(27, 100);
            this.checkFormatOPMLIncludeCats.Name = "checkFormatOPMLIncludeCats";
            this.checkFormatOPMLIncludeCats.Size = new System.Drawing.Size(182, 36);
            this.checkFormatOPMLIncludeCats.TabIndex = 3;
            this.checkFormatOPMLIncludeCats.Text = "Include empty &categories";
            // 
            // grpFeedSource
            // 
            this.grpFeedSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFeedSource.Controls.Add(this.cboFeedSources);
            this.grpFeedSource.Controls.Add(this.labelForCheckedTree);
            this.grpFeedSource.Controls.Add(this.treeExportFeedsSelection);
            this.grpFeedSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpFeedSource.Location = new System.Drawing.Point(12, 12);
            this.grpFeedSource.Name = "grpFeedSource";
            this.grpFeedSource.Size = new System.Drawing.Size(252, 216);
            this.grpFeedSource.TabIndex = 0;
            this.grpFeedSource.TabStop = false;
            this.grpFeedSource.Text = "From Feed Source";
            // 
            // cboFeedSources
            // 
            this.cboFeedSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFeedSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFeedSources.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cboFeedSources.FormattingEnabled = true;
            this.cboFeedSources.Location = new System.Drawing.Point(7, 25);
            this.cboFeedSources.Name = "cboFeedSources";
            this.cboFeedSources.Size = new System.Drawing.Size(235, 21);
            this.cboFeedSources.TabIndex = 0;
            this.cboFeedSources.SelectedIndexChanged += new System.EventHandler(this.cboFeedSources_SelectedIndexChanged);
            // 
            // grpFormat
            // 
            this.grpFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFormat.Controls.Add(this.checkFormatOPMLIncludeCats);
            this.grpFormat.Controls.Add(this.radioFormatNative);
            this.grpFormat.Controls.Add(this.radioFormatOPML);
            this.grpFormat.Controls.Add(this.checkFormatNativeFull);
            this.grpFormat.Location = new System.Drawing.Point(271, 12);
            this.grpFormat.Name = "grpFormat";
            this.grpFormat.Size = new System.Drawing.Size(215, 216);
            this.grpFormat.TabIndex = 1;
            this.grpFormat.TabStop = false;
            this.grpFormat.Text = "Format";
            // 
            // ExportFeedsDialog
            // 
            this.AcceptButton = null;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = null;
            this.ClientSize = new System.Drawing.Size(495, 286);
            this.Controls.Add(this.grpFormat);
            this.Controls.Add(this.grpFeedSource);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 315);
            this.Name = "ExportFeedsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Feeds";
            this.Controls.SetChildIndex(this.grpFeedSource, 0);
            this.Controls.SetChildIndex(this.grpFormat, 0);
            this.Controls.SetChildIndex(this.btnSubmit, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.horizontalEdge, 0);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.grpFeedSource.ResumeLayout(false);
            this.grpFormat.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion	
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.GroupBox grpFeedSource;
		private System.Windows.Forms.GroupBox grpFormat;
		private System.Windows.Forms.Label labelForCheckedTree;
		
	}
}
