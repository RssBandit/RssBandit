#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Forms;
using System.Drawing;

using Infragistics.Win;
using Infragistics.Win.UltraWinToolTip;


namespace RssBandit.WinGui.Controls
{
	
	/// <summary>
	/// This abstract class hooks into the MouseMove and MouseLeave
	/// events of an Control and determines when tooltip information
	/// needs to be re-evaluated. For example, when the mouse leaves an element
	/// for which a tooltip was being displayed, or when the mouse enters
	/// a new element that might need a tooltip displayed. 
	/// 
	/// When an evaluation needs to be make, the PrepareToolTip method is called. 
	/// 
	/// To use this class, derive a new class from it. Then override the 
	/// PrepareToolTip method. In that method, examine the element and determine
	/// if it should display a tooltip. If so, set the toolTipInfo properties
	/// and return true. If not, simply return false. 
	/// 
	/// For non-Infragistics sub elements support implement/override the method
	/// </summary>
	/// <example>
	/// // Sample for a Infragstics Control (Grid) see
	/// C:\Documents and Settings\All Users\Documents\Infragistics\NetAdvantage for .NET 2008 Vol. 1 CLR 2.0\Samples\Win\WinMisc\CS\ToolTipManager\ToolTips with Context CS
	/// </example>
	public abstract class UltraToolTipContextHelperBase : IDisposable 
	{
		#region Private Members

		// An Control that may need to display tooltips based on context.
		private Control control = null;

		// An UtraToolTipManager component that will display the tooltips. 
		private UltraToolTipManager toolTipManager = null;

		// Keeps track of the last element that was evaluated. 
		private object lastToolTipElement = null;

		#endregion Private Members

		#region Constructor

		public UltraToolTipContextHelperBase(Control control, UltraToolTipManager toolTipManager) {
			if (control == null)
				throw new ArgumentNullException("control");

			if (toolTipManager == null)
				throw new ArgumentNullException("toolTipManager");

			this.control = control;
			this.toolTipManager = toolTipManager;

			// Hook into the neccessary events of the control to trap when the mouse
			// mouse in and out of elements. 
			this.HookControlEvents();		
		}

		#endregion Constructor

		#region Abstract Methods
	
		#region PrepareToolTip

		/// <summary>
		/// The PrepareToolTip method fires each time an element is evaluated for tooltip display.
		/// </summary>
		/// <param name="element">The object currently needing evaluation.</param>
		/// <param name="toolTipInfo">The UltraToolTipInfo for the control</param>
		/// <returns>Returns true if the element in question should display a tooltip, false if it should not.</returns>
		/// <remarks>
		/// This method must be overriden in a derived class and each element evaluated. 
		/// If an element is evaluated by this method and the method returns false indicating 
		/// that the element does not need a tooltip, the method will be called with the
		/// parent element, all the way up the element chain. In this way, elements that 
		/// are contained by other elements can show more specific tooltips. 
		/// To properly use this method, derive a class from UltraToolTipContextHelperBase
		/// and override the PrepareToolTip method. Inisde the method, examine the
		/// UIElement and determine if a tooltip is needed. If so, set the appropriate 
		/// properties of toolTipInfo such as ToolTipText, ToolTipTitle, ToolTipImage, Appearance,
		/// etc. and return true from the method. If the element does not need a tooltip, return
		/// false. 
		/// </remarks>
		protected abstract bool PrepareToolTip(object element, UltraToolTipInfo toolTipInfo);	

		#endregion PrepareToolTip

		#endregion Abstract Methods

		#region Virtual Methods

		#region ElementFromPoint(Point)

		/// <summary>
		/// Should be overridden to get the element at the mouse position.
		/// The default impl. just returns null.
		/// </summary>
		/// <param name="p">The point.</param>
		/// <returns></returns>
		protected virtual object ElementFromPoint(Point p) {
			return null;
		}

		#endregion

		#region HookControlEvents

		/// <summary>
		/// Hooks into the neccessary events in order to show tooltips. 
		/// </summary>
		protected virtual void HookControlEvents() {
			this.control.MouseMove += new MouseEventHandler(this.control_MouseMove);
			this.control.MouseLeave += new EventHandler(this.control_MouseLeave);
		}
		#endregion HookControlEvents

		#region UnHookControlEvents
		/// <summary>
		/// Unhooks from events that were hooked in HookControlEvents
		/// </summary>
		protected virtual void UnHookControlEvents() {
			this.control.MouseMove -= new MouseEventHandler(this.control_MouseMove);
			this.control.MouseLeave -= new EventHandler(this.control_MouseLeave);
		}
		#endregion UnHookControlEvents

		#endregion Virtual Methods

		#region Protected Properties

		#region Control
		protected Control Control {
			get { return this.control; }
		}
		#endregion Control

		#endregion Protected Properties

		#region Private Properties

		#region ToolTipInfo

		/// <summary>
		/// Gets the UltraToolTipInfo for the Control
		/// </summary>
		private UltraToolTipInfo ToolTipInfo {
			get {
				return this.toolTipManager.GetUltraToolTip(this.control);
			}
		}
		#endregion ToolTipInfo

		#endregion Private Properties

		#region Event Handlers

		#region control_MouseMove
		/// <summary>
		/// Handles the MouseMove event of the Control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void control_MouseMove(object sender, MouseEventArgs e) {	
			
			bool showToolTipForElement = false;
			object element = null;

			// Get the toolTipInfo
			UltraToolTipInfo toolTipInfo = this.ToolTipInfo;

			// Get the main UIElement of the control. All UltraControlBase
			// controls implement IUltraControlElement, so they will all 
			// have a main element. 	
			IUltraControlElement iMainElement = this.control as IUltraControlElement;
			
			if (iMainElement != null) {	// Infragistics Controls...
				
				UIElement mainElement = iMainElement.MainUIElement;

				// Get the element under the mouse
				UIElement uiElement = mainElement.ElementFromPoint(new Point(e.X, e.Y));

				// Start with the element under the mouse and walk up it's parent chain
				// until we find an element that should display a tooltip, or
				// come to the end of the chain (where element == null).
				while (uiElement != null &&			
					! showToolTipForElement) {
					// If this is the same element we evaluated last time through, 
					// just return. This is both for efficiency and so the tooltip
					// does not continuously hide and re-display. 
					if (uiElement == this.lastToolTipElement )
						return;

					// Call PrepareToolTip. This will allow the derived class to
					// determine if the element should show a tooltip and to 
					// set the properties on the toolTipInfo. 
					showToolTipForElement = this.PrepareToolTip(uiElement, toolTipInfo);

					// If PrepareToolTip returned true, there's no reason to continue looping.
					// If not, get the parent element and try again. 
					if (!showToolTipForElement)
						uiElement = uiElement.Parent;
				}

				element = uiElement;

			} else {		// other (Std.) controls...

				// Get the element under the mouse
				element = this.ElementFromPoint(new Point(e.X, e.Y));

				// Start with the element under the mouse and walk up it's parent chain
				// until we find an element that should display a tooltip, or
				// come to the end of the chain (where element == null).
				while (element != null &&			
					! showToolTipForElement) {
					// If this is the same element we evaluated last time through, 
					// just return. This is both for efficiency and so the tooltip
					// does not continuously hide and re-display. 
					if (element == this.lastToolTipElement )
						return;

					// Call PrepareToolTip. This will allow the derived class to
					// determine if the element should show a tooltip and to 
					// set the properties on the toolTipInfo. 
					showToolTipForElement = this.PrepareToolTip(element, toolTipInfo);

					// If PrepareToolTip returned true, there's no reason to continue looping.
					if (!showToolTipForElement)
						element = null;
				}
			}

			// If showToolTipForElement is true, enabled the tooltip
			if ( showToolTipForElement ) {
				// If we are showing a tooltip for a different element than we did
				// last, Hide the old tooltip. 
				if (element != this.lastToolTipElement )
					toolTipManager.HideToolTip();

				// Enabled to ToolTipInfo. The tooltip will not actually display
				// until the InitialDelay of the UtraToolTipManager expires. 
				toolTipInfo.Enabled = DefaultableBoolean.True;			
			}
			else {			
				//showToolTipForElement is false. So no tooltip should be shown.
				// Hide any tooltips that were displayed. 
				toolTipManager.HideToolTip();

				// Disable the ToolTipInfo
				toolTipInfo.Enabled = DefaultableBoolean.False;
			}

			// Store the last element we just processed. 
			this.lastToolTipElement = element;
		}
		#endregion control_MouseMove

		#region control_MouseLeave
		private void control_MouseLeave(object sender, EventArgs e) {
			// If the mouse leaves the control, we won't get a MouseMove, so
			// just clear any tooltips.

			// Get the ToolTipInfo
			UltraToolTipInfo toolTipInfo = this.ToolTipInfo;
		
			// Hide any currently displayed tooltips
			toolTipManager.HideToolTip();

			// Disable the ToolTipInfo for this control. 
			toolTipInfo.Enabled = DefaultableBoolean.False;

			// Clear the cached element so that if the mouse re-enters the control
			// on the same element from which it exited, it will still show a 
			// tooltip. 
			this.lastToolTipElement = null;
		}
		#endregion control_MouseLeave
	
		#endregion Event Handlers

		#region Implementation of IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UnHookControlEvents();    
            }            
        }

		public void Dispose() {
			// We need to make sure we unhook from events if the class is disposed. 
			Dispose(true);
            GC.SuppressFinalize(this);
		}
		#endregion		
	}

}
