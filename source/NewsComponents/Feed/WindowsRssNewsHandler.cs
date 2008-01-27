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
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using Microsoft.Feeds.Interop;


using NewsComponents.Net;
using NewsComponents.Search;

namespace NewsComponents.Feed {
    /// <summary>
    /// A NewsHandler that retrieves user subscriptions and feeds from the Windows RSS platform. 
    /// </summary>
    class WindowsRssNewsHandler : NewsHandler
    {


        #region constructor

         /// <summary>
        /// Initializes a new instance of the <see cref="NewsHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public WindowsRssNewsHandler(INewsComponentsConfiguration configuration)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = NewsHandler.DefaultConfiguration;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);
           
        }    

        #endregion 


        #region private methods

        #endregion 

        #region public methods

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
        }

        /* 
        /// <summary>
        /// Loads the RSS feedlist from the given URL and validates it against the schema. 
        /// </summary>
        /// <param name="feedListUrl">The URL of the feedlist</param>
        /// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
        /// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
        public void LoadFeedlist(string feedListUrl, ValidationEventHandler veh)
        {
            // LoadFeedlist(AsyncWebRequest.GetSyncResponseStream(feedListUrl, null, this.UserAgent, this.Proxy), veh); 

            IFeeds fs = new FeedsClass();
            IFeedFolder root = fs.Subscriptions;

            if (root.Feeds.Count > 0)
            {
                foreach (IFeed f in root.Feeds)
                {
                    this.AddFeed(f, null, null);
                }
            }

            if (root.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in root.Subfolders)
                {
                    this.AddFolder(f, null, null, null);
                }
            }

        }

        /// <summary>
        /// Grabs the common feedlist and populates the input collections
        /// </summary>
        /// <param name="f"></param>
        /// <param name="c"></param>
        public void GetCurrentCommonFeedList(FeedsCollection feedList, CategoriesCollection catList)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder root = fs.Subscriptions;

            if (root.Feeds.Count > 0)
            {
                foreach (IFeed f in root.Feeds)
                {
                    this.AddFeed(f, null, feedList);
                }
            }

            if (root.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in root.Subfolders)
                {
                    this.AddFolder(f, null, feedList, catList);
                }
            }


        }

        /// <summary>
        /// Adds an IFeed instance to the subscription list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void AddFeed(IFeed feed, string category, FeedsCollection feedList)
        {

            feedList = (feedList == null ? _feedsTable : feedList);

            feedsFeed f = new feedsFeed();
            f.link = feed.Url;
            f.title = feed.Name;
            f.category = category;

            if (feedList.Contains(f.link) == false)
            {
                feedList.Add(f.link, f);
            }
        }

        /// <summary>
        /// Adds an IFeed instance to the common feed list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void AddFeed2CommonFeedList(feedsFeed feed, string category)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder folder = fs.Subscriptions;

            if (category != null)
            {
                string[] categoryPath = category.Split(new char[] { '\\' });

                foreach (string c in categoryPath)
                {
                    folder = folder.GetSubfolder(c);
                }
            }

            folder.CreateFeed(feed.title, feed.link);
        }

        /// Adds an IFeed instance to the common feed list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void DeleteFeedFromCommonFeedList(feedsFeed feed, string category)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder folder = fs.Subscriptions;

            if (category != null)
            {
                string[] categoryPath = category.Split(new char[] { '\\' });

                foreach (string c in categoryPath)
                {
                    folder = folder.GetSubfolder(c);
                }
            }

            folder.GetFeed(feed.title).Delete();
        }

        /// <summary>
        /// Add a folder to the feedlist 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="category">The path to the folder</param>
        public void AddFolder(IFeedFolder folder, string path, FeedsCollection feedList, CategoriesCollection catList)
        {

            feedList = (feedList == null ? _feedsTable : feedList);
            catList = (catList == null ? this.categories : catList);

            string category = (path == null ? folder.Name : path + "\\" + folder.Name);

            if (folder.Feeds.Count > 0)
            {

                foreach (IFeed f in folder.Feeds)
                {
                    this.AddFeed(f, category, feedList);
                }

                if (!catList.ContainsKey(category))
                {
                    catList.Add(category);
                }
            }

            if (folder.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in folder.Subfolders)
                {
                    this.AddFolder(f, category, feedList, catList);
                }
            }
        }

*/ 
        #endregion

    }
}
