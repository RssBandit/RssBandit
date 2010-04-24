#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Collections.ObjectModel;
using log4net;
using NewsComponents;
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.ViewModel
{
    public class FeedSourcesViewModel: ViewModelBase
    {
        private static readonly ILog _log = Log.GetLogger(typeof(FeedSourcesViewModel));

        ObservableCollection<FeedSourceViewModel> _sources;
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSourcesViewModel"/> class.
        /// Uses FeedSourceManager to get feed sources.
        /// </summary>
        public FeedSourcesViewModel()
        {
            _sources = new ObservableCollection<FeedSourceViewModel>();
            
            foreach (FeedSourceEntry entry in RssBanditApplication.Current.FeedSources.Sources)
            {
                if (entry.Source.FeedsListOK)
                {
                    _sources.Add(new FeedSourceViewModel(entry));
                    
                }
                else
                {
                    _log.Error("Feed source reported list was not OK: " + entry.Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the feed sources.
        /// </summary>
        /// <value>The sources.</value>
        public ObservableCollection<FeedSourceViewModel> Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }
    }
}
