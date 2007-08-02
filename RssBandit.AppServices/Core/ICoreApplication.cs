#region CVS Version Header
/*
 * $Id: ICoreApplication.cs,v 1.2 2005/06/05 17:13:10 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/05 17:13:10 $
 * $Revision: 1.2 $
 */
#endregion

namespace RssBandit.AppServices
{
	/// <summary>
	/// ICoreApplication contains the core service functions 
	/// of RSS Bandit.
	/// </summary>
	public interface ICoreApplication
	{
		#region General
		/// <summary>
		/// Returns the current global (specified via options)
		/// Feed Refresh Rate in minutes.
		/// </summary>
		int CurrentGlobalRefreshRate { get; } 
		#endregion

		#region Category management
		/// <summary>
		/// Gets the default category.
		/// </summary>
		string DefaultCategory { get; }
		/// <summary>
		/// Gets the list of categories including the default category.
		/// </summary>
		/// <returns></returns>
		string[] GetCategories();
		/// <summary>
		/// Use this method to add a new category.
		/// </summary>
		/// <param name="category"></param>
		void AddCategory(string category);
		#endregion

		#region Feeds management
		/// <summary>
		/// Call to subscribe to a new feed. This will initiate to
		/// display the Add Subscription Wizard with the parameters
		/// pre-set you provide.
		/// </summary>
		/// <param name="url">New feed Url</param>
		/// <returns></returns>
		bool SubscribeToFeed(string url);
		bool SubscribeToFeed(string url, string category);
		bool SubscribeToFeed(string url, string category, string title);
		#endregion

	}
}
