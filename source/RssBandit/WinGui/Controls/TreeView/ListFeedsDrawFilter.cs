

using System.Drawing;
using Infragistics.Win;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using RssBandit.WinGui.Utility;
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls.TreeView
{
	public sealed class ListFeedsDrawFilter : IUIElementDrawFilter
	{
	    readonly Control owner;

	    public ListFeedsDrawFilter(Control owner)
	    {
	        this.owner = owner;
	    }
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
		    var scale = (float)owner.DeviceDpi / 96;
		    var sz1 = (int)(1 * scale);
		    var sz2 = (int)(2 * scale);
		    var sz3 = (int)(3 * scale);
		    var sz4 = (int)(4 * scale);
		    var sz5 = (int)(5 * scale);
		    var sz6 = (int)(6 * scale);
		    var sz7 = (int)(7 * scale);
		    var sz8 = (int)(8 * scale);
		    var sz9 = (int)(9 * scale);
		    var sz10 = (int)(10 * scale);
		    var sz14 = (int)(14 * scale);
		    var sz16 = (int)(16 * scale);
		    var sz20 = (int)(20 * scale);
		    var sz23 = (int)(23 * scale);
		    var sz26 = (int)(26 * scale);
		    var sz27 = (int)(27 * scale);
		    var sz90 = (int)(90 * scale);
		    var sz180 = (int)(180 * scale);

            int FLAG_WIDTH = sz23;

            UltraTreeNodeExtended treeNode = drawParams.Element.GetContext( typeof(UltraTreeNode), true ) as UltraTreeNodeExtended;
			if ( treeNode != null )
			{
				RectangleF initialRect = (RectangleF)drawParams.Element.RectInsideBorders;
				Rectangle r = new Rectangle((int)initialRect.Left,(int)initialRect.Top,(int)initialRect.Width,(int)initialRect.Height);
				r.X -= sz2;
				r.Width += sz2;
				r.Height = treeNode.ItemHeightResolved;
				Rectangle rO = r;
				//
				if(treeNode.Level==0)
				{
					Image img = null;
					if(treeNode.Expanded)
						img = Properties.Resources.Minus_16.GetImageStretchedDpi(scale);
					else 
						img = Properties.Resources.Plus_16.GetImageStretchedDpi(scale);

                    if (!treeNode.Selected) {
						drawParams.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220,224,227)),r);
					}

					//Image
					drawParams.Graphics.DrawImage(img, r.Left+ sz4, r.Top+ sz5);
					treeNode.CollapseRectangle = new Rectangle(r.Left+ sz4, r.Top+ sz5, img.Width, img.Height); 
					//String
					Color c = Color.FromArgb(112, 111, 145);
					drawParams.Graphics.DrawString((string)treeNode.Cells[0].Value /*+ treeNode.DateTime.ToString()*/, new Font(drawParams.Font,FontStyle.Bold), 
					                               new SolidBrush(c), r.Left+ sz20, r.Top+sz4);
					//Bottom Lines
					Pen pen = new Pen(Color.FromArgb(165, 164, 189));
					drawParams.Graphics.DrawLine(pen, r.Left,r.Bottom-sz1,r.Right,r.Bottom-sz1);
					drawParams.Graphics.DrawLine(pen, r.Left,r.Bottom,r.Right,r.Bottom);
					//
					return true;
				}
				if(treeNode.Level==1)
				{
					INewsItem item = treeNode.NewsItem;
					Brush bGray = new SolidBrush(Color.FromArgb(128, 128, 128));
					if(treeNode.Selected){
						bGray = Brushes.White;						
						drawParams.Graphics.FillRectangle(new SolidBrush(Color.DarkBlue),r);										
					}

					//Icon
					treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
					                                                r.Left + sz7, r.Top + sz3,
					                                                treeNode.NodeOwner.ImageIndex);
					//Flag
					Rectangle rF = r;
					rF.X = rF.Right - FLAG_WIDTH;
					rF.Width = FLAG_WIDTH;
					int flagInd = GetFlagImageIndex(item.FlagStatus);
					treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
						rF.Left + sz2 + (rF.Width-treeNode.NodeOwner.ListView.SmallImageList.ImageSize.Width)/2, 
						rF.Top + sz2 + (rF.Height-treeNode.NodeOwner.ListView.SmallImageList.ImageSize.Height)/2,
						flagInd);
					treeNode.FlagRectangle = rF;
					//Date
					r.Width -= FLAG_WIDTH;
					r.Height = MeasureDisplayStringHeight(drawParams.Graphics, "W", treeNode.NodeOwner.Font);
					r.Y += sz3;
					Rectangle r2 = r;
					r.X += sz180;
					r.Width -= sz180;
					string dtS = item.Date.ToLocalTime().ToShortDateString()+" "+item.Date.ToLocalTime().ToShortTimeString();

					if(((UltraTreeNodeExtended)treeNode.Parent).IsGroupOneDay)
						dtS = item.Date.ToLocalTime().ToString("HH:mm");

					int ww = MeasureDisplayStringWidth(drawParams.Graphics, dtS, treeNode.NodeOwner.Font);
					r.X = r.Right - ww;
					r.Width = ww+sz10;
					drawParams.Graphics.DrawString(dtS, treeNode.NodeOwner.Font, bGray, r);
					//Author
					r2.X += sz26;
					r2.Width = r.X-r2.X;
					r = r2;
					string author = (string.IsNullOrEmpty(item.Author) ? item.Feed.title: item.Author);
					drawParams.Graphics.DrawString(author, treeNode.NodeOwner.Font, bGray, r);

					//Title
					r = rO;
					r.X += sz26;
					r.Width -= sz26+FLAG_WIDTH;
					if(item.CommentCount>0)
						r.Width -= sz16;
					if(item.Enclosures!=null && item.Enclosures.Count>0)
						r.Width -= sz16;
					r.Y += rO.Height / 2+sz2;
					r.Height = MeasureDisplayStringHeight(drawParams.Graphics, item.Title, treeNode.NodeOwner.Font);
					Brush titleBrush = (treeNode.Selected ? bGray : new SolidBrush(treeNode.NodeOwner.ForeColor));
					drawParams.Graphics.DrawString(item.Title, treeNode.NodeOwner.Font, titleBrush , r);
					//Comments
					int posAddons = 0;
					if(item.Enclosures!=null && item.Enclosures.Count>0)
					{
						Rectangle rC = r;
						rC.X = r.Right;
						rC.Width = sz16;
						rC.Height -= sz2;
						posAddons += sz16;
						treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
							rC.Left, rC.Top,
							Resource.NewsItemRelatedImage.Attachment);
						treeNode.EnclosureRectangle = rC;
					}
					if(item.CommentCount>0)
					{
						Rectangle rC = r;
						rC.X = r.Right + posAddons;
						rC.Width = sz16;
						posAddons += sz16;
						rC.Height -= sz2;
						var bmp = Properties.Resources.Comment_16.GetImageStretchedDpi(scale);
						drawParams.Graphics.DrawImage(bmp,rC.Left+ sz2, rC.Top+sz1,
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
							r.Y += sz2;
							r.Height -= sz2;
                            //Text
                            r.X += UltraTreeExtended.COMMENT_HEIGHT + sz3 + sz20;
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
						INewsItem ni = treeNode.NewsItem;
						r.X += sz27;
						r.Width -= sz27;
						r.Y += sz3;
						r.Height -= sz4;
						//Icon
						treeNode.NodeOwner.ListView.SmallImageList.Draw(drawParams.Graphics,
							r.Left, r.Top-sz1,
							treeNode.NodeOwner.ImageIndex);
                        //Text
                        r.X += UltraTreeExtended.COMMENT_HEIGHT + sz2;
                        r.Width -= UltraTreeExtended.COMMENT_HEIGHT + sz2;
                        string byAuthor = "";
						if(!string.IsNullOrEmpty(ni.Author))
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
























