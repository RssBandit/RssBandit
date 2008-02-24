#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


namespace RssBandit.AppServices.Core
{

	/// <summary>
	/// The set of properties that are common/shared over NewsHandler 
	/// (with the global settings), the categories a feed is member of
	/// and the feeds itself.
	/// </summary>
	public interface ISharedProperty
	{
		/// <summary>
		/// Gets or sets the maximum item age.
		/// </summary>
		/// <value>The max. item age.</value>
		string maxitemage { get; set; }

		/// <summary>
		/// Gets or sets the refresh rate.
		/// </summary>
		/// <value>The refreshrate.</value>
		int refreshrate { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [refresh rate is specified].
		/// </summary>
		/// <value><c>true</c> if [refresh rate specified]; otherwise, <c>false</c>.</value>
		bool refreshrateSpecified { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should download enclosures.
		/// </summary>
		/// <value><c>true</c> if download enclosures; otherwise, <c>false</c>.</value>
		bool downloadenclosures { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [download enclosures is specified].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [download enclosures specified]; otherwise, <c>false</c>.
		/// </value>
		bool downloadenclosuresSpecified { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should alert on enclosure downloads.
		/// </summary>
		/// <value><c>true</c> if enclosurealert; otherwise, <c>false</c>.</value>
		bool enclosurealert { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether [enclosure alert specified].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [enclosurealert specified]; otherwise, <c>false</c>.
		/// </value>
		bool enclosurealertSpecified { get; set; }

		/// <summary>
		/// Gets or sets the enclosure folder.
		/// </summary>
		/// <value>The enclosure folder.</value>
		string enclosurefolder { get; set; }

		/// <summary>
		/// Gets or sets the listview layout.
		/// </summary>
		/// <value>The listview layout.</value>
		string listviewlayout { get; set; }

		/// <summary>
		/// Gets or sets the stylesheet to render the feed/items.
		/// </summary>
		/// <value>The stylesheet.</value>
		string stylesheet { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should mark items read on exit.
		/// </summary>
		/// <value><c>true</c> if markitemsreadonexit; otherwise, <c>false</c>.</value>
		bool markitemsreadonexit { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether [mark items read on exit specified].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [mark items read on exit specified]; otherwise, <c>false</c>.
		/// </value>
		bool markitemsreadonexitSpecified { get; set; }
	}
}
