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
using System.ComponentModel;
using System.Xml;
using RssBandit.AppServices.Core;

namespace NewsComponents.Feed
{
    public interface INewsFeed : INotifyPropertyChanged, ISharedProperty
    {
        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        string title { get; set; }

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        string link { get; set; }

        string id { get; set; }
        bool lastretrievedSpecified { get; set; }
        DateTime lastretrieved { get; set; }

		string etag { get; set; }
        string cacheurl { get; set; }

		///// <summary />
		///// <remarks>Notifies on change. </remarks>
		//string maxitemage { get; set; }

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        List<string> storiesrecentlyviewed { get; set; }

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        List<string> deletedstories { get; set; }

        DateTime lastmodified { get; set; }
        bool lastmodifiedSpecified { get; set; }

		/// <summary>
		/// Gets or sets the certificate id.
		/// </summary>
		/// <remarks>Client certificate identifier (usually the cert's thumb print value)</remarks>
		/// <value>The certificate id.</value>
		string certificateId { get; set; }
		/// <summary>
		/// Gets or sets the auth user.
		/// </summary>
		/// <value>The auth user.</value>
		string authUser { get; set; }
        Byte[] authPassword { get; set; }
		//string listviewlayout { get; set; }

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        string favicon { get; set; }

		///// <summary />
		///// <remarks>Notifies on change. </remarks>
		//bool downloadenclosures { get; set; }

		//bool downloadenclosuresSpecified { get; set; }

		///// <summary />
		///// <remarks>Notifies on change. </remarks>
		//string enclosurefolder { get; set; }

		///// <summary />
		///// <remarks>Notifies on change. </remarks>
		//string stylesheet { get; set; }

        int causedExceptionCount { get; set; }
        bool causedException { get; set; }
        bool replaceitemsonrefresh { get; set; }
        bool replaceitemsonrefreshSpecified { get; set; }
        string newsaccount { get; set; }
		//bool markitemsreadonexit { get; set; }
		//bool markitemsreadonexitSpecified { get; set; }
        XmlElement[] Any { get; set; }
        XmlAttribute[] AnyAttr { get; set; }
        bool alertEnabled { get; set; }
        bool alertEnabledSpecified { get; set; }
		//bool enclosurealert { get; set; }
		//bool enclosurealertSpecified { get; set; }
        object Tag { get; set; }
        object owner { get; set; } /* NewsHandler */ 

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        bool containsNewComments { get; set; }

        /// <summary />
        /// <remarks>Notifies on change. </remarks>
        bool containsNewMessages { get; set; }
        
        string category { get; set; }
        List<string> categories { get; set; }

        /// <summary>
		/// Adds an entry to the storiesrecentlyviewed collection
        /// </summary>
        /// <seealso cref="storiesrecentlyviewed"/>
        /// <param name="storyid">The ID to add</param>
        void AddViewedStory(string storyid);

        /// <summary>
        /// Removes an entry from the storiesrecentlyviewed collection
        /// </summary>
        /// <seealso cref="storiesrecentlyviewed"/>
        /// <param name="storyid">The ID to remove</param>
        void RemoveViewedStory(string storyid);

        /// <summary>
        /// Adds an entry to the deletedstories collection
        /// </summary>
        /// <seealso cref="deletedstories"/>
        /// <param name="storyid">The ID to add</param>
        void AddDeletedStory(string storyid);

        /// <summary>
        /// Remove an entry from the deletedstories collection
        /// </summary>
        /// <seealso cref="deletedstories"/>
        /// <param name="storyid">The ID to remove</param>
        void RemoveDeletedStory(string storyid);

        /// <summary>
        /// Adds a category to the categories collection
        /// </summary>
        /// <seealso cref="categories"/>
        /// <param name="name">The category to add</param>
        void AddCategory(string name);

        /// <summary>
        /// Removes a category from the categories collection
        /// </summary>
        /// <seealso cref="categories"/>
        /// <param name="name">The category to remove</param>
        void RemoveCategory(string name); 
    }
}
