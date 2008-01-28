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

using NewsComponents.Net;
using NewsComponents.Search;

namespace NewsComponents.Feed {
    /// <summary>
    /// A NewsHandler that retrieves user subscriptions and feeds from Google Reader. 
    /// </summary>
    class GoogleReaderNewsHandler : NewsHandler{

        #region public methods

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this NewsHandler</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {

        }

        #endregion 

    }
}
