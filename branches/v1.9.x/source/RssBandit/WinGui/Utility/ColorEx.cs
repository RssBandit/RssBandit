#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Drawing;
using Infragistics.Win;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// Stores a color and provides conversion between the 
	/// RGB and HLS color models.
	/// </summary>
	public class ColorEx
	{
		public static readonly Color[] OneNoteColors = new Color[] 
		{ 
			FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderGradientLight,
			Color.FromArgb(145, 186, 174), 
			Color.FromArgb(246, 176, 120),
			Color.FromArgb(213, 164, 187), 
			Color.FromArgb(180, 158, 222), 
			Color.FromArgb(238, 149, 151), 
			Color.FromArgb(183, 201, 151),
			Color.FromArgb(138, 168, 228), 
			Color.FromArgb(255, 216, 105),
		};

		/// <summary> Maximum Hue </summary>
		public const int HUEMAX = 360;
		/// <summary> Maximum Saturation </summary>
		public const float SATMAX = 1.0f;
		/// <summary> Maximum Brightness </summary>
		public const float BRIGHTMAX = 1.0f;
		/// <summary> Maximum RBG </summary>
		public const int RGBMAX	= 255;

		// Member variables
		private Color	m_clrCurrent = Color.Red;

		/// <summary>
		/// Default constructor
		/// </summary>
		public ColorEx()	{;	}
		/// <summary>
		/// Constructor with a Color
		/// </summary>
		/// <param name="initialColor"> Color to initializ with</param>
		public ColorEx(Color initialColor)	{ this.m_clrCurrent = initialColor;	}

		/// <summary>
		/// The current Color (RGB model)
		/// </summary>
		public Color CurrentColor	{
			get	{	return m_clrCurrent;	}
			set	{	m_clrCurrent = value;	}
		}


		/// <summary>
		/// The Red component of the current Color
		/// </summary>
		public byte Red	{
			get	{	return m_clrCurrent.R;}
			set	{	m_clrCurrent = Color.FromArgb(value, Green, Blue);	}
		}

		/// <summary>
		/// The Green component of the current Color
		/// </summary>
		public byte Green	{
			get	{	return m_clrCurrent.G;	}
			set	{	m_clrCurrent = Color.FromArgb(Red, value, Blue);		}
		}

		/// <summary>
		/// The Blue component of the current Color
		/// </summary>
		public byte Blue {
			get	{	return m_clrCurrent.B;	}
			set	{	m_clrCurrent = Color.FromArgb(Red, Green, value);		}
		}

		/// <summary>
		/// The Hue component of the current Color
		/// </summary>
		public int Hue {
			get	{	return (int)m_clrCurrent.GetHue();	}
			set	{
				m_clrCurrent = ColorEx.HSBToRGB(value,
					m_clrCurrent.GetSaturation(),
					m_clrCurrent.GetBrightness());
			}
		}

		/// <summary>
		/// Get the Hue of the Color
		/// </summary>
		/// <returns>Hue value</returns>
		public float GetHue()	{
			float top = ((float)(2 * Red - Green - Blue)) / (2 * 255);
			float bottom = (float)Math.Sqrt(((Red - Green) * (Red - Green) + (Red - Blue) * (Green - Blue)) / 255);
			return (float)Math.Acos(top / bottom);
		}

		/// <summary>
		/// Get the Saturation of the Color
		/// </summary>
		/// <returns></returns>
		public float GetSaturation() {
			return (255 -
				(((float)(Red + Green + Blue)) / 3) * Math.Min(Red, Math.Min(Green, Blue))) / 255;
		}

		/// <summary>
		/// Get Brighness of the Color
		/// </summary>
		/// <returns></returns>
		public float GetBrightness()	{
			return ((float)(Red + Green + Blue)) / (255.0f * 3.0f);
		}

		/// <summary>
		/// The Saturation component of the current Color
		/// </summary>
		public float Saturation	{
			get	{
				if(0.0f == Brightness)
				{
					return 0.0f;
				}
				else
				{
					float fMax = (float)Math.Max(Red, Math.Max(Green, Blue));
					float fMin = (float)Math.Min(Red, Math.Min(Green, Blue));
					return (fMax - fMin) / fMax;
				}
			}
			set	{
				m_clrCurrent = ColorEx.HSBToRGB((int)m_clrCurrent.GetHue(),
					value, m_clrCurrent.GetBrightness());
			}
		}

		/// <summary>
		/// The Brightness component of the current Color
		/// </summary>
		public float Brightness	{
			get	{
				//return m_clrCurrent.GetBrightness();
				return (float)Math.Max(Red, Math.Max(Green, Blue)) / (255.0f);
			}
			set	{
				m_clrCurrent = ColorEx.HSBToRGB((int)m_clrCurrent.GetHue(),
					m_clrCurrent.GetSaturation(),
					value);
			}
		}

		/// <summary>
		/// Converts HSB Color components to an RGB System.Drawing.Color
		/// </summary>
		/// <param name="Hue">Hue component</param>
		/// <param name="Saturation">Saturation component</param>
		/// <param name="Brightness">Brightness component</param>
		/// <returns>Returns the RGB value as a System.Drawing.Color</returns>
		public static Color HSBToRGB(int Hue, float Saturation, float Brightness)	{
			// TODO: CheckHSBValues(Hue, Saturation, Brightness);
			int red = 0; int green = 0; int blue = 0;
			if(Saturation == 0.0f)
			{
				// Achromatic Color (black and white centre line)
				// Hue should be 0 (undefined), but we'll ignore it.
				// Set shade of grey
				red = green = blue = (int)(Brightness * 255);
			}
			else
			{
				// Chromatic Color
				// Map hue from [0-255] to [0-360] to hexagonal-space [0-6]
				// (360 / 256) * hue[0-255] / 60
				float fHexHue = (6.0f / 360.0f) * Hue;
				// Determine sector in hexagonal-space (RGB cube projection) {0,1,2,3,4,5}
				float fHexSector = (float)Math.Floor((double)fHexHue);
				// Determine exact position in particular sector [0-1]
				float fHexSectorPos = fHexHue - fHexSector;

				// Convert parameters to in-formula ranges
				float fBrightness = Brightness * 255.0f;
				float fSaturation = Saturation/*(float)Saturation * (1.0f / 360.0f)*/;

				// Magic formulas (from Foley & Van Dam). Adding 0.5 performs rounding instead of truncation
				byte bWashOut = (byte)(0.5f + fBrightness * (1.0f - fSaturation));
				byte bHueModifierOddSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * fHexSectorPos));
				byte bHueModifierEvenSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * (1.0f - fHexSectorPos)));

				// Assign values to RGB components (sector dependent)
				switch((int)fHexSector)	{
					case 0 :
						// Hue is between red & yellow
						red = (int)(Brightness * 255); green = bHueModifierEvenSector; blue = bWashOut;
						break;
					case 1 :
						// Hue is between yellow & green
						red = bHueModifierOddSector; green = (int)(Brightness * 255); blue = bWashOut;
						break;
					case 2 :
						// Hue is between green & cyan
						red = bWashOut; green = (int)(Brightness * 255); blue = bHueModifierEvenSector;
						break;
					case 3 :
						// Hue is between cyan & blue
						red = bWashOut; green = bHueModifierOddSector; blue = (int)(Brightness * 255);
						break;
					case 4 :
						// Hue is between blue & magenta
						red = bHueModifierEvenSector; green = bWashOut; blue = (int)(Brightness * 255);
						break;
					case 5 :
						// Hue is between magenta & red
						red = (int)(Brightness * 255); green = bWashOut; blue = bHueModifierOddSector;
						break;
					default :
						red = 0; green = 0; blue = 0;
						break;
				}
			}

			return Color.FromArgb(red, green, blue);
		}

		/// <summary>
		/// Colorize a control (BackColor) like the Tabs of Microsoft's OneNote.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="index"></param>
		public static void ColorizeOneNote(System.Windows.Forms.Control control, int index) {
			if (control != null) {	
				control.BackColor = OneNoteColors[(index % OneNoteColors.Length)];
			}
		}
		
	}
}
