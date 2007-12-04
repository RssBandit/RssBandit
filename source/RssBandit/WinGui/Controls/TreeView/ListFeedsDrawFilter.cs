#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Drawing;
using Infragistics.Win;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui
{
	public class ListFeedsDrawFilter : IUIElementDrawFilter
	{
		DrawPhase IUIElementDrawFilter.GetPhasesToFilter( ref UIElementDrawParams drawParams )
		{
			if(drawParams.Element is EditorWithTextDisplayTextUIElement )
				return DrawPhase.BeforeDrawForeground; //Middle side
			//
			return DrawPhase.None;
		}

		private int GetFlagImageIndex(Flagged flagStatus)
		{
			int imgIndex = 0;
			switch (flagStatus) 
			{
				case Flagged.Complete:
					imgIndex = Resource.FlagImage.Complete;
					break;
				case Flagged.FollowUp:
					imgIndex = Resource.FlagImage.Red;
					break;
				case Flagged.Forward:
					imgIndex = Resource.FlagImage.Blue;
					break;
				case Flagged.Read:
					imgIndex = Resource.FlagImage.Green;
					break;
				case Flagged.Review:
					imgIndex = Resource.FlagImage.Yellow;
					break;
				case Flagged.Reply:
					imgIndex = Resource.FlagImage.Purple;
					break;
				case Flagged.None:
					imgIndex = Resource.FlagImage.Clear;
					break;
			}
			return imgIndex;
		}
		
		bool IUIElementDrawFilter.DrawElement( DrawPhase drawPhase, ref UIElementDrawParams drawParams )
		{
			const int FLAG_WIDTH = 23;
			UltraTreeNodeExtended treeNode = drawParams.Element.GetContext( typeof(UltraTreeNode), true ) as UltraTreeNodeExtended;
			if ( treeNode != null )
			{
				RectangleF initialRect = (RectangleF)drawParams.Element.RectInsideBorders;
				Rectangle r = new Rectangle((int)initialRect.Left,(int)initialRect.Top,(int)initialRect.Width,(int)initialRect.Height);
				r.X -= 2;
				r.Width += 2;
				r.Height = treeNode.ItemHeightResolved;
				Rectangle rO = r;
				//
				if(treeNode.Level==0)
				{
					Image img = null;
					if(treeNode.Expanded)
						img = Properties.Resources.Minus_16;
					else 
						img = Properties.Resources.Plus_16;
					
					if(!treeNode.Selected) {
						drawParams.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220,224,227)),r);
					}

					//Image
					drawParams.Graphics.DrawImage(img, r.Left+ 4, r.Top+ 5);
					treeNode.CollapseRectangle = new Rectangle(r.Left+ 4, r.Top+ 5, img.Width, img.Height); 
					//String
					Color c = Color.FromArgb(112, 111, 145);
					drawParams.Graphics.DrawString((string)treeNode.Cells[0].Value /*+ treeNode.DateTime.ToString()*/, new Font(drawParams.Font,FontStyle.Bold), 
					                               new SolidBrush(c), r.Left+ 20, r.Top+4);
					//Bottom Lines
					Pen pen = new Pen(Color.FromArgb(165, 164, 189));
					drawParams.Graphics.DrawLine(pen, r.Left,r.Bottom-1,r.Right,r.Bottom-1);
					drawParams.Graphics.DrawLine(pen, r.Left,r.Bottom,r.Right,r.Bottom);
					//
					return true;
				}
				if(treeNode.Level==1)
				{
					NewsItem item = treeNode.NewsItem;
					Brush bGray = new SolidBrush(Color.FromArgb(128, 128, 128));
					if(treeNode.Selected){
						bGray = Brushes.White;						
						drawParams.Graphics.FillRectangle(new SolidBrush(Color.DarkBlue),r);										
					}

					//Icon
					treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
					                                                r.Left + 7, r.Top + 3,
					                                                treeNode.NodeOwner.ImageIndex);
					//Flag
					Rectangle rF = r;
					rF.X = rF.Right - FLAG_WIDTH;
					rF.Width = FLAG_WIDTH;
					int flagInd = GetFlagImageIndex(item.FlagStatus);
					treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
						rF.Left + 2+(rF.Width-treeNode.NodeOwner.ListView.SmallImageList.ImageSize.Width)/2, 
						rF.Top + 2+ (rF.Height-treeNode.NodeOwner.ListView.SmallImageList.ImageSize.Height)/2,
						flagInd);
					treeNode.FlagRectangle = rF;
					//Date
					r.Width -= FLAG_WIDTH;
					r.Height = MeasureDisplayStringHeight(drawParams.Graphics, "W", treeNode.NodeOwner.Font);
					r.Y += 3;
					Rectangle r2 = r;
					r.X += 180;
					r.Width -= 180;
					string dtS = item.Date.ToLocalTime().ToShortDateString()+" "+item.Date.ToLocalTime().ToShortTimeString();

					if(((UltraTreeNodeExtended)treeNode.Parent).IsGroupOneDay)
						dtS = item.Date.ToLocalTime().ToString("HH:mm");

					int ww = MeasureDisplayStringWidth(drawParams.Graphics, dtS, treeNode.NodeOwner.Font);
					r.X = r.Right - ww;
					r.Width = ww+10;
					drawParams.Graphics.DrawString(dtS, treeNode.NodeOwner.Font, bGray, r);
					//Author
					r2.X += 26;
					r2.Width = r.X-r2.X;
					r = r2;
					string author = (StringHelper.EmptyOrNull(item.Author) ? item.Feed.title: item.Author);
					drawParams.Graphics.DrawString(author, treeNode.NodeOwner.Font, bGray, r);

					//Title
					r = rO;
					r.X += 26;
					r.Width -= 26+FLAG_WIDTH;
					if(item.CommentCount>0)
						r.Width -= 16;
					if(item.Enclosures!=null && item.Enclosures.Count>0)
						r.Width -= 16;
					r.Y += rO.Height / 2+2;
					r.Height = MeasureDisplayStringHeight(drawParams.Graphics, item.Title, treeNode.NodeOwner.Font);
					Brush titleBrush = (treeNode.Selected ? bGray : new SolidBrush(treeNode.NodeOwner.ForeColor));
					drawParams.Graphics.DrawString(item.Title, treeNode.NodeOwner.Font, titleBrush , r);
					//Comments
					int posAddons = 0;
					if(item.Enclosures!=null && item.Enclosures.Count>0)
					{
						Rectangle rC = r;
						rC.X = r.Right;
						rC.Width = 16;
						rC.Height -= 2;
						posAddons += 16;
						treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
							rC.Left, rC.Top,
							Resource.NewsItemRelatedImage.Attachment);
						treeNode.EnclosureRectangle = rC;
					}
					if(item.CommentCount>0)
					{
						Rectangle rC = r;
						rC.X = r.Right + posAddons;
						rC.Width = 16;
						posAddons += 16;
						rC.Height -= 2;
						Bitmap bmp = Properties.Resources.Comment_16;
						drawParams.Graphics.DrawImage(bmp,rC.Left+2,rC.Top+1,
						                              bmp.Width,bmp.Height);
						treeNode.CommentsRectangle = rC;
					}
					//Bottom Line
					drawParams.Graphics.DrawLine(new Pen(Color.FromArgb(234,233,225)), 
					                             rO.Left,rO.Bottom,
					                             rO.Right,rO.Bottom);
					//
					return true;
				}
				if(treeNode.Level==2)
				{
					//Comments
					if(treeNode.IsCommentUpdating)
					{
						if(treeNode.Cells.Count>0 && treeNode.Cells[0].Value is string)
						{
							r.Y += 2;
							r.Height -= 2;
							//Text
							r.X += UltraTreeExtended.COMMENT_HEIGHT + 3 + 20;
							drawParams.Graphics.DrawString((string)treeNode.Cells[0].Value, treeNode.Control.Font, Brushes.Black, r);
							//Bottom Line
							drawParams.Graphics.DrawLine(new Pen(Color.FromArgb(234,233,225)), 
								rO.Left,rO.Bottom,
								rO.Right,rO.Bottom);
							return true;
						}
						return false;
					}
					else
					{
						NewsItem ni = treeNode.NewsItem;
						r.X += 27;
						r.Width -= 27;
						r.Y += 3;
						r.Height -= 4;
						//Icon
						treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
							r.Left, r.Top-1,
							treeNode.NodeOwner.ImageIndex);
						//Text
						r.X += UltraTreeExtended.COMMENT_HEIGHT+2;
						r.Width -= UltraTreeExtended.COMMENT_HEIGHT+2;
						string byAuthor = "";
						if(!StringHelper.EmptyOrNull(ni.Author))
							byAuthor = "(by " + ni.Author + ") ";//TODO:I18N
						drawParams.Graphics.DrawString(byAuthor + ni.Title, treeNode.NodeOwner.Font, Brushes.Black, r);
						//Bottom Line
						drawParams.Graphics.DrawLine(new Pen(Color.FromArgb(234,233,225)), 
							rO.Left,rO.Bottom,
							rO.Right,rO.Bottom);
						//
						return true;
					}
				}
			}

			// To return FALSE from this method indicates that the element should draw itself as normal.
			// To return TRUE  from this method indicates that the element should not draw itself. 
			// Return true to prevent further drawing by the element
			return false;
		}

		/// <summary>
		/// Measures a string in hte graphics param.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <returns></returns>
		static public int MeasureDisplayStringWidth(Graphics graphics, string text, Font font) {
			StringFormat format = new StringFormat ();
			RectangleF rect = new RectangleF(0, 0, 10000, 1000);
			CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

			format.SetMeasurableCharacterRanges(ranges);
			Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
			rect = regions[0].GetBounds(graphics);

			return (int)(rect.Right + 1f);
		}
		static public int MeasureDisplayStringHeight(Graphics graphics, string text, Font font) {
			StringFormat format = new StringFormat ();
			RectangleF rect = new RectangleF(0, 0, 10000, 1000);
			CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

			format.SetMeasurableCharacterRanges(ranges);
			Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
			rect = regions[0].GetBounds(graphics);

			return (int)(rect.Bottom + 1f);
		}
	}
}
























