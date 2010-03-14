using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win;

namespace Ribbon.WindowsApplication
{
	/// <summary>
	/// Summary description for IG_Experiments.
    /// Dragdrop impl. taken from http://www.codeproject.com/KB/combobox/DragDropListBox.aspx
	/// </summary>
    public class IG_Experiments : Form, IDragDropSource
	{
        private readonly UrlCompletionExtender urlExtender;

		private Rectangle _dragOriginBox = Rectangle.Empty;
        private bool _isDragDropTarget = true;
		
		
		private System.Windows.Forms.Panel Form2_Fill_Panel;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Left;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Right;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Top;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _Form2_Toolbars_Dock_Area_Bottom;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsManager ultraToolbarsManager;
		private System.Windows.Forms.ImageList imageList16;
		private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboEditor1;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEditor1;
        private System.Windows.Forms.Integration.ElementHost elementHost2;
        private RssBandit.UI.Forms.BanditToolbar banditToolbar1;
		private System.ComponentModel.IContainer components;

		public IG_Experiments()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            urlExtender = new UrlCompletionExtender(this);
            urlExtender.Add(this.ultraComboEditor1);

            // this do the trick to auto-complete with the system urls/IE urls:
            this.ultraComboEditor1.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.None;
            TextBox tb = ((EditorWithCombo)this.ultraComboEditor1.Editor).TextBox;
            tb.AutoCompleteSource = AutoCompleteSource.AllUrl;
            tb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;

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
            Infragistics.Win.UltraWinEditors.EditorButton editorButton1 = new Infragistics.Win.UltraWinEditors.EditorButton();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IG_Experiments));
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
            this._Form2_Toolbars_Dock_Area_Right = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
            this._Form2_Toolbars_Dock_Area_Top = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
            this._Form2_Toolbars_Dock_Area_Bottom = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
            this.ultraToolbarsManager = new Infragistics.Win.UltraWinToolbars.UltraToolbarsManager(this.components);
            this.elementHost2 = new System.Windows.Forms.Integration.ElementHost();
            this.banditToolbar1 = new RssBandit.UI.Forms.BanditToolbar();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboEditor1)).BeginInit();
            this.Form2_Fill_Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraToolbarsManager)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraComboEditor1
            // 
            this.ultraComboEditor1.AllowDrop = true;
            this.ultraComboEditor1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraComboEditor1.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            appearance15.Image = global::RssBandit.UI.Properties.Resources.feed_discovered_16;
            editorButton1.Appearance = appearance15;
            this.ultraComboEditor1.ButtonsLeft.Add(editorButton1);
            this.ultraComboEditor1.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2007;
            this.ultraComboEditor1.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.OnMouseEnter;
            this.ultraComboEditor1.HasMRUList = true;
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
            this.ultraComboEditor1.Location = new System.Drawing.Point(187, 15);
            this.ultraComboEditor1.MaxMRUItems = 3;
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
            this.ultraComboEditor1.Size = new System.Drawing.Size(315, 21);
            this.ultraComboEditor1.SortStyle = Infragistics.Win.ValueListSortStyle.Ascending;
            this.ultraComboEditor1.TabIndex = 1;
            this.ultraComboEditor1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseUp);
            this.ultraComboEditor1.DragLeave += new System.EventHandler(this.OnUrlDragLeave);
            this.ultraComboEditor1.SelectionChangeCommitted += new System.EventHandler(this.OnUrlSelectionChangeCommited);
            this.ultraComboEditor1.DragOver += new System.Windows.Forms.DragEventHandler(this.OnUrlDragOver);
            this.ultraComboEditor1.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnUrlDragDrop);
            this.ultraComboEditor1.AfterExitEditMode += new System.EventHandler(this.OnUrlAfterExitEditMode);
            this.ultraComboEditor1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseMove);
            this.ultraComboEditor1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnUrlMouseDown);
            this.ultraComboEditor1.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnUrlDragEnter);
            this.ultraComboEditor1.EditorButtonClick += new Infragistics.Win.UltraWinEditors.EditorButtonEventHandler(this.OnUrlEditorButtonClick);
            // 
            // Form2_Fill_Panel
            // 
            this.Form2_Fill_Panel.Controls.Add(this.ultraTextEditor1);
            this.Form2_Fill_Panel.Controls.Add(this.ultraComboEditor1);
            this.Form2_Fill_Panel.Cursor = System.Windows.Forms.Cursors.Default;
            this.Form2_Fill_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Form2_Fill_Panel.Location = new System.Drawing.Point(0, 92);
            this.Form2_Fill_Panel.Name = "Form2_Fill_Panel";
            this.Form2_Fill_Panel.Size = new System.Drawing.Size(507, 219);
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
            this.ultraTextEditor1.Size = new System.Drawing.Size(495, 167);
            this.ultraTextEditor1.TabIndex = 2;
            this.ultraTextEditor1.Text = "ultraTextEditor1";
            this.ultraTextEditor1.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnTextEditorDragDrop);
            this.ultraTextEditor1.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnTextEditorDragEnter);
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
            this._Form2_Toolbars_Dock_Area_Left.BackColor = System.Drawing.SystemColors.Control;
            this._Form2_Toolbars_Dock_Area_Left.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Left;
            this._Form2_Toolbars_Dock_Area_Left.ForeColor = System.Drawing.SystemColors.ControlText;
            this._Form2_Toolbars_Dock_Area_Left.Location = new System.Drawing.Point(0, 92);
            this._Form2_Toolbars_Dock_Area_Left.Name = "_Form2_Toolbars_Dock_Area_Left";
            this._Form2_Toolbars_Dock_Area_Left.Size = new System.Drawing.Size(0, 219);
            this._Form2_Toolbars_Dock_Area_Left.ToolbarsManager = this.ultraToolbarsManager;
            // 
            // _Form2_Toolbars_Dock_Area_Right
            // 
            this._Form2_Toolbars_Dock_Area_Right.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this._Form2_Toolbars_Dock_Area_Right.BackColor = System.Drawing.SystemColors.Control;
            this._Form2_Toolbars_Dock_Area_Right.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Right;
            this._Form2_Toolbars_Dock_Area_Right.ForeColor = System.Drawing.SystemColors.ControlText;
            this._Form2_Toolbars_Dock_Area_Right.Location = new System.Drawing.Point(507, 92);
            this._Form2_Toolbars_Dock_Area_Right.Name = "_Form2_Toolbars_Dock_Area_Right";
            this._Form2_Toolbars_Dock_Area_Right.Size = new System.Drawing.Size(0, 219);
            this._Form2_Toolbars_Dock_Area_Right.ToolbarsManager = this.ultraToolbarsManager;
            // 
            // _Form2_Toolbars_Dock_Area_Top
            // 
            this._Form2_Toolbars_Dock_Area_Top.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this._Form2_Toolbars_Dock_Area_Top.BackColor = System.Drawing.SystemColors.Control;
            this._Form2_Toolbars_Dock_Area_Top.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Top;
            this._Form2_Toolbars_Dock_Area_Top.ForeColor = System.Drawing.SystemColors.ControlText;
            this._Form2_Toolbars_Dock_Area_Top.Location = new System.Drawing.Point(0, 0);
            this._Form2_Toolbars_Dock_Area_Top.Name = "_Form2_Toolbars_Dock_Area_Top";
            this._Form2_Toolbars_Dock_Area_Top.Size = new System.Drawing.Size(507, 92);
            this._Form2_Toolbars_Dock_Area_Top.ToolbarsManager = this.ultraToolbarsManager;
            // 
            // _Form2_Toolbars_Dock_Area_Bottom
            // 
            this._Form2_Toolbars_Dock_Area_Bottom.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this._Form2_Toolbars_Dock_Area_Bottom.BackColor = System.Drawing.SystemColors.Control;
            this._Form2_Toolbars_Dock_Area_Bottom.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Bottom;
            this._Form2_Toolbars_Dock_Area_Bottom.ForeColor = System.Drawing.SystemColors.ControlText;
            this._Form2_Toolbars_Dock_Area_Bottom.Location = new System.Drawing.Point(0, 311);
            this._Form2_Toolbars_Dock_Area_Bottom.Name = "_Form2_Toolbars_Dock_Area_Bottom";
            this._Form2_Toolbars_Dock_Area_Bottom.Size = new System.Drawing.Size(507, 0);
            this._Form2_Toolbars_Dock_Area_Bottom.ToolbarsManager = this.ultraToolbarsManager;
            // 
            // ultraToolbarsManager
            // 
            this.ultraToolbarsManager.DesignerFlags = 1;
            this.ultraToolbarsManager.DockWithinContainer = this;
            this.ultraToolbarsManager.DockWithinContainerBaseType = typeof(System.Windows.Forms.Form);
            this.ultraToolbarsManager.ImageListSmall = this.imageList16;
            ribbonTab1.Caption = "ribbon1";
            ribbonGroup1.Caption = "Properties";
            ribbonGroup1.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool6});
            ribbonTab1.Groups.AddRange(new Infragistics.Win.UltraWinToolbars.RibbonGroup[] {
            ribbonGroup1});
            this.ultraToolbarsManager.Ribbon.NonInheritedRibbonTabs.AddRange(new Infragistics.Win.UltraWinToolbars.RibbonTab[] {
            ribbonTab1});
            this.ultraToolbarsManager.ShowFullMenusDelay = 500;
            ultraToolbar1.DockedColumn = 0;
            ultraToolbar1.DockedRow = 3;
            ultraToolbar1.FloatingSize = new System.Drawing.Size(100, 20);
            controlContainerTool1.ControlName = "ultraComboEditor1";
            controlContainerTool1.InstanceProps.Width = 315;
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
            buttonTool2.SharedPropsInternal.Caption = "Exit";
            buttonTool2.SharedPropsInternal.Category = "Menu";
            buttonTool2.SharedPropsInternal.CustomizerCaption = "Application Exit";
            popupMenuTool6.SharedPropsInternal.Caption = "File";
            popupMenuTool6.SharedPropsInternal.Category = "Menu";
            popupMenuTool6.SharedPropsInternal.CustomizerCaption = "File Menu";
            buttonTool4.InstanceProps.IsFirstInGroup = true;
            popupMenuTool6.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool3,
            buttonTool4});
            popupMenuTool7.SharedPropsInternal.Caption = "Edit";
            popupMenuTool7.SharedPropsInternal.Category = "Menu";
            popupMenuTool7.SharedPropsInternal.CustomizerCaption = "Edit Menu";
            popupMenuTool8.SharedPropsInternal.Caption = "View";
            popupMenuTool8.SharedPropsInternal.Category = "Menu";
            popupMenuTool8.SharedPropsInternal.CustomizerCaption = "View Menu";
            popupMenuTool9.SharedPropsInternal.Caption = "Tools";
            popupMenuTool9.SharedPropsInternal.Category = "Menu";
            popupMenuTool9.SharedPropsInternal.CustomizerCaption = "Tools Menu";
            popupMenuTool10.SharedPropsInternal.Caption = "Help";
            popupMenuTool10.SharedPropsInternal.Category = "Menu";
            popupMenuTool10.SharedPropsInternal.CustomizerCaption = "Help Menu";
            appearance4.Image = 0;
            buttonTool5.SharedPropsInternal.AppearancesSmall.Appearance = appearance4;
            buttonTool5.SharedPropsInternal.Caption = "Import...";
            buttonTool5.SharedPropsInternal.Category = "Menu";
            buttonTool5.SharedPropsInternal.CustomizerCaption = "Import";
            controlContainerTool2.ControlName = "ultraComboEditor1";
            controlContainerTool2.SharedPropsInternal.Caption = "Url";
            controlContainerTool2.SharedPropsInternal.Category = "Browse Tools";
            controlContainerTool2.SharedPropsInternal.CustomizerCaption = "Web Browser";
            controlContainerTool2.SharedPropsInternal.Width = 315;
            this.ultraToolbarsManager.Tools.AddRange(new Infragistics.Win.UltraWinToolbars.ToolBase[] {
            buttonTool2,
            popupMenuTool6,
            popupMenuTool7,
            popupMenuTool8,
            popupMenuTool9,
            popupMenuTool10,
            buttonTool5,
            controlContainerTool2});
            this.ultraToolbarsManager.Visible = false;
            // 
            // elementHost2
            // 
            this.elementHost2.Dock = System.Windows.Forms.DockStyle.Top;
            this.elementHost2.Location = new System.Drawing.Point(0, 92);
            this.elementHost2.Name = "elementHost2";
            this.elementHost2.Size = new System.Drawing.Size(507, 100);
            this.elementHost2.TabIndex = 6;
            this.elementHost2.Text = "toolbarHost";
            this.elementHost2.Child = this.banditToolbar1;
            // 
            // IG_Experiments
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(507, 311);
            this.Controls.Add(this.elementHost2);
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

		private void OnUrlDragDrop(object sender, DragEventArgs e) {
           
            IDragDropSource src = e.Data.GetData("IDragDropSource") as IDragDropSource;
            if (src != null)
            {
                object[] srcItems = src.GetSelectedItems();
                if (srcItems.Length > 0)
                    this.ultraComboEditor1.SelectedItem.DisplayText =srcItems[0] as string;

                DropOperation operation = DropOperation.CopyToHere; // Remembers the operation for the event we'll raise.

                // Notify the target (this control).
                DroppedEventArgs de = new DroppedEventArgs()
                {
                    Operation = operation,
                    Source = src,
                    Target = this,
                    DroppedItems = srcItems
                };

                //TODO:
                //OnDropped(de);

                // Notify the source (the other control).
                if (operation != DropOperation.Reorder)
                {
                    de = new DroppedEventArgs()
                    {
                        Operation = operation == DropOperation.MoveToHere ? DropOperation.MoveFromHere : DropOperation.CopyFromHere,
                        Source = src,
                        Target = this,
                        DroppedItems = srcItems
                    };
                    src.OnDropped(de);
                }
            }
		}

		private void OnUrlDragEnter(object sender, DragEventArgs e) {
            e.Effect = GetDragDropEffect(e);
            
		}

		private void OnUrlDragLeave(object sender, EventArgs e) {
		
		}

		private void OnUrlDragOver(object sender, DragEventArgs e) {

            e.Effect = GetDragDropEffect(e);
		}

		private void OnUrlMouseDown(object sender, MouseEventArgs e) 
        {
			
            if (this.ultraComboEditor1.SelectedItem != null) 
            {
                Size dragSize = SystemInformation.DragSize;
                _dragOriginBox = new Rectangle(new Point(e.X -
                  (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);

			}
		}

		
		private void OnUrlMouseMove(object sender, MouseEventArgs e) {

            if (_dragOriginBox != Rectangle.Empty &&
             !_dragOriginBox.Contains(e.X, e.Y))
            {
                ultraComboEditor1.DoDragDrop(
                    new DataObject("IDragDropSource", this),
                    DragDropEffects.All);
                _dragOriginBox = Rectangle.Empty;
            }
		    
		}

		private void OnUrlMouseUp(object sender, MouseEventArgs e) 
        {
			_dragOriginBox = Rectangle.Empty;
			
		}
		
		

		private void OnFormLoad(object sender, EventArgs e) 
        {
			
		}

        private void OnUrlEditorButtonClick(object sender, Infragistics.Win.UltraWinEditors.EditorButtonEventArgs e)
        {
            Debug.WriteLine("OnUrlEditorButtonClick");
        }

        private void OnTextEditorDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void OnTextEditorDragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the drag item data. 
            // Conditions have been testet in OnDragEnter and OnDragOver, so everything should be ok here.
            IDragDropSource src = e.Data.GetData("IDragDropSource") as IDragDropSource;
            if (src != null)
            {
                object[] srcItems = src.GetSelectedItems();
                if (srcItems.Length > 0)
                    ultraTextEditor1.Text = srcItems[0] as string;
                DropOperation operation = DropOperation.CopyToHere; // Remembers the operation for the event we'll raise.

                // Notify the target (this control).
                DroppedEventArgs de = new DroppedEventArgs()
                {
                    Operation = operation,
                    Source = src,
                    Target = this,
                    DroppedItems = srcItems
                };
                
                //TODO:
                //OnDropped(de);

                // Notify the source (the other control).
                if (operation != DropOperation.Reorder)
                {
                    de = new DroppedEventArgs()
                    {
                        Operation = operation == DropOperation.MoveToHere ? DropOperation.MoveFromHere : DropOperation.CopyFromHere,
                        Source = src,
                        Target = this,
                        DroppedItems = srcItems
                    };
                    src.OnDropped(de);
                }
            }

            
        }

        /// <summary>
        /// Determines the drag-and-drop operation which is beeing performed, which can be either None, Move or Copy. 
        /// </summary>
        /// <param name="drgevent">DragEventArgs.</param>
        /// <returns>The current drag-and-drop operation.</returns>
        private DragDropEffects GetDragDropEffect(DragEventArgs drgevent)
        {
            const int CtrlKeyPlusLeftMouseButton = 9; // KeyState.

            DragDropEffects effect = DragDropEffects.None;

            // Retrieve the source control of the drag-and-drop operation.
            IDragDropSource src = drgevent.Data.GetData("IDragDropSource") as IDragDropSource;

            if (src != null && _dragDropGroup == src.DragDropGroup)
            { // The stuff being draged is compatible.
                if (src == this)
                { 
                    // Drag-and-drop happens within this control.
                    
                    //if (_allowReorder && !this.Sorted)
                    //{
                    //    effect = DragDropEffects.Move;
                    //}
                }
                else if (_isDragDropTarget)
                {
                    // If only Copy is allowed then copy. If Copy and Move are allowed, then Move, unless the Ctrl-key is pressed.
                    if (src.IsDragDropCopySource && (!src.IsDragDropMoveSource || drgevent.KeyState == CtrlKeyPlusLeftMouseButton))
                    {
                        effect = DragDropEffects.Copy;
                    }
                    else if (src.IsDragDropMoveSource)
                    {
                        effect = DragDropEffects.Move;
                    }
                }
            }
            return effect;
        }

	    #region TODO: Implementation of IDragDropSource (should be at URL combo control)

        private string _dragDropGroup = String.Empty;
	    string IDragDropSource.DragDropGroup
	    {
            get { return _dragDropGroup; }
	    }

	    private bool _IsDragDropCopySource;
	    bool IDragDropSource.IsDragDropCopySource
	    {
            get { return _IsDragDropCopySource; }
	    }

	    private bool _IsDragDropMoveSource;
	    bool IDragDropSource.IsDragDropMoveSource
	    {
            get { return _IsDragDropMoveSource; }
	    }

	    object[] IDragDropSource.GetSelectedItems()
	    {
	        if (this.ultraComboEditor1.SelectedItem != null)
	        {
                return new object[] { this.ultraComboEditor1.SelectedItem .DisplayText};
	        }
            return new object[]{};
	    }

	    void IDragDropSource.RemoveSelectedItems(ref int rowIndexToAjust)
	    {
	        
	    }

        /// <summary>
        /// Raises the Dropped" event. Called by the target on a successful drop op
        /// to notify the drop source.
        /// </summary>
        /// <param name="e">The <see cref="Ribbon.WindowsApplication.DroppedEventArgs"/> instance containing the event data.</param>
	    void IDragDropSource.OnDropped(DroppedEventArgs e)
	    {
	        //TODO
	    }

	    #endregion

        private void OnUrlSelectionChangeCommited(object sender, EventArgs e)
        {
            Uri uri;
            if (Uri.TryCreate(this.ultraComboEditor1.Text, UriKind.Absolute, out uri))
            {
                this.ultraTextEditor1.Text ="Navigate to " +uri.AbsoluteUri;
            }
        }

        private void OnUrlAfterExitEditMode(object sender, EventArgs e)
        {
            Uri uri;
            if (Uri.TryCreate(this.ultraComboEditor1.Text, UriKind.Absolute, out uri))
            {
                ValueListItem item = new ValueListItem(this.ultraComboEditor1.Text);
                if (!this.ultraComboEditor1.Items.Contains(item))
                    this.ultraComboEditor1.Items.Add(item);
            }
        }
	}

    #region DragDrop support

    public interface IDragDropSource
    {
        string DragDropGroup { get; }
        bool IsDragDropCopySource { get; }
        bool IsDragDropMoveSource { get; }
        object[] GetSelectedItems();
        void RemoveSelectedItems(ref int rowIndexToAjust);
        void OnDropped(DroppedEventArgs e);
    }
    public enum DropOperation
    {
        Reorder,
        MoveToHere,
        CopyToHere,
        MoveFromHere,
        CopyFromHere
    }

    public class DroppedEventArgs : EventArgs
    {
        public DropOperation Operation { get; set; }
        public IDragDropSource Source { get; set; }
        public IDragDropSource Target { get; set; }
        public object[] DroppedItems { get; set; }
    }

    #endregion

    #region UrlCompletionExtender

    /// <summary>
    /// Used for Ctrl-Enter completion, similar to IE url combobox
    /// </summary>
    public class UrlCompletionExtender
    {

        private string[] urlTemplates = new[] {
												"http://www.{0}.com/",
												"http://www.{0}.net/",
												"http://www.{0}.org/",
												"http://www.{0}.info/",
		};
        private readonly Form ownerForm;
        private readonly IButtonControl ownerCancelButton;
        private int lastExpIndex = -1;
        private string toExpand;

        public UrlCompletionExtender(Form f)
        {
            if (f != null && f.CancelButton != null)
            {
                ownerForm = f;
                ownerCancelButton = f.CancelButton;
            }
        }


        public void Add(Control monitorControl)
        {
            if (monitorControl != null)
            {
                monitorControl.KeyDown += OnMonitorControlKeyDown;
                if (ownerForm != null && ownerCancelButton != null)
                {
                    monitorControl.Enter += OnMonitorControlEnter;
                    monitorControl.Leave += OnMonitorControlLeave;
                }
            }
        }

        private void ResetExpansion()
        {
            lastExpIndex = -1;
            toExpand = null;
        }

        private void RaiseExpansionIndex()
        {
            lastExpIndex = (++lastExpIndex % urlTemplates.Length);
        }

        private void OnMonitorControlKeyDown(object sender, KeyEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null) return;

            TextBox tb = sender as TextBox;
            ComboBox cb = sender as ComboBox;
            UltraComboEditor ce = sender as UltraComboEditor;

            bool ctrlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            if (e.KeyCode == Keys.Return && ctrlKeyPressed)
            {
                if (lastExpIndex < 0 || toExpand == null)
                {
                    string txt = ctrl.Text;
                    if (txt.Length > 0 && txt.IndexOfAny(new char[] { ':', '.', '/' }) < 0)
                    {
                        toExpand = txt;
                        RaiseExpansionIndex();
                    }
                }
                if (lastExpIndex >= 0 && toExpand != null)
                {
                    ctrl.Text = String.Format(urlTemplates[lastExpIndex], toExpand);
                    if (tb != null)
                        tb.SelectionStart = ctrl.Text.Length;
                    if (cb != null && cb.DropDownStyle != ComboBoxStyle.DropDownList)
                        cb.SelectionStart = cb.Text.Length;
                    if (ce != null && ce.DropDownStyle != DropDownStyle.DropDownList)
                        ce.SelectionStart = ctrl.Text.Length;
                    RaiseExpansionIndex();
                }
            }
            else
            {
                ResetExpansion();
            }
        }

        private void OnMonitorControlLeave(object sender, EventArgs e)
        {
            ownerForm.CancelButton = ownerCancelButton;		// restore, if not yet done
        }

        private void OnMonitorControlEnter(object sender, EventArgs e)
        {
            ownerForm.CancelButton = null;	// drop
        }
    }
    #endregion
}
