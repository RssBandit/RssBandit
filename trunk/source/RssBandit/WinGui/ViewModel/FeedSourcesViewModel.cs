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
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using log4net;
using NewsComponents;
using RssBandit.Common.Logging;
using RssBandit.Util;

namespace RssBandit.WinGui.ViewModel
{
    public class FeedSourcesViewModel : ModelBase
    {
        private static readonly ILog Log = DefaultLog.GetLogger(typeof (FeedSourcesViewModel));
        private readonly ObservableCollection<CategorizedFeedSourceViewModel> _sources = new ObservableCollection<CategorizedFeedSourceViewModel>();
        /// <summary>
        ///   Initializes a new instance of the <see cref = "FeedSourcesViewModel" /> class.
        ///   Uses FeedSourceManager to get feed sources.
        /// </summary>
        public FeedSourcesViewModel(FeedSourceManager sourceManager)
        {
            Contract.Requires(sourceManager != null);

            Sources = new ReadOnlyObservableCollection<CategorizedFeedSourceViewModel>(_sources);


            sourceManager.Sources.SynchronizeCollection(_sources, fs => new CategorizedFeedSourceViewModel(fs));


        }

        /// <summary>
        ///   Gets or sets the feed sources.
        /// </summary>
        /// <value>The sources.</value>
        public ReadOnlyObservableCollection<CategorizedFeedSourceViewModel> Sources { get; private set; }
    }
}