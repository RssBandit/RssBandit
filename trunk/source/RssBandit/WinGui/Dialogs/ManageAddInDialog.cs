#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Forms;

using RssBandit.UIServices;
using RssBandit.Resources;
using System.Collections.Generic;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// ManageAddInDialog: as the name indicates
	/// </summary>
	public class ManageAddInDialog : Form
	{

		private readonly IAddInManager manager;
		private readonly IServiceProvider serviceProvider;

		private System.Windows.Forms.Label lblAddInListCaption;
		private System.Windows.Forms.ListView lstLoadedAddIns;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colLocation;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Button btnConfigure;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ManageAddInDialog(IServiceProvider serviceProvider)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// dynamically enabled/disabled, if the selected AddIn implements
			// the IAddInPackageConfiguration interface
			this.btnConfigure.Enabled = false;

			this.serviceProvider = serviceProvider;
			this.manager = (IAddInManager)serviceProvider.GetService(typeof(IAddInManager));
			this.PopulateAddInList(manager.AddIns);
			
			OnAddInListItemActivate(this, EventArgs.Empty);
		}

		private void PopulateAddInList(IEnumerable<IAddIn> addIns) {
			if (addIns == null)
				return;

			foreach (IAddIn addIn in addIns) {
				ListViewItem item = this.lstLoadedAddIns.Items.Add(new ListViewItem(new string[]{addIn.Name, addIn.Location}));
				item.Tag = addIn;
			}
			
			if (this.lstLoadedAddIns.Items.Count > 0) {
				this.lstLoadedAddIns.Columns[0].Width = -1;
				this.lstLoadedAddIns.Columns[1].Width = -1;
			}
		}

		private bool AddInHasConfigurationUI () {
			if (this.lstLoadedAddIns.SelectedItems.Count > 0) {
				ListViewItem selected = this.lstLoadedAddIns.SelectedItems[0];
				try {
					IAddIn addIn = (IAddIn)selected.Tag;
					foreach (IAddInPackage package in addIn.AddInPackages) {
						IAddInPackageConfiguration cfg = package as IAddInPackageConfiguration;
						if (cfg != null && cfg.HasConfigurationUI) {
							return true;
						}
					}
					
				}catch (Exception ex) {
					MessageBox.Show(this, String.Format(SR.AddInGeneralFailure,ex.Message, selected.SubItems[0].Text));
				}
			}
			return false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ManageAddInDialog));
			this.lblAddInListCaption = new System.Windows.Forms.Label();
			this.lstLoadedAddIns = new System.Windows.Forms.ListView();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.colLocation = new System.Windows.Forms.ColumnHeader();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.btnConfigure = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblAddInListCaption
			// 
			this.lblAddInListCaption.AccessibleDescription = resources.GetString("lblAddInListCaption.AccessibleDescription");
			this.lblAddInListCaption.AccessibleName = resources.GetString("lblAddInListCaption.AccessibleName");
			this.lblAddInListCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblAddInListCaption.Anchor")));
			this.lblAddInListCaption.AutoSize = ((bool)(resources.GetObject("lblAddInListCaption.AutoSize")));
			this.lblAddInListCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblAddInListCaption.Dock")));
			this.lblAddInListCaption.Enabled = ((bool)(resources.GetObject("lblAddInListCaption.Enabled")));
			this.lblAddInListCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblAddInListCaption.Font = ((System.Drawing.Font)(resources.GetObject("lblAddInListCaption.Font")));
			this.lblAddInListCaption.Image = ((System.Drawing.Image)(resources.GetObject("lblAddInListCaption.Image")));
			this.lblAddInListCaption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAddInListCaption.ImageAlign")));
			this.lblAddInListCaption.ImageIndex = ((int)(resources.GetObject("lblAddInListCaption.ImageIndex")));
			this.lblAddInListCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblAddInListCaption.ImeMode")));
			this.lblAddInListCaption.Location = ((System.Drawing.Point)(resources.GetObject("lblAddInListCaption.Location")));
			this.lblAddInListCaption.Name = "lblAddInListCaption";
			this.lblAddInListCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblAddInListCaption.RightToLeft")));
			this.lblAddInListCaption.Size = ((System.Drawing.Size)(resources.GetObject("lblAddInListCaption.Size")));
			this.lblAddInListCaption.TabIndex = ((int)(resources.GetObject("lblAddInListCaption.TabIndex")));
			this.lblAddInListCaption.Text = resources.GetString("lblAddInListCaption.Text");
			this.lblAddInListCaption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAddInListCaption.TextAlign")));
			this.lblAddInListCaption.Visible = ((bool)(resources.GetObject("lblAddInListCaption.Visible")));
			// 
			// lstLoadedAddIns
			// 
			this.lstLoadedAddIns.AccessibleDescription = resources.GetString("lstLoadedAddIns.AccessibleDescription");
			this.lstLoadedAddIns.AccessibleName = resources.GetString("lstLoadedAddIns.AccessibleName");
			this.lstLoadedAddIns.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.lstLoadedAddIns.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("lstLoadedAddIns.Alignment")));
			this.lstLoadedAddIns.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstLoadedAddIns.Anchor")));
			this.lstLoadedAddIns.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstLoadedAddIns.BackgroundImage")));
			this.lstLoadedAddIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lstLoadedAddIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							  this.colName,
																							  this.colLocation});
			this.lstLoadedAddIns.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstLoadedAddIns.Dock")));
			this.lstLoadedAddIns.Enabled = ((bool)(resources.GetObject("lstLoadedAddIns.Enabled")));
			this.lstLoadedAddIns.Font = ((System.Drawing.Font)(resources.GetObject("lstLoadedAddIns.Font")));
			this.lstLoadedAddIns.FullRowSelect = true;
			this.lstLoadedAddIns.HideSelection = false;
			this.lstLoadedAddIns.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstLoadedAddIns.ImeMode")));
			this.lstLoadedAddIns.LabelWrap = ((bool)(resources.GetObject("lstLoadedAddIns.LabelWrap")));
			this.lstLoadedAddIns.Location = ((System.Drawing.Point)(resources.GetObject("lstLoadedAddIns.Location")));
			this.lstLoadedAddIns.MultiSelect = false;
			this.lstLoadedAddIns.Name = "lstLoadedAddIns";
			this.lstLoadedAddIns.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstLoadedAddIns.RightToLeft")));
			this.lstLoadedAddIns.Size = ((System.Drawing.Size)(resources.GetObject("lstLoadedAddIns.Size")));
			this.lstLoadedAddIns.TabIndex = ((int)(resources.GetObject("lstLoadedAddIns.TabIndex")));
			this.lstLoadedAddIns.Text = resources.GetString("lstLoadedAddIns.Text");
			this.lstLoadedAddIns.View = System.Windows.Forms.View.Details;
			this.lstLoadedAddIns.Visible = ((bool)(resources.GetObject("lstLoadedAddIns.Visible")));
			this.lstLoadedAddIns.ItemActivate += new System.EventHandler(this.OnAddInListItemActivate);
			this.lstLoadedAddIns.SelectedIndexChanged += new System.EventHandler(this.OnAddInListSelectedIndexChanged);
			// 
			// colName
			// 
			this.colName.Text = resources.GetString("colName.Text");
			this.colName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colName.TextAlign")));
			this.colName.Width = ((int)(resources.GetObject("colName.Width")));
			// 
			// colLocation
			// 
			this.colLocation.Text = resources.GetString("colLocation.Text");
			this.colLocation.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colLocation.TextAlign")));
			this.colLocation.Width = ((int)(resources.GetObject("colLocation.Width")));
			// 
			// btnAdd
			// 
			this.btnAdd.AccessibleDescription = resources.GetString("btnAdd.AccessibleDescription");
			this.btnAdd.AccessibleName = resources.GetString("btnAdd.AccessibleName");
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnAdd.Anchor")));
			this.btnAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAdd.BackgroundImage")));
			this.btnAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnAdd.Dock")));
			this.btnAdd.Enabled = ((bool)(resources.GetObject("btnAdd.Enabled")));
			this.btnAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnAdd.FlatStyle")));
			this.btnAdd.Font = ((System.Drawing.Font)(resources.GetObject("btnAdd.Font")));
			this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
			this.btnAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.ImageAlign")));
			this.btnAdd.ImageIndex = ((int)(resources.GetObject("btnAdd.ImageIndex")));
			this.btnAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnAdd.ImeMode")));
			this.btnAdd.Location = ((System.Drawing.Point)(resources.GetObject("btnAdd.Location")));
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnAdd.RightToLeft")));
			this.btnAdd.Size = ((System.Drawing.Size)(resources.GetObject("btnAdd.Size")));
			this.btnAdd.TabIndex = ((int)(resources.GetObject("btnAdd.TabIndex")));
			this.btnAdd.Text = resources.GetString("btnAdd.Text");
			this.btnAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.TextAlign")));
			this.btnAdd.Visible = ((bool)(resources.GetObject("btnAdd.Visible")));
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnRemove
			// 
			this.btnRemove.AccessibleDescription = resources.GetString("btnRemove.AccessibleDescription");
			this.btnRemove.AccessibleName = resources.GetString("btnRemove.AccessibleName");
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnRemove.Anchor")));
			this.btnRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRemove.BackgroundImage")));
			this.btnRemove.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnRemove.Dock")));
			this.btnRemove.Enabled = ((bool)(resources.GetObject("btnRemove.Enabled")));
			this.btnRemove.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnRemove.FlatStyle")));
			this.btnRemove.Font = ((System.Drawing.Font)(resources.GetObject("btnRemove.Font")));
			this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
			this.btnRemove.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRemove.ImageAlign")));
			this.btnRemove.ImageIndex = ((int)(resources.GetObject("btnRemove.ImageIndex")));
			this.btnRemove.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnRemove.ImeMode")));
			this.btnRemove.Location = ((System.Drawing.Point)(resources.GetObject("btnRemove.Location")));
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnRemove.RightToLeft")));
			this.btnRemove.Size = ((System.Drawing.Size)(resources.GetObject("btnRemove.Size")));
			this.btnRemove.TabIndex = ((int)(resources.GetObject("btnRemove.TabIndex")));
			this.btnRemove.Text = resources.GetString("btnRemove.Text");
			this.btnRemove.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRemove.TextAlign")));
			this.btnRemove.Visible = ((bool)(resources.GetObject("btnRemove.Visible")));
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// btnClose
			// 
			this.btnClose.AccessibleDescription = resources.GetString("btnClose.AccessibleDescription");
			this.btnClose.AccessibleName = resources.GetString("btnClose.AccessibleName");
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnClose.Anchor")));
			this.btnClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClose.BackgroundImage")));
			this.btnClose.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnClose.Dock")));
			this.btnClose.Enabled = ((bool)(resources.GetObject("btnClose.Enabled")));
			this.btnClose.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnClose.FlatStyle")));
			this.btnClose.Font = ((System.Drawing.Font)(resources.GetObject("btnClose.Font")));
			this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
			this.btnClose.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnClose.ImageAlign")));
			this.btnClose.ImageIndex = ((int)(resources.GetObject("btnClose.ImageIndex")));
			this.btnClose.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnClose.ImeMode")));
			this.btnClose.Location = ((System.Drawing.Point)(resources.GetObject("btnClose.Location")));
			this.btnClose.Name = "btnClose";
			this.btnClose.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnClose.RightToLeft")));
			this.btnClose.Size = ((System.Drawing.Size)(resources.GetObject("btnClose.Size")));
			this.btnClose.TabIndex = ((int)(resources.GetObject("btnClose.TabIndex")));
			this.btnClose.Text = resources.GetString("btnClose.Text");
			this.btnClose.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnClose.TextAlign")));
			this.btnClose.Visible = ((bool)(resources.GetObject("btnClose.Visible")));
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "dll";
			this.openFileDialog.Filter = resources.GetString("openFileDialog.Filter");
			this.openFileDialog.Title = resources.GetString("openFileDialog.Title");
			// 
			// btnConfigure
			// 
			this.btnConfigure.AccessibleDescription = resources.GetString("btnConfigure.AccessibleDescription");
			this.btnConfigure.AccessibleName = resources.GetString("btnConfigure.AccessibleName");
			this.btnConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnConfigure.Anchor")));
			this.btnConfigure.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnConfigure.BackgroundImage")));
			this.btnConfigure.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnConfigure.Dock")));
			this.btnConfigure.Enabled = ((bool)(resources.GetObject("btnConfigure.Enabled")));
			this.btnConfigure.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnConfigure.FlatStyle")));
			this.btnConfigure.Font = ((System.Drawing.Font)(resources.GetObject("btnConfigure.Font")));
			this.btnConfigure.Image = ((System.Drawing.Image)(resources.GetObject("btnConfigure.Image")));
			this.btnConfigure.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnConfigure.ImageAlign")));
			this.btnConfigure.ImageIndex = ((int)(resources.GetObject("btnConfigure.ImageIndex")));
			this.btnConfigure.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnConfigure.ImeMode")));
			this.btnConfigure.Location = ((System.Drawing.Point)(resources.GetObject("btnConfigure.Location")));
			this.btnConfigure.Name = "btnConfigure";
			this.btnConfigure.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnConfigure.RightToLeft")));
			this.btnConfigure.Size = ((System.Drawing.Size)(resources.GetObject("btnConfigure.Size")));
			this.btnConfigure.TabIndex = ((int)(resources.GetObject("btnConfigure.TabIndex")));
			this.btnConfigure.Text = resources.GetString("btnConfigure.Text");
			this.btnConfigure.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnConfigure.TextAlign")));
			this.btnConfigure.Visible = ((bool)(resources.GetObject("btnConfigure.Visible")));
			this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
			// 
			// ManageAddInDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.btnConfigure);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnRemove);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.lstLoadedAddIns);
			this.Controls.Add(this.lblAddInListCaption);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ManageAddInDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		#region events
		private void btnClose_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void btnAdd_Click(object sender, EventArgs e) {
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// add Add-in
				try {
					IAddIn addIn = manager.Load(openFileDialog.FileName);
					if (addIn != null) {
						foreach (IAddInPackage package in addIn.AddInPackages) {
							package.Load(this.serviceProvider);
						}
						ListViewItem item = this.lstLoadedAddIns.Items.Add(new ListViewItem(new string[]{addIn.Name, addIn.Location}));
						item.Tag = addIn;
					}
				} catch (Exception ex) {
					MessageBox.Show(this, String.Format(SR.AddInLoadFailure,ex.Message, openFileDialog.FileName));
				}
			}
		}

		private void btnRemove_Click(object sender, EventArgs e) {
			if (this.lstLoadedAddIns.SelectedItems.Count > 0) {
				ListViewItem selected = this.lstLoadedAddIns.SelectedItems[0];
				try {
					IAddIn addIn = (IAddIn)selected.Tag;
					foreach (IAddInPackage package in addIn.AddInPackages) {
						package.Unload();
					}
					manager.Unload(addIn);
				}catch (Exception ex) {
					MessageBox.Show(this, String.Format(SR.AddInUnloadFailure,ex.Message, selected.SubItems[0].Text));
				}
				this.lstLoadedAddIns.Items.Remove(selected);
			}
		}
		
		private void btnConfigure_Click(object sender, EventArgs e) {
			if (this.lstLoadedAddIns.SelectedItems.Count > 0) {
				ListViewItem selected = this.lstLoadedAddIns.SelectedItems[0];
				try {
					IAddIn addIn = (IAddIn)selected.Tag;
					foreach (IAddInPackage package in addIn.AddInPackages) {
						IAddInPackageConfiguration cfg = package as IAddInPackageConfiguration;
						if (cfg != null && cfg.HasConfigurationUI) {
							cfg.ShowConfigurationUI(this);
						}
					}
					
				}catch (Exception ex) {
					MessageBox.Show(this, String.Format(SR.AddInGeneralFailure,ex.Message, selected.SubItems[0].Text));
				}
			}
		}

		private void OnAddInListItemActivate(object sender, EventArgs e) {
			this.btnRemove.Enabled = this.lstLoadedAddIns.SelectedItems.Count > 0;
			this.btnConfigure.Enabled = (this.btnRemove.Enabled && AddInHasConfigurationUI());
		}
		private void OnAddInListSelectedIndexChanged(object sender, EventArgs e) {
			this.btnRemove.Enabled = this.lstLoadedAddIns.SelectedItems.Count > 0;		
			this.btnConfigure.Enabled = (this.btnRemove.Enabled && AddInHasConfigurationUI());
		}

		#endregion


	}
}
