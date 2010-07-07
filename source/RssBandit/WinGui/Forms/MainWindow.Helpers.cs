#undef USE_IG_UL_COMBOBOX

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RssBandit.WinGui.Controls.ThListView;
using RssBandit.WinGui.Controls.ThListView.Sorting;
using AppInteropServices;
using IEControl;
using Infragistics.Win.UltraWinTree;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using Syndication.Extensibility;
using SortOrder=System.Windows.Forms.SortOrder;
using Infragistics.Win.UltraWinEditors;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using System.Net;

namespace RssBandit.WinGui.Forms
{
    public partial class MainWindow
    {
       		
        /// <summary>
        /// Returns the FeedSource where this feed is subscribed in. 
        /// </summary>
        /// <param name="feedUrl">The specified feed</param>
        /// <returns>The FeedSource where the feed is subscribed</returns>
		public FeedSource FeedSourceOf(string feedUrl)
		{
			FeedSourceEntry entry = FeedSourceEntryOf(feedUrl);
			if (entry != null)
				return entry.Source;
			return null;
		}

        /// <summary>
        /// Returns the FeedSourceEntry where this feed is subscribed in. 
        /// </summary>
        /// <param name="feedUrl">The specified feed</param>
        /// <returns>The FeedSourceEntry where the feed is subscribed</returns>		
    	public FeedSourceEntry FeedSourceEntryOf(string feedUrl)
        {
            if (StringHelper.EmptyTrimOrNull(feedUrl))
                return null;

            return  RssBanditApplication.Current.FeedSources.Sources.FirstOrDefault(fse => fse.Source.IsSubscribed(feedUrl)); 
        }



    }
}