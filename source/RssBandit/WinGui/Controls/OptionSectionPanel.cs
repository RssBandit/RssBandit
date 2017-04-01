#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// OptionSectionPanel mimics the option panels of Outlook 2003
	/// (Caption text, a line on the right and optional an image left below).
	/// </summary>
	[DefaultProperty("Text")]
	public class OptionSectionPanel : System.Windows.Forms.Panel
	{
		private Color	_highlight		= Color.FromKnownColor( KnownColor.ControlLightLight );
		private Color	_shadow			= Color.FromKnownColor( KnownColor.ControlDark );
		private Image	_image			= null;
		private Point	_imageLocation;
		private System.ComponentModel.IContainer components = null;
        readonly float scaleFactor;

		/// <summary>
		/// Initializer.
		/// </summary>
		public OptionSectionPanel()
		{
            scaleFactor = (float)DeviceDpi / 96;
            _imageLocation = new Point(0, 20);
			SetStyle(ControlStyles.DoubleBuffer, true);
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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

		protected override void OnPaintBackground(PaintEventArgs pe) {

			base.OnPaintBackground (pe);
			
			StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces|StringFormatFlags.NoWrap);
			Graphics	g	= pe.Graphics;
			SizeF lt = g.MeasureString(this.Text + " ", this.Font, this.Width, sf);
			float lth = lt.Height / 2;

			Brush		h	= new SolidBrush( _highlight );
			Pen		ph = new Pen(h, 1.0f);
			Brush		s	= new SolidBrush( _shadow );
			Pen		ps = new Pen(s, 1.0f);
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			g.DrawString(this.Text, this.Font, SystemBrushes.FromSystemColor(this.ForeColor) , new PointF(0,0), sf);
			g.DrawLine( ps, lt.Width, lth, Width,  lth );
			g.DrawLine( ph, lt.Width, lth + 1.0f, Width,  lth + 1.0f);
			
			if (this._image != null)
				g.DrawImage(this._image, _imageLocation.X * scaleFactor, _imageLocation.Y * scaleFactor, this._image.Width, this._image.Height);
			
			sf.Dispose();
			ph.Dispose();
			ps.Dispose();
			h.Dispose();
			s.Dispose();

		}

		#region for ref. (old)
//		protected override void OnPaint(PaintEventArgs pe) {
//			base.OnPaint (pe);
//
//			StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces|StringFormatFlags.NoWrap);
//			Graphics	g	= pe.Graphics;
//			SizeF lt = g.MeasureString(this.Text + " ", this.Font, this.Width, sf);
//			float lth = lt.Height / 2;
//
//			Brush		h	= new SolidBrush( _highlight );
//			Pen		ph = new Pen(h, 1.0f);
//			Brush		s	= new SolidBrush( _shadow );
//			Pen		ps = new Pen(s, 1.0f);
//			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
//			g.DrawString(this.Text, this.Font, SystemBrushes.FromSystemColor(this.ForeColor) , new PointF(0,0), sf);
//			g.DrawLine( ps, lt.Width, lth, Width,  lth );
//			g.DrawLine( ph, lt.Width, lth + 1.0f, Width,  lth + 1.0f);
//			
//			if (this._image != null)
//				g.DrawImage(this._image, 0, 20, this._image.Width, this._image.Height);
//			
//			sf.Dispose();
//			ph.Dispose();
//			ps.Dispose();
//			h.Dispose();
//			s.Dispose();
//			
//		}
		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// OptionSectionPanel
			// 
			this.Size = new System.Drawing.Size((int)(scaleFactor * 220), (int)(150*scaleFactor));

		}
		#endregion

		#region public properties
		
		/// <summary>
		/// Set/Get the image to be displayed for this option
		/// </summary>
		[
		Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		DefaultValue(null),
		Category("Appearance")
		]
		public Image Image {
			get { return this._image;	}
			set { 
				this._image = value?.GetImageStretchedDpi(scaleFactor);	
				this.Invalidate();
			}
		}

		/// <summary>
		/// Set/Get the section caption text
		/// </summary>
		[
		Browsable(true), Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		DefaultValue(null),
		Category("Appearance")
		]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
				this.Invalidate();
			}
		}


		/// <summary>
		/// Set/Get the Image Location.
		/// </summary>
		[
		Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		Category("Layout")
		]
		public Point ImageLocation {
			get {
				return _imageLocation;
			}
			set {
				_imageLocation = value;
				this.Invalidate();
			}
		}

		#endregion

	}
}
