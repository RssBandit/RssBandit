#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: TreeFeedsDrawFilter.cs,v $
 * Revision 1.4  2007/02/09 14:54:09  t_rendelmann
 * fixed: added missing configuration option for newComments font style and color;
 * changed: some refactoring in FontColorHelper;
 *
 * Revision 1.3  2006/09/22 16:34:19  t_rendelmann
 * added CVS header and change history
 *
 */
#endregion

using System;
using System.Drawing;
using Infragistics.Win;
using Infragistics.Win.UltraWinTree;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Utility;
using ExpansionIndicatorUIElement = Infragistics.Win.UltraWinTree.ExpansionIndicatorUIElement;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Used to impl. custom UltraTreeNode drawing additions
	/// </summary>
	public class TreeFeedsDrawFilter : IUIElementDrawFilter
	{
		public TreeFeedsDrawFilter() {}

		#region IUIElementDrawFilter

		/// <summary>
		/// Called before each element is about to be drawn.
		/// </summary>
		/// <param name="drawParams">Exposes properties required for drawing an element (e.g. Element, Graphics, InvalidRect etc.)</param>
		/// <returns>
		/// Bit flags indicating which phases of the drawing operation to filter. The DrawElement method will be called only for those phases.
		/// </returns>
		DrawPhase IUIElementDrawFilter.GetPhasesToFilter( ref UIElementDrawParams drawParams )
		{
			UltraTreeNode treeNode = drawParams.Element.GetContext( typeof(UltraTreeNode), true ) as UltraTreeNode;
			if (treeNode != null) {
				
				//TODO: FIX, DOES NOT YET WORK AS EXPECTED:
//				if(drawParams.Element is ExpansionIndicatorUIElement )
//					return DrawPhase.AfterDrawElement; //to extend the clickable area
				
				if (treeNode.Level == 0) {
					// draw complete area (Group Header):
					if(drawParams.Element is EditorWithTextDisplayTextUIElement )
						return DrawPhase.BeforeDrawForeground; //Middle side
					//
					if(drawParams.Element is PreNodeAreaUIElement )
						return DrawPhase.BeforeDrawForeground; //Left text
					if(drawParams.Element is EditorWithTextUIElement )
						return DrawPhase.BeforeDrawForeground; //Middle side
					if(drawParams.Element is NodeSelectableAreaUIElement )
						//return DrawPhase.BeforeDrawForeground; //Right side
						return DrawPhase.BeforeDrawForeground | DrawPhase.AfterDrawElement; //Right side and unread items
					//
				} else if (treeNode.Level > 0) {
					// draw the part after the selectable UI area
					// (here: unread counter)
					if(drawParams.Element is NodeSelectableAreaUIElement )
						return DrawPhase.AfterDrawElement;
				}
			}
			return DrawPhase.None;
		}

		/// <summary>
		/// Called during the drawing operation of a UIElement for a specific phase
		/// of the operation. This will only be called for the phases returned
		/// from the GetPhasesToFilter method.
		/// </summary>
		/// <param name="drawPhase">Contains a single bit which identifies the current draw phase.</param>
		/// <param name="drawParams">Exposes properties required for drawing an element (e.g. Element, Graphics, InvalidRect etc.)</param>
		/// <returns>
		/// Returning true from this method indicates that this phase has been handled and the default processing should be skipped.
		/// </returns>
		bool IUIElementDrawFilter.DrawElement( DrawPhase drawPhase, ref UIElementDrawParams drawParams )
		{
			UltraTreeNode treeNode = drawParams.Element.GetContext( typeof(UltraTreeNode), true ) as UltraTreeNode;
			if ( treeNode != null )
			{
				//TODO: FIX, DOES NOT YET WORK AS EXPECTED:
//				if(drawParams.Element is ExpansionIndicatorUIElement) {
//					ExpansionIndicatorUIElement elem = drawParams.Element as ExpansionIndicatorUIElement;
//					if (elem != null && treeNode.HasExpansionIndicator) {
//						int ext = 4; //to extend the clickable area
//						elem.Rect = new Rectangle(elem.Rect.Left - ext, elem.Rect.Top - ext, elem.Rect.Bottom + ext, elem.Rect.Right + ext);
//						return false; 
//					}
//				}

				//if(treeNode.Level > 0)
				//{
					TreeFeedsNodeBase feedsNode = treeNode as TreeFeedsNodeBase;
					if (drawPhase == DrawPhase.AfterDrawElement && 
						feedsNode != null && feedsNode.Control != null) 
					{
						int unread = feedsNode.UnreadCount;
						if (unread > 0) 
						{
							// this image width is a workaround to extend the nods's clickable
							// area to include the unread count visual representation...
							int clickableAreaExtenderImageWidth = feedsNode.Control.RightImagesSize.Width;
							
							string st = String.Format("({0})", unread);
							UIElement uiElement = drawParams.Element; 
							Rectangle ur = uiElement.Rect;
							using (Brush unreadColorBrush = new SolidBrush(FontColorHelper.UnreadCounterColor)) {
								drawParams.Graphics.DrawString(st, FontColorHelper.UnreadCounterFont, 
									unreadColorBrush, ur.X + ur.Width - clickableAreaExtenderImageWidth, 
									ur.Y, StringFormat.GenericDefault);
							}
							return true;
						}
						return false;
					} 
//					return false;
//				}
				//
				if(treeNode.Level==0)
				{
					RectangleF initialRect = drawParams.Element.RectInsideBorders;
					Rectangle r = new Rectangle((int)initialRect.Left,(int)initialRect.Top,(int)initialRect.Width,(int)initialRect.Height);
					r.Width = treeNode.Control.DisplayRectangle.Width+300;
					r.Height = treeNode.ItemHeightResolved;

					if(drawParams.Element is EditorWithTextDisplayTextUIElement )
					{
						return false;
					}
					if(drawParams.Element is PreNodeAreaUIElement)
					{
						//Left Side
						//r.Height+=2;
						r.Width++;
					}
					if(drawParams.Element is EditorWithTextUIElement)
					{
						//Middle 
						r.Y--;
						r.X--;
						//r.Height++;
						r.Width++;
					}
					if(drawParams.Element is NodeSelectableAreaUIElement)
					{
						//Rigth side
						//r.Height+=2;
						r.Width++;
					}
					TreeFeedsNodeGroupHeaderPainter.PaintOutlook2003Header( drawParams.Graphics, r);
					return true;
				}
			}

			// To return FALSE from this method indicates that the element should draw itself as normal.
			// To return TRUE  from this method indicates that the element should not draw itself. 
			// Return true to prevent further drawing by the element
			return false;
		}

		#endregion
	}
}
