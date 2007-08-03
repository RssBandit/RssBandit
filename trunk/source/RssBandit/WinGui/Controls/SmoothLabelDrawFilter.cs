#region CVS Version Header
/*
 * $Id: SmoothLabelDrawFilter.cs,v 1.2 2006/08/03 19:16:48 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/08/03 19:16:48 $
 * $Revision: 1.2 $
 */
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// Helper to draw the Details Header Caption Label Text Font
	/// anti-aliased (smooth).
	/// </summary>
	class SmoothLabelDrawFilter : IUIElementDrawFilter {
		private UltraLabel label;
		bool renderVertical;
		
		public SmoothLabelDrawFilter(UltraLabel label):
			this(label, false) {
		}

		public SmoothLabelDrawFilter(UltraLabel label, bool renderVertical) {
			this.label = label;
			this.renderVertical = renderVertical;
		}

		#region IUIElementDrawFilter Members
		public DrawPhase GetPhasesToFilter(ref UIElementDrawParams drawParams) {
			// Indicates that we want this filter's DrawElement method to be invoked prior to when the
			// foreground (text) of the control is drawn.
			return drawParams.Element is TextUIElementBase ? DrawPhase.BeforeDrawForeground : DrawPhase.None;
		}

		public bool DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams) {

			Graphics g = drawParams.Graphics;
			StringFormat frmt  = new StringFormat();
			
			g.SmoothingMode = SmoothingMode.AntiAlias;

			if (this.renderVertical)
				frmt.FormatFlags = StringFormatFlags.DirectionVertical;
			
			if (!this.label.WrapText)
				frmt.FormatFlags |= StringFormatFlags.NoWrap;
			if (this.label.RightToLeft == RightToLeft.Yes)
				frmt.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

			// The TextHAlign set on the label must be honored, so we setup a StringFormat
			// to use when drawing the text.
			HAlign       align = drawParams.AppearanceData.TextHAlign;
			if( align == HAlign.Left )
				frmt.Alignment = StringAlignment.Near;
			else 
				if( align == HAlign.Center )
				frmt.Alignment = StringAlignment.Center;
			else 
				if( align == HAlign.Right )
				frmt.Alignment = StringAlignment.Far;

			VAlign       valign = drawParams.AppearanceData.TextVAlign;
			if( valign == VAlign.Top )
				frmt.LineAlignment = StringAlignment.Near;
			else 
				if( valign == VAlign.Middle )
				frmt.LineAlignment = StringAlignment.Center;
			else 
				if( valign == VAlign.Bottom )
				frmt.LineAlignment = StringAlignment.Far;

			TextTrimming t = this.label.Appearance.TextTrimming;
			if (t == TextTrimming.Character)
				frmt.Trimming = StringTrimming.Character;
			else
				if (t == TextTrimming.EllipsisCharacter)
				frmt.Trimming = StringTrimming.EllipsisCharacter;
			else
				if (t == TextTrimming.EllipsisPath)
				frmt.Trimming = StringTrimming.EllipsisPath;
			else
				if (t == TextTrimming.EllipsisWord)
				frmt.Trimming = StringTrimming.EllipsisWord;
			else
				if (t == TextTrimming.Word)
				frmt.Trimming = StringTrimming.Word;

			if (Win32.IsOSAtLeastWindowsXP) {
				g.TextRenderingHint = TextRenderingHint.SystemDefault;
			} else {
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			}

			Rectangle r = drawParams.Element.RectInsideBorders;
				
			r.X += this.label.Padding.Width;
			r.Width -= 2 * this.label.Padding.Width;
			r.Y += this.label.Padding.Height;
			r.Height -= 2 * this.label.Padding.Height;

			// Draw the text. drawParams.Font is the Appearance.Font, while label.Font is the Control.Font
			g.DrawString( 
				this.label.Text, 
				drawParams.Font, 
				drawParams.TextBrush, 
				drawParams.Element.RectInsideBorders,
				frmt
				);

			return true;

		}
		#endregion IUIElementDrawFilter Members
	}


}
