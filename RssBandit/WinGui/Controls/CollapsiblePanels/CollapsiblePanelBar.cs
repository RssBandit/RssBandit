#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using RssBandit.WinGui.Utility;


namespace RssBandit.WinGui.Controls.CollapsiblePanels {
	/// <summary>
	/// Type of a CollapsiblePanelBar. StandardTaskBar corresponds to the TaskBars known
	/// from Windows XP (static size). FilledTaskBar is a bar that resizes its
	/// CollapsiblePanels to fill the entire ClientArea of the Bar.
	/// </summary>
	public enum PanelBarType
	{
		/// <summary>
		/// The mode XPTaskBar behaves like the standard Windows XP sidebars/taskbars
		/// A number of collapsiblepanels can be hosted in the bar.
		/// If one of the panels collapses, all the ones below are moved up so they are adjacent
		/// </summary>
		StandardTaskBar = 1,
		/// <summary>
		/// This mode is a special version of the XPTaskBar mode
		/// It alows only to host TWO collapsiblepanels.
		/// If one of the panels is collapsed while the other is still expanded, 
		/// the expanded one will be stretched to fill the whole clienarea of the bar.
		/// So the clientarea of the bar is always fully used except if both panels are collapsed.
		/// </summary>
		FilledTaskBar = 2
	}

	/// <summary>
	/// A bar containing Collapsible panels.
	/// The bar takes control over the resizing of the panels.
	/// </summary>
	public class CollapsiblePanelBar : Panel, System.ComponentModel.ISupportInitialize
	{

		#region ------- PRIVATE MEMBERS ---------------------------------------
		private CollapsiblePanelCollection panels = new CollapsiblePanelCollection();
		private int miSpacing = 4;
		private int miBorder = 4;
		private bool mbInitializing = false;
		private PanelBarType meBarType;
		private bool mbPanelsResizable;
		#endregion
		
		#region ------- CONSTRUCTORS, DESIGNERCODE & OTHERS -------------------

		/// <summary>
		/// Def. constructor
		/// </summary>
		public CollapsiblePanelBar() : base()
		{
			InitializeComponent();

			this.BackColor = Color.CornflowerBlue;
			meBarType = PanelBarType.StandardTaskBar;
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{

		}
		#endregion

		#endregion

		#region ------- PUBLIC PROPERTIES--------------------------------------
//		private CollapsiblePanelCollection CollapsiblePanelCollection
//		{
//			get
//			{
//				return this.panels;
//			}
//		}

		/// <summary>
		/// Hozizontal space between panels
		/// </summary>
		public int Spacing
		{
			get
			{
				return this.miSpacing;
			}
			set
			{
				this.miSpacing = value;
				UpdatePositions();
			}
		}

		/// <summary>
		/// Border around panels
		/// </summary>
		public int Border
		{
			get
			{
				return this.miBorder;
			}
			set
			{
				this.miBorder = value;
				UpdatePositions();
			}
		}

		/// <summary>
		/// If the bartype is TwoPaneSplitter this property tells
		/// the control whether the user can resize the two panels
		/// </summary>
		public bool PanelsResizable
		{
			get
			{
				return this.mbPanelsResizable;
			}
			set
			{
				this.mbPanelsResizable = value;
			}
		}


		/// <summary>
		/// Type of the panel
		/// </summary>
		public PanelBarType BarType
		{
			get
			{
				return meBarType;
			}
			set
			{
				meBarType = value;
				UpdatePositions();
			}
		}
		#endregion

		#region ------- PUBLIC METHODS ----------------------------------------
		/// <summary>
		/// Signals the object that initialization is starting.
		/// </summary>
		public void BeginInit()
		{
			this.mbInitializing = true;
		}

		/// <summary>
		/// Signals the object that initialization is complete.
		/// </summary>
		public void EndInit()
		{
			this.mbInitializing = false;
		}
		
		/// <summary>
		/// Expand all panels and make them equal height (using the full client area of the bar)
		/// </summary>
		public void DistributePanelsEqualy()
		{
			int iHeight;
			int i;
			CollapsiblePanel oPanel;

			if (panels.Count >0)
			{

				iHeight = (this.ClientSize.Height - 
					2 * miBorder - 
					(panels.Count-1)*miSpacing)   / panels.Count;

				for (i=0; i<panels.Count;i++)
				{
					oPanel = panels.Item(i);
					oPanel.ExpandedHeight = iHeight;
					if (oPanel.IsExpanded)
					{
						oPanel.Height = oPanel.ExpandedHeight;
					}
				}
				UpdatePositions();
			}
		}


		/// <summary>
		/// Collapse all collapsible panels
		/// </summary>
		public void CollapseAll() {

			if (this.panels.Count > 0) {

				foreach (CollapsiblePanel panel in this.panels) {
					if (panel.PanelState == PanelState.Expanded)
						panel.PanelState = PanelState.Collapsed;
				}
				UpdatePositions();

			}
		}

		/// <summary>
		/// Expand all collapsible panels to their ExpandedHeight.
		/// </summary>
		/// <returns></returns>
		public void ExpandAll() {
			if (this.panels.Count > 0) {

				foreach (CollapsiblePanel panel in this.panels) {
					if (panel.PanelState == PanelState.Collapsed)
						panel.PanelState = PanelState.Expanded;
				}
				UpdatePositions();

			}
		}

		/// <summary>
		/// Refreshes the position and size of the panels
		/// </summary>
		public void RefreshLayout()
		{
			UpdatePositions();
		}

		#endregion

		#region ------- PRIVATE METHODS ---------------------------------------
		/// <summary>
		/// Update size and position of the panels depending on the BarType
		/// </summary>
		private void UpdatePositions()
		{
			CollapsiblePanel oCurPanel;
			int iStretchPanelIndex;
			int iHeight;

			//There is a LOOOOOOOOOOOT of thinking in this routine
			//Think very well before you change anything
			//Especially take care for cases like:
			// Both panels collapsed, then expanded
			// Changing size in designer
			// Changing size with splitter
			// ...

			iStretchPanelIndex = -1;
			iHeight = 2 * miBorder;
			if (meBarType == PanelBarType.FilledTaskBar)
			{
				//Find the panel that has to be stretched and set it's expandedheight
				//We start from the bottom and find the first one that is expanded
				for(int i = 0; i < panels.Count; i++)
				{
					oCurPanel = panels.Item(i);

					//First revert the panel back to it's original state
					//It could have been stretched before!!
					if (oCurPanel.IsExpanded)
					{
						oCurPanel.Height = oCurPanel.ExpandedHeight;
					}

					if ((iStretchPanelIndex == -1) && 
						(oCurPanel.PanelState == PanelState.Expanded) && 
						(! oCurPanel.FixedSize))
					{
						iStretchPanelIndex = i;
					}
					else
					{
						iHeight = iHeight + oCurPanel.Height + miSpacing;
					}
				}

				if (iStretchPanelIndex >= 0)
				{
					//DON'T set the expanded height here as this is a stretched height!! 
					oCurPanel = panels.Item(iStretchPanelIndex);
					oCurPanel.Height = this.ClientSize.Height - iHeight;
				}
			}

			//Now reposition
			//The topmost panel is has the highest index, so we start from the end of the collection
			for(int i = panels.Count-1; i >=0; i--)
			{
				oCurPanel = this.panels.Item(i);
				// Update the panel locations.
				if(i == this.panels.Count - 1)
				{
					// Top panel.
					oCurPanel.Top = this.miBorder;
				}
				else
				{
					oCurPanel.Top = this.panels.Item(i + 1).Bottom + this.miSpacing;
				} 

				SetPanelEdges(oCurPanel);

				//Set Anchor
				if ((i > iStretchPanelIndex) || (iStretchPanelIndex == -1))
				{
					//Above stretchpanel or no stretchpanel available
					oCurPanel.Anchor = (System.Windows.Forms.AnchorStyles)
						((System.Windows.Forms.AnchorStyles.Top) 
						| (System.Windows.Forms.AnchorStyles.Left) 
						| (System.Windows.Forms.AnchorStyles.Right));
				}
				else if (iStretchPanelIndex == i)
				{
					//The strechtpanel
					oCurPanel.Anchor = (System.Windows.Forms.AnchorStyles)
						((System.Windows.Forms.AnchorStyles.Top) 
						| (System.Windows.Forms.AnchorStyles.Bottom)
						| (System.Windows.Forms.AnchorStyles.Left) 
						| (System.Windows.Forms.AnchorStyles.Right));
				}
				else
				{
					//The strechtpanel
					oCurPanel.Anchor = (System.Windows.Forms.AnchorStyles)
						((System.Windows.Forms.AnchorStyles.Bottom)
						| (System.Windows.Forms.AnchorStyles.Left) 
						| (System.Windows.Forms.AnchorStyles.Right));
				}

				
				
			}
		}

		private void SetPanelEdges(CollapsiblePanel Panel)
		{
			Panel.Left = this.miBorder;
			Panel.Width = this.ClientSize.Width - (2 * this.miBorder);

//			if(this.VScroll)
//			{
//				Panel.Width -= SystemInformation.VerticalScrollBarWidth;
//			}	
		}


		

		#endregion
	
		#region ------- EVENT HANDLERS ----------------------------------------
		/// <summary>
		/// Handles adding of a CollapsiblePanel
		/// </summary>
		protected override void OnControlAdded(ControlEventArgs e)
		{
			CollapsiblePanel oPanel;

			base.OnControlAdded(e);

			if(e.Control is CollapsiblePanel)
			{
				oPanel = e.Control as CollapsiblePanel;
				
				//Anchor the panel at the top by default
				oPanel.Anchor = ((System.Windows.Forms.AnchorStyles)
					(System.Windows.Forms.AnchorStyles.Top
					| System.Windows.Forms.AnchorStyles.Left) 
					| System.Windows.Forms.AnchorStyles.Right);

				//Add it to the controls collection
				//Needed for the UIformatter
				this.Controls.Add(e.Control);
				if(true == mbInitializing)
				{
					// In the middle of InitializeComponent call.
					// Generated code adds panels in reverse order, so add to end
					this.panels.Add((CollapsiblePanel)e.Control);

					this.panels.Item(this.panels.Count - 1).PanelStateChanged +=
						new CollapsiblePanel.PanelStateChangedEventHandler(this.panel_StateChanged);
				}
				else
				{
					// Add the panel to the beginning of the internal collection.
					panels.Insert(0, (CollapsiblePanel)e.Control);

					panels.Item(0).PanelStateChanged += 
						new CollapsiblePanel.PanelStateChangedEventHandler(this.panel_StateChanged);
					// Update the size and position of the panels
					//Only necessary in design mode
					UpdatePositions();
				}
				
			}
		}

		/// <summary>
		/// Handles removing of a CollapsiblePanel
		/// </summary>
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);

			if(e.Control is CollapsiblePanel)
			{
				// Get the index of the panel within the collection.
				int index = this.panels.IndexOf((CollapsiblePanel)e.Control);
				if(-1 != index)
				{
					// Remove this panel from the collection.
					this.panels.Remove(index);
					// Update the position of any remaining panels.
					UpdatePositions();
				}
			}
		}

		private void panel_StateChanged(object sender, PanelEventArgs e)
		{
			int index;
			index = panels.IndexOf(e.CollapsiblePanel);
			if (index != -1)
			{
				UpdatePositions();
			}
		}

		#endregion
	
	}
}
