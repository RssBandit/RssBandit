#region CVS Version Header
/*
 * $Id: CollapsiblePanel.cs,v 1.1 2004/01/20 17:15:43 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2004/01/20 17:15:43 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using RssBandit.WinGui.Utility;


namespace RssBandit.WinGui.Controls.CollapsiblePanels
{
	/// <summary>
	/// Enum describing the state of a panel
	/// </summary>
	public enum PanelState
	{
		/// <summary>
		/// 
		/// </summary>
		Expanded = 0,
		/// <summary>
		/// 
		/// </summary>
		Collapsed = 1
	}

	/// <summary>
	/// A collapsible panel that can be placed in a CollapsiblePanelBar
	/// </summary>
	public class CollapsiblePanel : Panel
	{
		#region ------- EVENTS --------------------------------
		/// <summary>
		/// Event that is used to tell the PanelBar about a resize
		/// </summary>
		public event PanelStateChangedEventHandler PanelStateChanged;

		/// <summary>
		/// This event is raised when the status of the panel changes
		/// Allow the panelbar to resize and reposition the panels
		/// </summary>
		public delegate void PanelStateChangedEventHandler(object sender, PanelEventArgs e);
		#endregion
			
		

		#region ------- ENUMS & OTHERS ----------------------------------------
		private const int IconBorder = 3;
		private const int ExpandBorder = 4;
		#endregion
		
		#region ------- PRIVATE MEMBERS ---------------------------------------
		//Used to draw image transparent
		private System.Drawing.Imaging.ImageAttributes moImageTranspAttribute;  
		private System.Drawing.Imaging.ImageAttributes moImageListTranspAttribute;  
	
		private PanelState meCurrentState;
		private int miExpandedHeight;	
		private int miMinTitleHeight;
		private bool mbFixedSize;
		private Point pt = new Point(0,0);

		private System.Drawing.Imaging.ColorMatrix grayMatrix;
		private System.Drawing.Imaging.ImageAttributes grayAttributes;
		private System.Drawing.Color mcStartColor = Color.White;
		private System.Drawing.Color mcEndColor = Color.FromArgb(199, 212, 247);
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label lblTitle;
		private System.Drawing.Image mxImage;
		private System.Windows.Forms.ToolTip MainToolTip;
		private System.Windows.Forms.Label lblSplitter;
		private System.Windows.Forms.ImageList mxImageList;
		#endregion
		
		#region ------- CONSTRUCTORS, DESIGNERCODE & OTHERS -------------------
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CollapsiblePanel));
			this.lblTitle = new System.Windows.Forms.Label();
			this.mxImageList = new System.Windows.Forms.ImageList(this.components);
			this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.lblSplitter = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.BackColor = System.Drawing.SystemColors.Control;
			this.lblTitle.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTitle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.ForeColor = System.Drawing.Color.Navy;
			this.lblTitle.Location = new System.Drawing.Point(0, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(200, 24);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Title";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblTitle.Paint += new System.Windows.Forms.PaintEventHandler(this.labelTitle_Paint);
			this.lblTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelTitle_MouseUp);
			this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelTitle_MouseMove);
			// 
			// mxImageList
			// 
			this.mxImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
			this.mxImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.mxImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mxImageList.ImageStream")));
			this.mxImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// lblSplitter
			// 
			this.lblSplitter.BackColor = System.Drawing.SystemColors.Control;
			this.lblSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.lblSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblSplitter.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblSplitter.ForeColor = System.Drawing.Color.Navy;
			this.lblSplitter.Location = new System.Drawing.Point(311, 0);
			this.lblSplitter.Name = "lblSplitter";
			this.lblSplitter.Size = new System.Drawing.Size(200, 2);
			this.lblSplitter.TabIndex = 0;
			this.lblSplitter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblSplitter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseUp);
			this.lblSplitter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseMove);
			this.lblSplitter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseDown);
			// 
			// CollapsiblePanel
			// 
			this.Controls.Add(this.lblTitle);
			this.SizeChanged += new System.EventHandler(this.CollapsiblePanel_SizeChanged);
			this.BackColorChanged += new System.EventHandler(this.CollapsiblePanel_BackColorChanged);
			this.ResumeLayout(false);

		}
	
		/// <summary>
		/// Def. Constructor
		/// </summary>
		public CollapsiblePanel() : base()
		{
			this.components = new System.ComponentModel.Container();
			InitializeComponent();

			//Important. IF the label is not in the controls it will not be visible
			this.Controls.Add(this.lblSplitter);

			if (IsExpanded)
			{
				this.ExpandedHeight = this.Height;
			}

			MinTitleHeight = 22;

			this.BackColor = System.Drawing.SystemColors.Window;

			moImageTranspAttribute = new System.Drawing.Imaging.ImageAttributes();
			moImageTranspAttribute.SetColorKey(mxImageList.TransparentColor, mxImageList.TransparentColor);

			moImageListTranspAttribute = new System.Drawing.Imaging.ImageAttributes();
			ColorMap[] cm = new ColorMap[1];
			cm[0] = new ColorMap();
			cm[0].OldColor = mxImageList.TransparentColor;
			cm[0].NewColor = Color.Transparent;
			moImageListTranspAttribute.SetRemapTable(cm);

			// Setup the ColorMatrix and ImageAttributes for grayscale images.
			this.grayMatrix = new ColorMatrix();
			this.grayMatrix.Matrix00 = 1/3f;
			this.grayMatrix.Matrix01 = 1/3f;
			this.grayMatrix.Matrix02 = 1/3f;
			this.grayMatrix.Matrix10 = 1/3f;
			this.grayMatrix.Matrix11 = 1/3f;
			this.grayMatrix.Matrix12 = 1/3f;
			this.grayMatrix.Matrix20 = 1/3f;
			this.grayMatrix.Matrix21 = 1/3f;
			this.grayMatrix.Matrix22 = 1/3f;
			this.grayAttributes = new ImageAttributes();
			this.grayAttributes.SetColorMatrix(this.grayMatrix, ColorMatrixFlag.Default,
				ColorAdjustType.Bitmap);
		}

		#endregion
		
		#region ------- PROTECTED & OTHERS ------------------------------------
		private void OnPanelStateChanged(PanelEventArgs e)
		{
			if(PanelStateChanged != null)
			{
				PanelStateChanged(this, e);
			}
		}

		#endregion
		
		#region ------- PUBLIC PROPERTIES--------------------------------------
		/// <summary>
		/// Current state of the panel
		/// </summary>
		public PanelState PanelState
		{
			get
			{
				return this.meCurrentState;
			}
			set
			{
				PanelState oldState = this.meCurrentState;
				this.meCurrentState = value;
				if(oldState != this.meCurrentState)
				{
					// update the display
					UpdatePanelState();
				}
			}
		}

		/// <summary>
		/// Expanded height is the height of the panel if it is regularly expanded (not stretched)
		/// </summary>
		public int ExpandedHeight
		{
			get
			{
				return miExpandedHeight;
			}
			set
			{
				miExpandedHeight = value;
			}
		}

		/// <summary>
		/// Is the panel currently in expanded state?
		/// </summary>
		public bool IsExpanded
		{
			get
			{
				return (meCurrentState == PanelState.Expanded);
			}
		}
		/// <summary>
		/// If a panel has fixed size it cannot be stretched (if the panelbar is in fill mode)
		/// </summary>
		public bool FixedSize
		{
			get
			{
				return mbFixedSize;
			}
			set
			{
				mbFixedSize = value;
				lblSplitter.Visible = ! mbFixedSize;
			}
		}
		/// <summary>
		/// Titletext of the panel
		/// </summary>
		public string TitleText
		{
			get
			{
				return this.lblTitle.Text;
			}
			set
			{
				this.lblTitle.Text = value;
			}
		}

		/// <summary>
		/// Color for the title-text
		/// </summary>
		public Color TitleFontColour
		{
			get
			{
				return this.lblTitle.ForeColor;
			}
			set
			{
				this.lblTitle.ForeColor = value;
			}
		}

		/// <summary>
		/// Font for the title
		/// </summary>
		public Font TitleFont
		{
			get
			{
				return this.lblTitle.Font;
			}
			set
			{
				this.lblTitle.Font = value;
			}
		}

		/// <summary>
		/// ImageList of the Panel
		/// </summary>
		public ImageList ImageList
		{
			get
			{
				return this.mxImageList;
			}
			//			set
			//			{
			//				this.mxImageList = value;
			//				if(null != this.mxImageList)
			//				{
			//					if(this.mxImageList.Images.Count > 0)
			//					{
			//						this.miImageIndex = 0;
			//					}
			//				}
			//				else
			//				{
			//					this.miImageIndex = -1;
			//				}
			//			}
		}

		/// <summary>
		/// Start Color for the Title. The title is draw as gradient from startcolor to end color
		/// </summary>
		public Color StartColor
		{
			get
			{
				return this.mcStartColor;
			}
			set
			{
				this.mcStartColor = value;
				this.lblTitle.Invalidate();
			}
		}

		/// <summary>
		/// End Color for the Title. The title is draw as gradient from startcolor to end color
		/// </summary>
		public Color EndColor
		{
			get
			{
				return this.mcEndColor;
			}
			set
			{
				this.mcEndColor = value;
				this.lblTitle.Invalidate();
			}
		}

		/// <summary>
		/// Minimal height of the title bar
		/// </summary>
		public int MinTitleHeight
		{
			get
			{
				return this.miMinTitleHeight;
			}
			set
			{
				this.miMinTitleHeight = value;
				SetTitleHeight();
				this.lblTitle.Invalidate();
			}
		}

		/// <summary>
		/// An additional image displayed on the left side of the title bar
		/// </summary>
		public Image Image
		{
			get
			{
				return this.mxImage;
			}
			set
			{
				this.mxImage = value;
				SetTitleHeight();				
				this.lblTitle.Invalidate();
			}
		}
		
		#endregion

		#region ------- PRIVATE METHODS ---------------------------------------
		private void SetTitleHeight()
		{
			if (this.Image != null)
			{
				if (this.mxImage.Height < miMinTitleHeight)
				{
					this.lblTitle.Height = miMinTitleHeight;
				}
				else
				{
					this.lblTitle.Height = this.mxImage.Height;
				}
			}
			else
			{
				this.lblTitle.Height = miMinTitleHeight;
			}
		}
		private bool IsOverTitle(int xPos, int yPos)
		{
			// Get the dimensions of the title label
			Rectangle rectTitle = this.lblTitle.Bounds;
			// Check if the supplied coordinates are over the title label
			if(rectTitle.Contains(xPos, yPos))
			{
				return true;
			}
			return false;
		}

		private void UpdatePanelState()
		{
			switch(this.meCurrentState)
			{
				case PanelState.Collapsed :
					this.Height = lblTitle.Height;
					//Important: Hide the splitter, otherwise the user could resize in collapsed state...
					lblSplitter.Visible = false;
					break;
				case PanelState.Expanded :
					// Entering expanded State, so expand the panel.
					this.Height = this.miExpandedHeight;
					if (! mbFixedSize)
					{
						lblSplitter.Visible = true;
					}
					break;
				default :
					// Ignore
					break;
			}
			this.lblTitle.Invalidate();

			OnPanelStateChanged(new PanelEventArgs(this));

			//Set the new tooltip
			//Do this after the OnPanelStateChanged to prevent flickering
			switch(this.meCurrentState)
			{
				case PanelState.Collapsed :
					MainToolTip.SetToolTip(lblTitle, Resource.Manager["RES_ControlCollapsiblePanelExpandTooltip"]);
					break;
				case PanelState.Expanded :
					MainToolTip.SetToolTip(lblTitle, Resource.Manager["RES_ControlCollapsiblePanelCollapseTooltip"]);
					break;
			}
		}

		
		#endregion

		#region ------- EVENT HANDLERS ----------------------------------------
		private void labelTitle_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			const int diameter = 14;
			int radius = diameter / 2;
			Rectangle bounds = lblTitle.Bounds;
			int offsetY = 0;

			if(null != this.mxImage)
			{
				offsetY = this.lblTitle.Height - miMinTitleHeight;
				if(offsetY < 0)
				{
					offsetY = 0;
				}
				bounds.Offset(0, offsetY);
				bounds.Height -= offsetY;
			}

			e.Graphics.Clear(this.Parent.BackColor);

			// Create a GraphicsPath with curved top corners
			GraphicsPath path = new GraphicsPath();
			path.AddLine(bounds.Left + radius, bounds.Top, bounds.Right - diameter - 1, bounds.Top);
			path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
			path.AddLine(bounds.Right, bounds.Top + radius, bounds.Right, bounds.Bottom);
			path.AddLine(bounds.Right, bounds.Bottom, bounds.Left - 1, bounds.Bottom);
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);

			// Create a colour gradient
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if(true == this.Enabled)
			{
				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, this.mcStartColor, this.mcEndColor, LinearGradientMode.Horizontal);

				// Paint the colour gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}
			else
			{
				ColorEx grayStart = new ColorEx(this.mcStartColor);
				grayStart.Saturation = 0f;
				ColorEx grayEnd = new ColorEx(this.mcEndColor);
				grayEnd.Saturation = 0f;
				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, grayStart.CurrentColor, grayEnd.CurrentColor,
					LinearGradientMode.Horizontal);

				// Paint the grayscale gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}

			// Draw the header icon, if there is one
			System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
			int offsetX = CollapsiblePanel.IconBorder;
			if(null != this.mxImage)
			{
				offsetX += this.mxImage.Width + CollapsiblePanel.IconBorder;
				// <feature>Draws the title icon grayscale when the panel is disabled.
				// <version>1.4</version>
				// <date>25-Nov-2002</date>
				RectangleF srcRectF = this.mxImage.GetBounds(ref graphicsUnit);
				Rectangle destRect = new Rectangle(CollapsiblePanel.IconBorder,
					CollapsiblePanel.IconBorder, this.mxImage.Width, this.mxImage.Height);
				if(true == this.Enabled)
				{
					e.Graphics.DrawImage(this.mxImage, destRect, (int)srcRectF.Left, (int)srcRectF.Top,
						(int)srcRectF.Width, (int)srcRectF.Height, graphicsUnit, moImageTranspAttribute);
				}
				else
				{
					e.Graphics.DrawImage(this.mxImage, destRect, (int)srcRectF.Left, (int)srcRectF.Top,
						(int)srcRectF.Width, (int)srcRectF.Height, graphicsUnit, this.grayAttributes);
				}
			}

			// Draw the title text.
			SolidBrush textBrush = new SolidBrush(this.TitleFontColour);
			// <feature>Title text truncated with an ellipsis where necessary.
			float left = (float)offsetX;
			float top = (float)offsetY + (float)CollapsiblePanel.ExpandBorder;
			float width = (float)this.lblTitle.Width - left - this.mxImageList.ImageSize.Width - 
				CollapsiblePanel.ExpandBorder;
			//Only subtract one ExpandBorder, so characters like 'y' have enough room
			float height = (float)miMinTitleHeight - ((float)CollapsiblePanel.ExpandBorder);
			RectangleF textRectF = new RectangleF(left, top, width, height);
			StringFormat format = new StringFormat();
			format.Trimming = StringTrimming.EllipsisWord;
			// <feature>Draw title text disabled where appropriate.
			if(true == this.Enabled)
			{
				e.Graphics.DrawString(lblTitle.Text, lblTitle.Font, textBrush, 
					textRectF, format);
			}
			else
			{
				Color disabled = SystemColors.GrayText;
				ControlPaint.DrawStringDisabled(e.Graphics, lblTitle.Text, lblTitle.Font,
					disabled, textRectF, format);
			}

			//			// Draw a white line at the bottom:
			const int lineWidth = 1;
			SolidBrush lineBrush = new SolidBrush(Color.White);
			Pen linePen = new Pen(lineBrush, lineWidth);
			path.Reset();
			path.AddLine(bounds.Left, bounds.Bottom - lineWidth, bounds.Right, 
				bounds.Bottom - lineWidth);
			e.Graphics.DrawPath(linePen, path);

			// Draw the expand/collapse image
			int xPos = bounds.Right - this.mxImageList.ImageSize.Width - CollapsiblePanel.IconBorder;
			int yPos = (bounds.Height - this.mxImageList.ImageSize.Height)/ 2;
			RectangleF srcIconRectF = this.mxImageList.Images[(int)this.meCurrentState].GetBounds(ref graphicsUnit);
			Rectangle destIconRect = new Rectangle(xPos, yPos, 
				this.mxImageList.ImageSize.Width, this.mxImageList.ImageSize.Height);
			if(true == this.Enabled)
			{
				e.Graphics.DrawImage(this.mxImageList.Images[(int)this.meCurrentState], destIconRect,
					(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
					(int)srcIconRectF.Height, graphicsUnit, moImageListTranspAttribute);
			}
			else
			{
				e.Graphics.DrawImage(this.mxImageList.Images[(int)this.meCurrentState], destIconRect,
					(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
					(int)srcIconRectF.Height, graphicsUnit, this.grayAttributes);
			}
		}

		private void labelTitle_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.Left) && (true == IsOverTitle(e.X, e.Y)))
			{
				if((null != this.mxImageList) && (this.mxImageList.Images.Count >=2))
				{
					if(this.meCurrentState == PanelState.Expanded)
					{
						this.meCurrentState = PanelState.Collapsed;
					}
					else
					{
						this.meCurrentState = PanelState.Expanded;
					}
					UpdatePanelState();
				}
			}
		}

		private void labelTitle_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.None) && (true == IsOverTitle(e.X, e.Y)))
			{
				this.lblTitle.Cursor = Cursors.Hand;
			}
			else
			{
				this.lblTitle.Cursor = Cursors.Default;
			}
		}
		private void CollapsiblePanel_SizeChanged(object sender, System.EventArgs e)
		{
			if (DesignMode)
			{
				if (IsExpanded)
				{
					//Adjust the expandedheight if the panel is resized in the designer
					miExpandedHeight = this.Height;
				}
			}
		}


		private void lblSplitter_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (lblSplitter.Visible)
			{
				lblSplitter.Capture = true;
				pt.X = e.X;
				pt.Y = e.Y;
			}
		}

		private void lblSplitter_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ((lblSplitter.Visible ) && (e.Button == MouseButtons.Left))
			{
				this.Height = this.Height + e.Y - pt.Y;
				if (miExpandedHeight != this.Height)
				{
					//Only fire OnPanelStateChanged if there is a real change
					//This helps to prevent flickering
					miExpandedHeight = this.Height;
					OnPanelStateChanged(new PanelEventArgs(this));
				}
			}
		}

		private void lblSplitter_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (lblSplitter.Visible)
			{
				lblSplitter.Capture = false;
				OnPanelStateChanged(new PanelEventArgs(this));
			}
		}

		private void CollapsiblePanel_BackColorChanged(object sender, System.EventArgs e)
		{
			lblSplitter.BackColor = this.BackColor;
		}
		#endregion


	}

	
	/// <summary>
	/// EventArgs for the PanelStateChanged Event
	/// </summary>
	public class PanelEventArgs : System.EventArgs
	{
		private CollapsiblePanel panel;

		/// <summary>
		/// Def. Constructore
		/// </summary>
		/// <param name="sender"></param>
		public PanelEventArgs(CollapsiblePanel sender)
		{
			this.panel = sender;
		}

		/// <summary>
		/// The panel that fired the event
		/// </summary>
		public CollapsiblePanel CollapsiblePanel
		{
			get
			{
				return this.panel;
			}
		}

		/// <summary>
		/// The state of the panel
		/// </summary>
		public PanelState PanelState
		{
			get
			{
				return this.panel.PanelState;
			}
		}
	}



}
