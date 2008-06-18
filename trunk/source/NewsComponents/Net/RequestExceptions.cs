using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using NewsComponents.Feed;
using NewsComponents.Resources;

namespace NewsComponents.Net
{
	/// <summary>
	/// ResourceAuthorizationException is raised if a HTTP request results in a statuscode 401.
	/// </summary>
	[ComVisible(false)]
	public class ResourceAuthorizationException: WebException {
		/// <summary></summary>
		public ResourceAuthorizationException():base(ComponentsText.ExceptionResourceAuthorization){	}
		/// <summary></summary>
		public ResourceAuthorizationException(string message):base(message){	}
		/// <summary></summary>
		public ResourceAuthorizationException(string message, Exception innerException):base(message, innerException){	}
	}
	
	/// <summary>
	/// ResourceGoneException is raised if a HTTP request results in a statuscode 410.
	/// </summary>
	[ComVisible(false)]
	public class ResourceGoneException: WebException
	{
		/// <summary></summary>
		public ResourceGoneException():base(ComponentsText.ExceptionResourceGone){	}
		/// <summary></summary>
		public ResourceGoneException(string message):base(message){	}
		/// <summary></summary>
		public ResourceGoneException(string message, Exception innerException):base(message, innerException){	}
	}

	/// <summary>
	/// FeedRequestException is raised if a feed request fails.
	/// </summary>
	[ComVisible(false)]
	public class FeedRequestException: WebException {
		/// <summary></summary>
		public FeedRequestException():base(){	}
		/// <summary></summary>
		public FeedRequestException(string message, Hashtable context):base(message){
			this.context = context;
		}
		/// <summary></summary>
		public FeedRequestException(string message, Exception innerException, Hashtable context):base(message, innerException){
			this.context = context;
		}
		/// <summary></summary>
		public FeedRequestException(string message, WebExceptionStatus status, Hashtable context):base(message, status){
			this.context = context;
		}
		/// <summary></summary>
		public FeedRequestException(string message,  Exception innerException, WebExceptionStatus status, Hashtable context):base(message, innerException, status, null){
			this.context = context;
		}

		private Hashtable context = new Hashtable();

		private string GetValue(string key) {
			if (this.context != null && this.context.ContainsKey(key))
				return (string)this.context[key];
			return String.Empty;
		}
		private object GetObject(string key) {
			if (this.context != null && this.context.ContainsKey(key))
				return this.context[key];
			return null;
		}

		/// <summary>
		/// Gets the technical contact info (e-mail)
		/// </summary>
		public string TechnicalContact { get { return this.GetValue("TECH_CONTACT"); } }
		/// <summary>
		/// Gets the publisher contact info (e-mail)
		/// </summary>
		public string Publisher { get { return this.GetValue("PUBLISHER"); } }
		/// <summary>
		/// Gets the full path and title of the feed within the treeview (UI)
		/// </summary>
		public string FullTitle { get { return this.GetValue("FULL_TITLE"); } }
		/// <summary>
		/// Gets the publisher homepage (HTML Url)
		/// </summary>
		public string PublisherHomepage { get { return this.GetValue("PUBLISHER_HOMEPAGE"); } }
		/// <summary>
		/// Gets the generator info (usually generator software name and version)
		/// </summary>
		public string Generator { get { return this.GetValue("GENERATOR"); } }
		/// <summary>
		/// Gets the NewsFeed causing the exception
		/// </summary>
		public INewsFeed Feed { get { return (INewsFeed)this.GetObject("FAILURE_OBJECT"); } }
		/// <summary>
		/// Gets the NewsFeed causing the exception
		/// </summary>
		public FeedSource FeedSource { get { return (FeedSource)this.GetObject("SUBSCRIPTION_SOURCE"); } }

	}

}
