using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RssBandit.WinGui
{
    public static class CommonImages
    {
        public static ComponentResourceKey BanditFeedSourceKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "BanditFeedSourceKey");
            }
        }

        public static ComponentResourceKey GoogleFeedSourceKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "GoogleFeedSourceKey");
            }
        }

        public static ComponentResourceKey FacebookFeedSourceKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "FacebookFeedSourceKey");
            }
        }

        public static ComponentResourceKey WindowsFeedSourceKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "WindowsFeedSourceKey");
            }
        }

        public static ComponentResourceKey FolderOpenKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "FolderOpenKey");
            }
        }

        public static ComponentResourceKey FolderClosedKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "FolderClosedKey");
            }
        }

        public static ComponentResourceKey NntpFeedIconKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "NntpFeedIconKey");
            }
        }

        public static ComponentResourceKey RssFeedIconKey
        {
            get
            {
                return new ComponentResourceKey(typeof(CommonImages), "RssFeedIconKey");
            }
        }

    }
}
