using System;
using System.Collections.Generic;
using System.Text;

using NewsComponents.Net;
using NewsComponents.Search;

namespace NewsComponents.Feed {
    /// <summary>
    /// A NewsHandler that directly accesses RSS/Atom feeds via HTTP or HTTPS 
    /// and newsgroups via NNTP. 
    /// </summary>
    class BanditNewsHandler : NewsHandler{

          /// <summary>
        /// Initializes a new instance of the <see cref="NewsHandler"/> class
        /// with a default configuration.
        /// </summary>
        public BanditNewsHandler() :
            this(NewsComponentsConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public BanditNewsHandler(INewsComponentsConfiguration configuration)
        {
            this.configuration = configuration;
            if (this.configuration == null)
                this.configuration = new NewsComponentsConfiguration();

            // check for programmers error in configuration:
            ValidateAndThrow(this.configuration);

            this.LoadFeedlistSchema();

            this.rssParser = new RssParser(this);

            //TODO: LuceneSearch and LuceneIndexer need to work in a world with multiple 
            //      NewsHandlers. 
            if(searchHandler == null)
                searchHandler = new LuceneSearch(this.configuration, this);

            // initialize (later on loaded from feedlist):
            this.PodcastFolder = this.configuration.DownloadedFilesDataPath;
            this.EnclosureFolder = this.configuration.DownloadedFilesDataPath;

            if (this.EnclosureFolder != null)
            {
                this.enclosureDownloader = new BackgroundDownloadManager(this.configuration, this);
                this.enclosureDownloader.DownloadCompleted += this.OnEnclosureDownloadComplete;
            }

            this.AsyncWebRequest = new AsyncWebRequest();
            this.AsyncWebRequest.OnAllRequestsComplete += this.OnAllRequestsComplete;
        }
    }
}
