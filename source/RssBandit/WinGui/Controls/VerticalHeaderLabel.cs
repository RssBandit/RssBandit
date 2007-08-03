#region CVS Version Header
/*
 * $Id: VerticalHeaderLabel.cs,v 1.1 2006/08/03 19:16:48 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/08/03 19:16:48 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// Summary description for VerticalHeaderLabel.
	/// </summary>
	public class VerticalHeaderLabel : UltraLabel
	{

		/// <summary>
		/// Raised, if an image is assigned and the mouse click happens over them.
		/// </summary>
		public event EventHandler ImageClick;

		private Cursor _saveCursorImageArea;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public VerticalHeaderLabel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this._saveCursorImageArea = null;
			this.DrawFilter = new SmoothLabelDrawFilter(this, true);
			this.MouseEnterElement += new Infragistics.Win.UIElementEventHandler(VerticalHeaderLabel_MouseEnterElement);
			this.MouseLeaveElement += new Infragistics.Win.UIElementEventHandler(VerticalHeaderLabel_MouseLeaveElement);
			this.Click += new System.EventHandler(VerticalHeaderLabel_Click);
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// VerticalHeaderLabel
			// 
			this.Name = "VerticalHeaderLabel";
			this.Size = new System.Drawing.Size(25, 245);

		}
		#endregion

		private void VerticalHeaderLabel_MouseEnterElement(object sender, Infragistics.Win.UIElementEventArgs e) {
			//Debug.WriteLine("Enter Element " + e.Element.GetType().Name);
			if (this.Appearance.Image != null && 
				e.Element is ImageAndTextUIElement.ImageAndTextDependentImageUIElement) {
				this._saveCursorImageArea = this.Cursor;
				this.Cursor = Cursors.Hand;
			}
		}

		private void VerticalHeaderLabel_MouseLeaveElement(object sender, Infragistics.Win.UIElementEventArgs e) {
			//Debug.WriteLine("Leave Element " + e.Element.GetType().Name);
			if (this.Appearance.Image != null && 
				e.Element is ImageAndTextUIElement.ImageAndTextDependentImageUIElement) {
				if (this._saveCursorImageArea != null)
					this.Cursor = this._saveCursorImageArea;
				else
					this.Cursor = Cursors.Default;
				this._saveCursorImageArea = null;
			}
		}

		private void VerticalHeaderLabel_Click(object sender, System.EventArgs e) {
			if (this._saveCursorImageArea != null)
				OnImageClick();
		}

		/// <summary>
		/// Called when the image was clicked and raises the ImageClick event (if set).
		/// </summary>
		protected void OnImageClick() {
			if (ImageClick != null)
				ImageClick(this, EventArgs.Empty);
		}
	}
}
