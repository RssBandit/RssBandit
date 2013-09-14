#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#define SANDBOX

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using Lucene.Net.Util;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace NewsComponents.Feed.Sources
{
	internal class FeedlyCloudFeedSource : FeedSource
	{
		#region private fields

		// logging/tracing:
		private static readonly ILog _log = Log.GetLogger(typeof(FeedlyCloudFeedSource));

#if SANDBOX
		/// <summary>
		/// The first part of the URL to various Feedly Cloud API end points
		/// </summary>
		private static readonly string _apiUrlPrefix = @"https://sandbox.feedly.com";
#else
		/// <summary>
		/// The first part of the URL to various Feedly Cloud API end points
		/// </summary>
		private static readonly string _apiUrlPrefix = @"https://cloud.feedly.com";
#endif

		/// <summary>
		/// The URL for authenticating a Google user (currently used by feedly).
		/// </summary>
		private static readonly string _authUrl = _apiUrlPrefix + @"/v3/auth/auth";

		private const string AuthScope = @"https://cloud.feedly.com/subscriptions"; 

		/// <summary>
		/// The body of the request that will authenticate the feedly user. 
		/// </summary>
		private static readonly string _authBody = @"response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}";

		private string _authCode = String.Empty;

		private string _googleUserId = String.Empty;

		/// <summary>
		/// Updates Feedly Cloud in a background thread.
		/// </summary>
		private static FeedlyCloudUpdater _feedlyCloudUpdater;

		#endregion

		#region ctor's

		private FeedlyCloudFeedSource()
		{
		}

		internal FeedlyCloudFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
		{
			this.p_configuration = configuration;
			if (this.p_configuration == null)
				this.p_configuration = FeedSource.DefaultConfiguration;

			this.location = location;
			
			// check for programmers error in configuration:
			ValidateAndThrow(this.Configuration);

			//register with background thread for updating Google Reader
			FeedlyCloudUpdater.RegisterFeedSource(this);

		}

		#endregion

		#region public properties

		/// <summary>
		/// Returns the Google username associated with this feed source
		/// </summary>
		public string GoogleUserName
		{
			get { return this.location.Credentials.UserName; }
		}

		/// <summary>
		/// The Google User ID of the user. 
		/// </summary>
		public string GoogleUserId
		{
			get { return _googleUserId; }
			private set
			{
				this._googleUserId = value;
			}
		}

		/// <summary>
		/// Gets or sets the Feedly Reader updater
		/// </summary>
		/// <value>The FeedlyReaderUpdater instance used by all instances of this class.</value>
		internal FeedlyCloudUpdater FeedlyCloudUpdater
		{
			get
			{
				if (_feedlyCloudUpdater == null)
				{
					_feedlyCloudUpdater = new FeedlyCloudUpdater(this.Configuration.UserApplicationDataPath);
				}
				return _feedlyCloudUpdater;
			}
			set
			{
				_feedlyCloudUpdater = value;
			}
		}

		/// <summary>
		/// Gets the client unique identifier.
		/// </summary>
		/// <returns>string</returns>
		internal string GetClientId()
		{
			// <remarks>
			// Real API ID: rssbandit (real)
			// API ID: effonaqvg (rot13)
			// </remarks>
			return Utils.StringHelper.Rot13("effonaqvg");
		}

		/// <summary>
		/// Gets the API secret.
		/// </summary>
		/// <returns></returns>
		internal string GetApiSecret()
		{
			// <remarks>
			// Real Secret: KKBBU08S5R8KTKJERP5T152A
			// Secret: XXOOH08F5E8XGXWREC5G152N (rot13)
			// </remarks>
			return Utils.StringHelper.Rot13("XXOOH08F5E8XGXWREC5G152N");
		}

		#endregion 

		#region FeedSource overrides

		public override void LoadFeedlist()
		{
			feeds myFeeds = null;
			if (File.Exists(this.location.Location))
			{
				//load Bandit subscriptions.xml document into memory
				XmlReader reader = XmlReader.Create(this.location.Location);
				XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
				myFeeds = (feeds)serializer.Deserialize(reader);
				reader.Close();
			}
			else
			{
				myFeeds = new feeds();
			}

			//load feed list from Feedly Cloud and use settings from subscriptions.xml
			this.BootstrapAndLoadFeedlist(myFeeds);
			//FeedlyCloudUpdater.StartBackgroundThread(); 
		}

		public override void BootstrapAndLoadFeedlist(feeds feedlist)
		{
			if (Offline)
			{
				//TODO
			}
			else
			{
				//matchup feeds from Feedly cloud with 
				var cloudFeeds = this.LoadFeedlistFromFeedlyCloud();

			}
		}

		#endregion

		#region user authentication methods

		/// <summary>
		/// Authenticates a user to Feedly's services and obtains their 'code'
		/// </summary>
		private void AuthenticateUser()
		{
			string fullUrl = _authUrl +"?" + String.Format(_authBody, GetClientId(), 
				HtmlHelper.UrlEncode( "urn:rssbandit.org"), HtmlHelper.UrlEncode(AuthScope), HtmlHelper.UrlEncode("1.9.x_experimental"));
			try
			{
				StreamReader reader = new StreamReader(SyncWebRequest.GetResponseStream(fullUrl, null, this.Proxy));
				string[] response = reader.ReadToEnd().Split('\n');

				foreach (string s in response)
				{
					if (s.Contains("code="))
					{
						this._authCode = s.Substring(s.IndexOf("code=", StringComparison.OrdinalIgnoreCase) + 5);
						return;
					}
				}
			}
			catch (ClientCertificateRequiredException) // Google/feedly returns a 403 instead of a 401 on invalid password
			{
				throw new ResourceAuthorizationException();
			}

			throw new WebException("Could not authenticate user to Feedly Cloud because no authentication token provided in response", WebExceptionStatus.UnknownError);
		}

		#endregion

		#region Cloud service access methods

		private object LoadFeedlistFromFeedlyCloud()
		{
			AuthenticateUser();
			return null;
		}

		#endregion
	}
}
