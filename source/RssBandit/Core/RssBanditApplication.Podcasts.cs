using System;
using System.IO;
using iTunesLib;
using NewsComponents;
using NewsComponents.Net;
using WMPLib;

namespace RssBandit
{
    internal partial class RssBanditApplication
    {
        #region Podcast related routines

        /// <summary>
        /// Gets the current Enclosure folder
        /// </summary>		
        public string EnclosureFolder
        {
            get
            {
				return Preferences.EnclosureFolder;
            }
        }


        /// <summary>
        /// Gets the current Podcast folder
        /// </summary>		
        public string PodcastFolder
        {
            get
            {
				return Preferences.PodcastFolder;
            }
        }

        /// <summary>
        /// Indicates the number of enclosures which should be downloaded automatically from a newly subscribed feed.
        /// </summary>
        public int NumEnclosuresToDownloadOnNewFeed
        {
            get
            {
                return Preferences.NumEnclosuresToDownloadOnNewFeed;
            }
        }


        /// <summary>
        /// Indicates the maximum amount of space that enclosures and podcasts can use on disk.
        /// </summary>
        public int EnclosureCacheSize
        {
            get
            {
                return Preferences.EnclosureCacheSize;
            }
        }

        /// <summary>
        /// Gets a semi-colon delimited list of file extensions of enclosures that 
        /// should be treated as podcasts
        /// </summary>
        public string PodcastFileExtensions
        {
            get
            {
                return Preferences.PodcastFileExtensions;
            }
        }

        /// <summary>
        /// Gets whether enclosures should be created in a subfolder named after the feed. 
        /// </summary>
        public bool DownloadCreateFolderPerFeed
        {
            get
            {
                return Preferences.CreateSubfoldersForEnclosures;
            }
        }

        /// <summary>
        /// Gets whether alert Windows should be displayed for enclosures or not. 
        /// </summary>
        public bool EnableEnclosureAlerts
        {
            get
            {
                return Preferences.EnclosureAlert;
            }
        }

        /// <summary>
        /// Gets whether enclosures should be downloaded automatically or not.
        /// </summary>
        public bool DownloadEnclosures
        {
            get
            {
                return Preferences.DownloadEnclosures;
            }
        }

        /// <summary>
        /// Tests whether a file type is supported by Windows Media Player by checking the 
        /// file extension. 
        /// </summary>
        /// <param name="fileExt">The file extension to test</param>
        /// <returns>True if the file extension is supported by Windows Media Player</returns>
        private static bool IsWMPFile(IEquatable<string> fileExt)
        {
            if (fileExt.Equals(".asf") || fileExt.Equals(".wma") || fileExt.Equals(".avi")
                || fileExt.Equals(".mpg") || fileExt.Equals(".mpeg") || fileExt.Equals(".m1v")
                || fileExt.Equals(".wmv") || fileExt.Equals(".wm") || fileExt.Equals(".asx")
                || fileExt.Equals(".wax") || fileExt.Equals(".wpl") || fileExt.Equals(".wvx")
                || fileExt.Equals(".wmd") || fileExt.Equals(".dvr-ms") || fileExt.Equals(".m3u")
                || fileExt.Equals(".mp3") || fileExt.Equals(".mp2") || fileExt.Equals(".mpa")
                || fileExt.Equals(".mpe") || fileExt.Equals(".mpv2") || fileExt.Equals(".wms")
                || fileExt.Equals(".mid") || fileExt.Equals(".midi") || fileExt.Equals(".rmi")
                || fileExt.Equals(".aif") || fileExt.Equals(".aifc") || fileExt.Equals(".aiff")
                || fileExt.Equals(".wav") || fileExt.Equals(".au") || fileExt.Equals(".snd")
                || fileExt.Equals(".ivf") || fileExt.Equals(".wmz"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the downloaded item to a playlist in Windows Media Player. 
        /// </summary>
        /// <remarks>The title of the playlist is the name of the feed in RSS Bandit.</remarks>
        /// <param name="podcast"></param>
        private void AddPodcastToWMP(DownloadItem podcast)
        {
            try
            {
                if (!IsWMPFile(Path.GetExtension(podcast.File.LocalName)))
                {
                    return;
                }

                string playlistName = Preferences.SinglePlaylistName;
                FeedSourceEntry entry = guiMain.FeedSourceOf(podcast.OwnerFeedId); 

                if (!Preferences.SinglePodcastPlaylist && entry!= null)
                {                    
                    playlistName = entry.Source.GetFeeds()[podcast.OwnerFeedId].title;
                }

                WindowsMediaPlayer wmp = new WindowsMediaPlayer();

                //get a handle to the playlist if it exists or create it if it doesn't				
                IWMPPlaylist podcastPlaylist = null;
                IWMPPlaylistArray playlists = wmp.playlistCollection.getAll();

                for (int i = 0; i < playlists.count; i++)
                {
                    IWMPPlaylist pl = playlists.Item(i);

                    if (pl.name.Equals(playlistName))
                    {
                        podcastPlaylist = pl;
                    }
                }

                if (podcastPlaylist == null)
                {
                    podcastPlaylist = wmp.playlistCollection.newPlaylist(playlistName);
                }

                IWMPMedia wm = wmp.newMedia(Path.Combine(podcast.TargetFolder, podcast.File.LocalName));
                podcastPlaylist.appendItem(wm);
            }
            catch (Exception e)
            {
                _log.Error("The following error occured in AddPodcastToWMP(): ", e);
            }
        }

        /// <summary>
        /// Tests whether a file type is supported by iTunesby checking the 
        /// file extension. 
        /// </summary>
        /// <param name="fileExt">The file extension to test</param>
        /// <returns>True if the file extension is supported by iTunes</returns>
        private static bool IsITunesFile(IEquatable<string> fileExt)
        {
            if (fileExt.Equals(".mov") || fileExt.Equals(".mp4") || fileExt.Equals(".mp3")
                || fileExt.Equals(".m4v") || fileExt.Equals(".m4a") || fileExt.Equals(".m4b")
                || fileExt.Equals(".m4p") || fileExt.Equals(".wav") || fileExt.Equals(".aiff")
                || fileExt.Equals(".aif") || fileExt.Equals(".aifc") || fileExt.Equals(".aa"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the downloaded item to a playlist in iTunes. 
        /// </summary>
        /// <remarks>The title of the playlist is the name of the feed in RSS Bandit.</remarks>
        /// <param name="podcast"></param>
        private void AddPodcastToITunes(DownloadItem podcast)
        {
            try
            {
                if (!IsITunesFile(Path.GetExtension(podcast.File.LocalName)))
                {
                    return;
                }

                string playlistName = Preferences.SinglePlaylistName;
                FeedSourceEntry entry = guiMain.FeedSourceOf(podcast.OwnerFeedId); 

                if (!Preferences.SinglePodcastPlaylist && entry!= null)
                {
                    playlistName = entry.Source.GetFeeds()[podcast.OwnerFeedId].title;
                }

                // initialize iTunes application connection
                iTunesApp itunes = new iTunesApp();

                //get a handle to the playlist if it exists or create it if it doesn't				
                IITUserPlaylist podcastPlaylist = null;

                foreach (IITPlaylist pl in itunes.LibrarySource.Playlists)
                {
                    if (pl.Name.Equals(playlistName))
                    {
                        podcastPlaylist = (IITUserPlaylist) pl;
                    }
                }

                if (podcastPlaylist == null)
                {
                    podcastPlaylist = (IITUserPlaylist) itunes.CreatePlaylist(playlistName);
                }

                //add podcast to our playlist for this feed							
                podcastPlaylist.AddFile(Path.Combine(podcast.TargetFolder, podcast.File.LocalName));
            }
            catch (Exception e)
            {
                _log.Error("The following error occured in AddPodcastToITunes(): ", e);
            }
        }

        #endregion
    }
}