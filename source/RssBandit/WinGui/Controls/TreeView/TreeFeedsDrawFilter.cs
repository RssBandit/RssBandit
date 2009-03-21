#region Version Header

/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Drawing;
using Infragistics.Win;
using Infragistics.Win.UltraWinTree;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Used to impl. custom UltraTreeNode drawing additions
	/// </summary>
	public class TreeFeedsDrawFilter : IUIElementDrawFilter
	{
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
			if (treeNode != null) 
			{

				if (drawParams.Element is NodeSelectableAreaUIElement)
					return DrawPhase.AfterDrawElement;

				if (treeNode.Level == 0) {
					// draw background area (Group Headers):
					if (drawParams.Element is TreeNodeUIElement)
						return DrawPhase.BeforeDrawBorders; //Middle side
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
            UltraTreeNode treeNode = drawParams.Element.GetContext(typeof(UltraTreeNode), true) as UltraTreeNode;
            if (treeNode != null)
            {

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
                        Rectangle ur = drawParams.Element.RectInsideBorders;
                        using (Brush unreadColorBrush = new SolidBrush(FontColorHelper.UnreadCounterColor))
                        {
                            drawParams.Graphics.DrawString(st, FontColorHelper.UnreadCounterFont,
                                unreadColorBrush, ur.X + ur.Width - clickableAreaExtenderImageWidth,
                                ur.Y, StringFormat.GenericDefault);
                        }
                        return true;
                    }
                    return false;
                }

                if (treeNode.Level == 0)
                {
					RectangleF initialRect = drawParams.Element.RectInsideBorders;
                    RectangleF r = new RectangleF(initialRect.Left, initialRect.Top, 0, 0);
                    // this SHOULD be the horizontal scrolling area, but how do we get this (?):
					r.Width = treeNode.Control.DisplayRectangle.Width + 300;
                    r.Height = treeNode.ItemHeightResolved;

                    TreeFeedsNodeGroupHeaderPainter.PaintOutlook2003Header(drawParams.Graphics, r);
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
