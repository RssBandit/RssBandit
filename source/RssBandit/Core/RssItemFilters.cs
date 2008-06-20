#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using RssBandit.WinGui.Controls.ThListView;
using NewsComponents;
using RssBandit.AppServices;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.Filter
{
    /// <summary>
    /// Summary description for NewsItemFilterManager.
    /// </summary>
    internal class NewsItemFilterManager
    {
        /// <summary>
        /// Can be used to refresh Gui state. Called, if any filter criteria match
        /// </summary>
        public event FilterActionCancelEventHandler FilterMatch;

        public delegate void FilterActionCancelEventHandler(object sender, FilterActionCancelEventArgs e);

        private readonly Dictionary<string, INewsItemFilter> filters = new Dictionary<string, INewsItemFilter>();


        /// <summary>
        /// Add a new filter to the internal collection. If the filter exists,
        /// it will be replaced by the new one.
        /// </summary>
        /// <param name="key">Filter Identifier</param>
        /// <param name="newFilter">A INewsItemFilter instance</param>
        /// <returns>The INewsItemFilter instance</returns>
        public INewsItemFilter Add(string key, INewsItemFilter newFilter)
        {
            if (key == null || newFilter == null)
                throw new ArgumentException("Parameter cannot be null", (newFilter == null ? "key" : "newFilter"));

            if (filters.ContainsKey(key))
                filters.Remove(key);

            filters.Add(key, newFilter);
            return newFilter;
        }

        /// <summary>
        /// Indexer: Sets/Get a INewsItemFilter
        /// </summary>
        public INewsItemFilter this[string key]
        {
            get
            {
                return filters[key];
            }
            set
            {
                filters[key] = value;
            }
        }

        /// <summary>
        /// Removes a filter from the internal collection.
        /// </summary>
        /// <param name="key">Filter identifier</param>
        public void Remove(string key)
        {
            if (filters.ContainsKey(key))
                filters.Remove(key);
        }

        /// <summary>
        /// Apply all filters to the specified NewsItem.
        /// </summary>
        public bool Apply( /*NewsItem item */ ThreadedListViewItem lvItem)
        {
            NewsItem item = lvItem.Key as NewsItem;
            if (item == null)
                return false;

            bool anyApplied = false;

            foreach (string key in filters.Keys)
            {
                INewsItemFilter filter = filters[key];
                if (filter != null && filter.Match(item))
                {
                    if (!CancelFilterAction(key))
                    {
                        filter.ApplyAction(item, lvItem);
                        anyApplied = true;
                    }
                }
            }
            return anyApplied;
        }

        /// <summary>
        /// Apply a specific filter to the specified NewsItem.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item">The NewsItem instance</param>
        public bool Apply(string key, NewsItem item)
        {
            bool anyApplied = false;
            if (!filters.ContainsKey(key))
                return anyApplied;
            INewsItemFilter filter = filters[key];
            if (filter.Match(item))
            {
                if (!CancelFilterAction(key))
                {
                    filter.ApplyAction(item, null);
                    anyApplied = true;
                }
            }
            return anyApplied;
        }

        protected bool CancelFilterAction(string key)
        {
            if (FilterMatch != null)
            {
                FilterActionCancelEventArgs ceh = new FilterActionCancelEventArgs(key, false);
                FilterMatch(this, ceh);
                return ceh.Cancel;
            }
            return false;
        }

        public class FilterActionCancelEventArgs : CancelEventArgs
        {
            private readonly string filterKey = null;

            public FilterActionCancelEventArgs()
            {
                ;
            }

            public FilterActionCancelEventArgs(string key, bool cancelState) : base(cancelState)
            {
                filterKey = key;
            }

            public string FilterKey
            {
                get
                {
                    return filterKey;
                }
            }
        }
    }

    internal class NewsItemReferrerFilter : INewsItemFilter
    {
        private string _referrer;

        public NewsItemReferrerFilter(RssBanditApplication app)
        {
            _referrer = null;
            app.PreferencesChanged += OnPreferencesChanged;
            app.FeedSourceSubscriptionsLoaded += OnFeedlistLoaded;
        }

        /// <summary>
        /// Used to init. Identities are stored in the feedlist, so we need
        /// that to init.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFeedlistLoaded(object sender, FeedSourceEventArgs e)
        {
			if (e.Entry.SourceType == FeedSourceType.DirectAccess)
			{
				RssBanditApplication app = sender as RssBanditApplication;
				if (app != null)
				{
					if (e.Entry.Source.UserIdentity != null &&
					    e.Entry.Source.UserIdentity.ContainsKey(app.Preferences.UserIdentityForComments))
						InitWith(e.Entry.Source.UserIdentity[app.Preferences.UserIdentityForComments]);
				}
			}
        }

        /// <summary>
        /// When the user changes any preferences, this method is called to 
        /// make sure that we have a correct identity referer url.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreferencesChanged(object sender, EventArgs e)
        {
            RssBanditApplication app = (RssBanditApplication) sender;
            if (app != null)
            {
                if (app.FeedHandler.UserIdentity != null &&
                    app.FeedHandler.UserIdentity.ContainsKey(app.Preferences.UserIdentityForComments))
                    InitWith(app.FeedHandler.UserIdentity[app.Preferences.UserIdentityForComments]);
            }
        }

        private void InitWith(IUserIdentity ui)
        {
            if (ui != null && ui.ReferrerUrl != null && ui.ReferrerUrl.Length > 0)
                _referrer = ui.ReferrerUrl;
        }

        #region Implementation of INewsItemFilter

        /// <summary>
        /// Returns true if the NewsItem has an outgoing link to the default user's identity 
        /// referer url
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Match(NewsItem item)
        {
            if (_referrer != null && item != null)
            {
                // && !item.BeenRead)
                if ((item.HasContent) && (item.Content.IndexOf(_referrer) >= 0))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the font of the list view item to the color used to denote 
        /// an item that links to the user's URL.  Currently this color is 
        /// Blue.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lvItem"></param>
        public void ApplyAction(NewsItem item, ThreadedListViewItem lvItem)
        {
            if (lvItem != null)
            {
                lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.ReferenceStyle);
                lvItem.ForeColor = FontColorHelper.ReferenceColor;
            }
        }

        #endregion
    }

    internal class NewsItemFlagFilter : INewsItemFilter
    {
        #region Implementation of INewsItemFilter

        /// <summary>
        /// Returns true if the NewsItem has a flag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Match(NewsItem item)
        {
            if (item != null && item.FlagStatus != Flagged.None)
                return true;
            return false;
        }

        /// <summary>
        /// Sets the font of the list view item to the color used to denote 
        /// an item that links to the user's URL.  Currently this color is 
        /// Blue.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lvItem"></param>
        public void ApplyAction(NewsItem item, ThreadedListViewItem lvItem)
        {
            if (lvItem != null && item != null)
            {
                lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.HighlightStyle);
                lvItem.ForeColor = FontColorHelper.HighlightColor;
            }
        }

        #endregion
    }
}