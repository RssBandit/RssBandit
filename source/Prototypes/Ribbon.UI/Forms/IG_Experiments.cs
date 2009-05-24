using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ribbon.WindowsApplication
{
	/// <summary>
	/// Summary description for IG_Experiments.
	/// </summary>
	public class IG_Experiments : System.Windows.Forms.Form
	{
		private string dragText = null;
		private bool dragging = false;
		private bool dragMouseDown = false;
		
		private System.Windows.Forms.Panel Form2_Fill_Panel;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Left;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Right;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Top;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Bottom;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsManager ultraToolbarsManager;
		private System.Windows.Forms.ImageList imageList16;
		private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboEditor1;
		private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEditor1;
		private System.Windows.Forms.Timer startDragTimer;
		private System.ComponentModel.IContainer components;

		public IG_Experiments()
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
			this.components = new System.ComponentModel.Container();
			Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IG_Experiments));
			Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
			Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
			Infragistics.Win.UltraWinToolbars.RibbonTab ribbonTab1 = new Infragistics.Win.UltraWinToolbars.RibbonTab("ribbon1");
			Infragistics.Win.UltraWinToolbars.RibbonGroup ribbonGroup1 = new Infragistics.Win.UltraWinToolbars.RibbonGroup("ribbonGroup1");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool6 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdImport");
			Infragistics.Win.UltraWinToolbars.UltraToolbar ultraToolbar1 = new Infragistics.Win.UltraWinToolbars.UltraToolbar("tbWebBrowser");
			Infragistics.Win.UltraWinToolbars.ControlContainerTool controlContainerTool1 = new Infragistics.Win.UltraWinToolbars.ControlContainerTool("cmdUrlContainer");
			Infragistics.Win.UltraWinToolbars.UltraToolbar ultraToolbar2 = new Infragistics.Win.UltraWinToolbars.UltraToolbar("tbMainMenu");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool1 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuFile");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool2 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuEdit");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool3 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuView");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool4 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuTools");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool5 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuHelp");
			Infragistics.Win.UltraWinToolbars.UltraToolbar ultraToolbar3 = new Infragistics.Win.UltraWinToolbars.UltraToolbar("tbMainAppBar");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool1 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdImport");
			Infragistics.Win.UltraWinToolbars.UltraToolbar ultraToolbar4 = new Infragistics.Win.UltraWinToolbars.UltraToolbar("tbSearchBar");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool2 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdAppExit");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool6 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuFile");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool3 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdImport");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool4 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdAppExit");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool7 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuEdit");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool8 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuView");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool9 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuTools");
			Infragistics.Win.UltraWinToolbars.PopupMenuTool popupMenuTool10 = new Infragistics.Win.UltraWinToolbars.PopupMenuTool("mnuHelp");
			Infragistics.Win.UltraWinToolbars.ButtonTool buttonTool5 = new Infragistics.Win.UltraWinToolbars.ButtonTool("cmdImport");
			Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
			Infragistics.Win.UltraWinToolbars.ControlContainerTool controlContainerTool2 = new Infragistics.Win.UltraWinToolbars.ControlContainerTool("cmdUrlContainer");
			this.ultraComboEditor1 = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
			this.Form2_Fill_Panel = new System.Windows.Forms.Panel();
			this.ultraTextEditor1 = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
			this.imageList16 = new System.Windows.Forms.ImageList(this.components);
			this._Form2_Toolbars_Dock_Area_Left = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this.ultraToolbarsManager = new Infragistics.Win.UltraWinToolbars.UltraToolbarsManager(this.components);
			this._Form2_Toolbars_Dock_Area_Right = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this._Form2_Toolbars_Dock_Area_Top = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this._Form2_Toolbars_Dock_Area_Bottom = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this.startDragTimer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.ultraComboEditor1)).BeginInit();
			this.Form2_Fill_Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ultraTextEditor1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ultraToolbarsManager)).BeginInit();
			this.SuspendLayout();
			// 
			// ultraComboEditor1
			// 
			this.ultraComboEditor1.AllowDrop = true;
			appearance1.Image = ((object)(resources.GetObject("appearance1.Image")));
			this.ultraComboEditor1.Appearance = appearance1;
			this.ultraComboEditor1.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.Append;
			this.ultraComboEditor1.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2003;
			this.ultraComboEditor1.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.OnMouseEnter;
			appearance2.Image = ((object)(resources.GetObject("appearance2.Image")));
			this.ultraComboEditor1.ItemAppearance = appearance2;
			valueListItem1.DataValue = "ValueListItem3";
			valueListItem1.DisplayText = "msdn.microsoft.com";
			valueListItem2.DataValue = "ValueListItem1";
			valueListItem2.DisplayText = "www.microsoft.com";
			valueListItem3.DataValue = "ValueListItem0";
			valueListItem3.DisplayText = "www.procos.com";
			valueListItem4.DataValue = "ValueListItem2";
			valueListItem4.DisplayText = "www.rssbandit.org";
			this.ultraComboEditor1.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem1,
            valueListItem2,
            valueListItem3,
            valueListItem4});
			this.ultraComboEditor1.Location = new System.Drawing.Point(350, 15);
			this.ultraComboEditor1.MRUList = new object[] {
        ((object)("ValueListItem3")),
        ((object)("ValueListItem1")),
        ((object)("ValueListItem0")),
        ((object)("ValueListItem2"))};
			this.ultraComboEditor1.Name = "ultraComboEditor1";
			this.ultraComboEditor1.NullText = "[empty]";
			appearance3.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ultraComboEditor1.NullTextAppearance = appearance3;
			this.ultraComboEditor1.ShowOverflowIndicator = true;
			this.ultraComboEditor1.Size = new System.Drawing.Size(144, 21);
			this.ultraComboEditor1.SortStyle = Infragistics.Win.ValueListSortStyle.Ascending;
			this.ultraComboEditor1.TabIndex = 1;
			this.ultraComboEditor1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseUp);
			this.ultraComboEditor1.DragLeave += new System.EventHandler(this.OnUrlDragLeave);
			this.ultraComboEditor1.DragOver += new System.Windows.Forms.DragEventHandler(this.OnUrlDragOver);
			this.ultraComboEditor1.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnUrlDragDrop);
			this.ultraComboEditor1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseMove);
			this.ultraComboEditor1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseDown);
			this.ultraComboEditor1.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnUrlDragEnter);
			// 
			// Form2_Fill_Panel
			// 
			this.Form2_Fill_Panel.Controls.Add(this.ultraTextEditor1);
			this.Form2_Fill_Panel.Controls.Add(this.ultraComboEditor1);
			this.Form2_Fill_Panel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Form2_Fill_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Form2_Fill_Panel.Location = new System.Drawing.Point(4, 153);
			this.Form2_Fill_Panel.Name = "Form2_Fill_Panel";
			this.Form2_Fill_Panel.Size = new System.Drawing.Size(499, 154);
			this.Form2_Fill_Panel.TabIndex = 0;
			// 
			// ultraTextEditor1
			// 
			this.ultraTextEditor1.AcceptsTab = true;
			this.ultraTextEditor1.AllowDrop = true;
			this.ultraTextEditor1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ultraTextEditor1.Location = new System.Drawing.Point(5, 45);
			this.ultraTextEditor1.Multiline = true;
			this.ultraTextEditor1.Name = "ultraTextEditor1";
			this.ultraTextEditor1.Size = new System.Drawing.Size(487, 102);
			this.ultraTextEditor1.TabIndex = 2;
			this.ultraTextEditor1.Text = "ultraTextEditor1";
			// 
			// imageList16
			// 
			this.imageList16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList16.ImageStream")));
			this.imageList16.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList16.Images.SetKeyName(0, "");
			// 
			// _Form2_Toolbars_Dock_Area_Left
			// 
			this._Form2_Toolbars_Dock_Area_Left.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._Form2_Toolbars_Dock_Area_Left.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
			this._Form2_Toolbars_Dock_Area_Left.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Left;
			this._Form2_Toolbars_Dock_Area_Left.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Form2_Toolbars_Dock_Area_Left.InitialResizeAreaExtent = 4;
			this._Form2_Toolbars_Dock_Area_Left.Location = new System.Drawing.Point(0, 153);
			this._Form2_Toolbars_Dock_Area_Left.Name = "_Form2_Toolbars_Dock_Area_Left";
			this._Form2_Toolbars_Dock_Area_Left.Size = new System.Drawing.Size(4, 154);
			this._Form2_Toolbars_Dock_Area_Left.ToolbarsManager = this.ultraToolbarsManager;
			// 
			// ultraToolbarsManager
			// 
			this.ultraToolbarsManager.DesignerFlags = 1;
			this.ultraToolbarsManager.DockWithinContainer = this;
			this.ultraToolbarsManager.DockWithinContainerBaseType = typeof(System.Windows.Forms.Form);
			this.ultraToolbarsManager.ImageListSmall = this.imageList16;
			ribbonTab1.Caption = "ribbon1";
			ribbonGroup1.Caption = "ribbonGroup1";
			ribbonGroup1.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool6});
			ribbonTab1.Groups.AddRange(new Infragistics.Win.UltraWinToolbars.RibbonGroup[] {
            ribbonGroup1});
			this.ultraToolbarsManager.Ribbon.NonInheritedRibbonTabs.AddRange(new Infragistics.Win.UltraWinToolbars.RibbonTab[] {
            ribbonTab1});
			this.ultraToolbarsManager.Ribbon.Visible = true;
			this.ultraToolbarsManager.ShowFullMenusDelay = 500;
			ultraToolbar1.DockedColumn = 1;
			ultraToolbar1.DockedRow = 1;
			ultraToolbar1.FloatingSize = new System.Drawing.Size(100, 20);
			controlContainerTool1.ControlName = "ultraComboEditor1";
			ultraToolbar1.NonInheritedTools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            controlContainerTool1});
			ultraToolbar1.Text = "Web Browser Bar";
			ultraToolbar2.DockedColumn = 0;
			ultraToolbar2.DockedRow = 0;
			ultraToolbar2.IsMainMenuBar = true;
			ultraToolbar2.NonInheritedTools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            popupMenuTool1,
            popupMenuTool2,
            popupMenuTool3,
            popupMenuTool4,
            popupMenuTool5});
			ultraToolbar2.Text = "Main Menu Bar";
			ultraToolbar3.DockedColumn = 0;
			ultraToolbar3.DockedRow = 1;
			ultraToolbar3.NonInheritedTools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool1});
			ultraToolbar3.Text = "Main Application Bar";
			ultraToolbar4.DockedColumn = 0;
			ultraToolbar4.DockedRow = 2;
			ultraToolbar4.Text = "Search Bar";
			this.ultraToolbarsManager.Toolbars.AddRange(new Infragistics.Win.UltraWinToolbars.UltraToolbar[] {
            ultraToolbar1,
            ultraToolbar2,
            ultraToolbar3,
            ultraToolbar4});
			buttonTool2.SharedProps.Caption = "Exit";
			buttonTool2.SharedProps.Category = "Menu";
			buttonTool2.SharedProps.CustomizerCaption = "Application Exit";
			popupMenuTool6.SharedProps.Caption = "File";
			popupMenuTool6.SharedProps.Category = "Menu";
			popupMenuTool6.SharedProps.CustomizerCaption = "File Menu";
			buttonTool4.InstanceProps.IsFirstInGroup = true;
			popupMenuTool6.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool3,
            buttonTool4});
			popupMenuTool7.SharedProps.Caption = "Edit";
			popupMenuTool7.SharedProps.Category = "Menu";
			popupMenuTool7.SharedProps.CustomizerCaption = "Edit Menu";
			popupMenuTool8.SharedProps.Caption = "View";
			popupMenuTool8.SharedProps.Category = "Menu";
			popupMenuTool8.SharedProps.CustomizerCaption = "View Menu";
			popupMenuTool9.SharedProps.Caption = "Tools";
			popupMenuTool9.SharedProps.Category = "Menu";
			popupMenuTool9.SharedProps.CustomizerCaption = "Tools Menu";
			popupMenuTool10.SharedProps.Caption = "Help";
			popupMenuTool10.SharedProps.Category = "Menu";
			popupMenuTool10.SharedProps.CustomizerCaption = "Help Menu";
			appearance4.Image = 0;
			buttonTool5.SharedProps.AppearancesSmall.Appearance = appearance4;
			buttonTool5.SharedProps.Caption = "Import...";
			buttonTool5.SharedProps.Category = "Menu";
			buttonTool5.SharedProps.CustomizerCaption = "Import";
			controlContainerTool2.ControlName = "ultraComboEditor1";
			controlContainerTool2.SharedProps.Caption = "Url";
			controlContainerTool2.SharedProps.Category = "Browse Tools";
			controlContainerTool2.SharedProps.CustomizerCaption = "Web Browser";
			this.ultraToolbarsManager.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool2,
            popupMenuTool6,
            popupMenuTool7,
            popupMenuTool8,
            popupMenuTool9,
            popupMenuTool10,
            buttonTool5,
            controlContainerTool2});
			// 
			// _Form2_Toolbars_Dock_Area_Right
			// 
			this._Form2_Toolbars_Dock_Area_Right.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._Form2_Toolbars_Dock_Area_Right.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
			this._Form2_Toolbars_Dock_Area_Right.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Right;
			this._Form2_Toolbars_Dock_Area_Right.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Form2_Toolbars_Dock_Area_Right.InitialResizeAreaExtent = 4;
			this._Form2_Toolbars_Dock_Area_Right.Location = new System.Drawing.Point(503, 153);
			this._Form2_Toolbars_Dock_Area_Right.Name = "_Form2_Toolbars_Dock_Area_Right";
			this._Form2_Toolbars_Dock_Area_Right.Size = new System.Drawing.Size(4, 154);
			this._Form2_Toolbars_Dock_Area_Right.ToolbarsManager = this.ultraToolbarsManager;
			// 
			// _Form2_Toolbars_Dock_Area_Top
			// 
			this._Form2_Toolbars_Dock_Area_Top.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._Form2_Toolbars_Dock_Area_Top.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
			this._Form2_Toolbars_Dock_Area_Top.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Top;
			this._Form2_Toolbars_Dock_Area_Top.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Form2_Toolbars_Dock_Area_Top.Location = new System.Drawing.Point(0, 0);
			this._Form2_Toolbars_Dock_Area_Top.Name = "_Form2_Toolbars_Dock_Area_Top";
			this._Form2_Toolbars_Dock_Area_Top.Size = new System.Drawing.Size(507, 153);
			this._Form2_Toolbars_Dock_Area_Top.ToolbarsManager = this.ultraToolbarsManager;
			// 
			// _Form2_Toolbars_Dock_Area_Bottom
			// 
			this._Form2_Toolbars_Dock_Area_Bottom.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._Form2_Toolbars_Dock_Area_Bottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
			this._Form2_Toolbars_Dock_Area_Bottom.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Bottom;
			this._Form2_Toolbars_Dock_Area_Bottom.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Form2_Toolbars_Dock_Area_Bottom.InitialResizeAreaExtent = 4;
			this._Form2_Toolbars_Dock_Area_Bottom.Location = new System.Drawing.Point(0, 307);
			this._Form2_Toolbars_Dock_Area_Bottom.Name = "_Form2_Toolbars_Dock_Area_Bottom";
			this._Form2_Toolbars_Dock_Area_Bottom.Size = new System.Drawing.Size(507, 4);
			this._Form2_Toolbars_Dock_Area_Bottom.ToolbarsManager = this.ultraToolbarsManager;
			// 
			// startDragTimer
			// 
			this.startDragTimer.Interval = 250;
			this.startDragTimer.Tick += new System.EventHandler(this.OnDragTimerTick);
			// 
			// IG_Experiments
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(507, 311);
			this.Controls.Add(this.Form2_Fill_Panel);
			this.Controls.Add(this._Form2_Toolbars_Dock_Area_Left);
			this.Controls.Add(this._Form2_Toolbars_Dock_Area_Right);
			this.Controls.Add(this._Form2_Toolbars_Dock_Area_Top);
			this.Controls.Add(this._Form2_Toolbars_Dock_Area_Bottom);
			this.Name = "IG_Experiments";
			this.Text = "IG_Experiments";
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.ultraComboEditor1)).EndInit();
			this.Form2_Fill_Panel.ResumeLayout(false);
			this.Form2_Fill_Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ultraTextEditor1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ultraToolbarsManager)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void OnUrlDragDrop(object sender, System.Windows.Forms.DragEventArgs e) {
		
		}

		private void OnUrlDragEnter(object sender, System.Windows.Forms.DragEventArgs e) {
		
		}

		private void OnUrlDragLeave(object sender, System.EventArgs e) {
		
		}

		private void OnUrlDragOver(object sender, System.Windows.Forms.DragEventArgs e) {
		
		}

		private void OnUrlMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			this.dragMouseDown = true;
			if (this.ultraComboEditor1.SelectedItem != null) {
				this.dragText = this.ultraComboEditor1.SelectedItem.DisplayText;
				this.startDragTimer.Enabled = true;
			}
		}

		
		private void OnUrlMouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (dragText != null && this.dragging)
				ultraComboEditor1.DoDragDrop(dragText, DragDropEffects.Copy);
		}

		private void OnUrlMouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			this.startDragTimer.Enabled = false;
			this.dragMouseDown = false;
			this.dragging = false;
			this.dragText = null;
		}
		
		private void OnDragTimerTick(object sender, System.EventArgs e) {
			if (this.dragMouseDown  && dragText != null)
				ultraComboEditor1.DoDragDrop(dragText, DragDropEffects.Copy);
		}

		private void OnFormLoad(object sender, System.EventArgs e) {
			//ultraComboEditor1.
			//ultraComboEditor1.Editor.
		}

	}
}
