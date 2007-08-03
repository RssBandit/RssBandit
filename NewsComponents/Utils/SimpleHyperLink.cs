#region CVS Version Header
/*
 * $Id: SimpleHyperLink.cs,v 1.1 2005/12/08 14:45:58 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/12/08 14:45:58 $
 * $Revision: 1.1 $
 */
#endregion

namespace NewsComponents.Utils
{
	/// <summary>
	/// SimpleHyperLink represents a simple HyperLink
	/// without the overhead of the yet existing class 
	/// System.Web.UI.WebControls.HyperLink.
	/// </summary>
	public class SimpleHyperLink
	{
		private string _navigateUrl;
		private string _text;
		private string _imageUrl;

		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleHyperLink"/> class.
		/// </summary>
		/// <param name="navigateUrl">The navigate URL.</param>
		public SimpleHyperLink(string navigateUrl):
			this(navigateUrl, null, null) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleHyperLink"/> class.
		/// </summary>
		/// <param name="navigateUrl">The navigate URL.</param>
		/// <param name="text">The text.</param>
		public SimpleHyperLink(string navigateUrl, string text):
			this(navigateUrl, text, null) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleHyperLink"/> class.
		/// </summary>
		/// <param name="navigateUrl">The navigate URL.</param>
		/// <param name="text">The text.</param>
		/// <param name="imageUrl">The image URL.</param>
		public SimpleHyperLink(string navigateUrl, string text, string imageUrl)
		{
			_navigateUrl = navigateUrl;
			_text = text;
			_imageUrl = imageUrl;
		}

		#endregion

		#region public properties

		/// <summary>
		/// Gets or sets the image URL.
		/// </summary>
		/// <value>The image URL.</value>
		public string ImageUrl {
			get { return _imageUrl; }
			set { _imageUrl = value; }
		}

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get { return _text; }
			set { _text = value; }
		}

		/// <summary>
		/// Gets or sets the navigate URL.
		/// </summary>
		/// <value>The navigate URL.</value>
		public string NavigateUrl {
			get { return _navigateUrl; }
			set { _navigateUrl = value; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/> by
		/// returning the NavigateUrl;
		/// </returns>
		public override string ToString() {
			return NavigateUrl;
		}

		#endregion
	}
}
