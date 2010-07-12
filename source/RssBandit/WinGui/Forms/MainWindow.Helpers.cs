#undef USE_IG_UL_COMBOBOX

using System.Linq;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.WinGui.ViewModel;

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

        public MainWindowViewModel Model
        {
            get { return (MainWindowViewModel)DataContext; }
        }

    }
}