using System;
using System.Collections;
using System.Text.RegularExpressions;

using NewsComponents;

namespace ChannelServices.AdsBlocker
{
	/// <summary>
	/// Summary description for AdsBlockerChannel.
	/// </summary>
	public class AdsBlockerChannel: NewsItemChannel
	{
		private static string AdsReplacementFormat = "<span style='color:gray;'>[blocked Ad]<!-- ChannelServices.AdsBlocker blocked url: {0} --></span>";
		private static Regex RegExFindAHref = new Regex(@"<a\s+([^>]+\s+)?href\s*=\s*(?:""(?<1>[/\a-z0-9_][^""]*)""|'(?<1>[/\a-z0-9_][^']*)'|(?<1>[/\a-z0-9_]\S*))(\s[^>]*)?>(?<2>.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private ArrayList blockList = new ArrayList();

		string _content , _baseUrl;

		public AdsBlockerChannel():
			base("http://www.rssbandit.org/channels/newsitemcontent/adsblocker", 100) {
			// improvment: load from a list/file
			blockList.Add(new Regex(@"feeds\.feedburner\.com", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled));
		}

		public override INewsItem Process(INewsItem item) {
			if (blockList.Count > 0) {
				if (item.HasContent) {
					_content = item.Content;
					_baseUrl = (item.Link == null || item.Link.Length == 0) ? item.FeedLink : item.Link;

					// iterate the <a href="badUrl">...</a> matches
					if (RegExFindAHref.IsMatch(_content)) {
						item.SetContent(RegExFindAHref.Replace(_content, new MatchEvaluator(this.RegexTagEvaluate)), item.ContentType);
					}
				}
			}
			return base.Process (item);
		}

		private string RegexTagEvaluate(Match m) {
			string aElement = m.ToString();
			string inspectUrl =  ConvertToAbsoluteUrl(m.Groups[1].ToString(), _baseUrl);
			string linkText = m.Groups[2].ToString();	

			foreach (Regex r in blockList) {
				if (r.IsMatch(inspectUrl)) 
					return String.Format(AdsReplacementFormat, inspectUrl);
			}
			return aElement;
		}

		/// <summary>
		/// Converts a relative url to an absolute one. baseUrl is used as the base to fix the other.
		/// </summary>
		/// <param name="url">Url to fix</param>
		/// <param name="baseUrl">base Url to be used</param>
		/// <returns></returns>
		private string ConvertToAbsoluteUrl(string url, string baseUrl) {
			
			// we try to prevent the exception caused in the case the url is relative
			// (no scheme info) just for speed
			if (url.IndexOf(Uri.SchemeDelimiter) < 0 && baseUrl != null) {
				try {
					Uri baseUri= new Uri(baseUrl);
					return (new Uri(baseUri,url).ToString()); 
				} catch {}
			} 
			
			try{ 
				Uri uri = new Uri(url); 
				return uri.ToString(); 
			}catch(Exception){

				if (baseUrl != null) {
					try {
						Uri baseUri= new Uri(baseUrl);
						return (new Uri(baseUri,url).ToString()); 
					} catch (Exception) {
						return url;
					}
				} else {
					return url;
				}
			}
		}

	}
}
