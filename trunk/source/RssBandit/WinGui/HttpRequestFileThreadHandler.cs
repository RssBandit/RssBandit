#region CVS Version Header
/*
 * $Id: HttpRequestFileThreadHandler.cs,v 1.7 2005/04/08 15:00:18 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/08 15:00:18 $
 * $Revision: 1.7 $
 */
#endregion

using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;

using NewsComponents.Feed;
using NewsComponents.Net;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Summary description for HttpRequestFileThreadHandler.
	/// </summary>
	public class HttpRequestFileThreadHandler: EntertainmentThreadHandlerBase
	{
		private string requestUrl = String.Empty;
		private IWebProxy proxy = null;
		private Stream responseStream = null;
		
		private HttpRequestFileThreadHandler() {;}
		public HttpRequestFileThreadHandler(string requestUrl, IWebProxy proxy) {
			this.requestUrl = requestUrl;
			this.proxy = proxy;
		}

		public string Url {
			get {	return this.requestUrl;	}
			set {	this.requestUrl = value;	}
		}

		public IWebProxy Proxy {
			get {	return this.proxy;	}
			set {	this.proxy = value;	}
		}

		public Stream ResponseStream {
			get {	return responseStream;	}
		}

		protected override void Run() {

			this.responseStream = null;
			
			try {
				this.responseStream = AsyncWebRequest.GetSyncResponseStream(
					this.requestUrl, null, RssBanditApplication.UserAgent, this.Proxy); 							
			} catch (ThreadAbortException) {
				// eat up
			} catch (Exception e) {	// fatal errors
				this.p_operationException = e;
			} finally {
				WorkDone.Set();
			}
		}
		
	}
}
