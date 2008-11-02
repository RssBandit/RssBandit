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
using System.IO;
using System.Reflection;
using System.Text;
using RssBandit.AppServices;
using RssBandit.UIServices;

namespace ChannelServices.AdsBlocker.AddIn
{
	/// <summary>
	/// AdsBlockerAddIn. The AddIn connector class.
	/// </summary>
	public class AdsBlockerAddIn: AddInBase, IAddInPackage
	{
		private readonly NewsItemAdsBlocker _adsBlocker;
		private ICoreApplication _app;

		public AdsBlockerAddIn()
		{
			_adsBlocker = new NewsItemAdsBlocker();
		}

		#region IAddInPackage Members
		
		/// <summary>
		/// Called on loading an AddInPackage.
		/// </summary>
		/// <param name="serviceProvider">IServiceProvider</param>
		public void Load(IServiceProvider serviceProvider) {

			string path = GetConfigLocation();
			string blackListFile = Path.Combine(path, "ads.blacklist.txt");
			string whiteListFile = Path.Combine(path, "ads.whitelist.txt");

			if (File.Exists(blackListFile)) {
				using (Stream s = File.OpenRead(blackListFile)) {
					using (StreamReader r = new StreamReader(s, Encoding.UTF8)) {
						BlackWhiteListFactory.AddBlacklist(new UserAdsBlacklist(), ParseContent(r));
					}
				}
			}

			if (File.Exists(whiteListFile)) {
				using (Stream s = File.OpenRead(whiteListFile)) {
					using (StreamReader r = new StreamReader(s, Encoding.UTF8)) {
						BlackWhiteListFactory.AddWhitelist(new UserAdsWhitelist(), ParseContent(r));
					}
				}
			}

			if (BlackWhiteListFactory.HasBlacklists) {
				// register displaying channel processor:
				_app = (ICoreApplication) serviceProvider.GetService(typeof(ICoreApplication));
				_app.RegisterDisplayingNewsChannelProcessor(_adsBlocker);
				//TODO: add menu(s) to manage blocklist, or how the ads are visualized
			}
		}

		/// <summary>
		/// Called on unloading an AddInPackage. Use it for cleanup task(s).
		/// </summary>
		public void Unload() {
			if (BlackWhiteListFactory.HasBlacklists && _app != null) {
				_app.UnregisterDisplayingNewsChannelProcessor(_adsBlocker);
			}
		}
		
		#endregion

		#region IDisposable Members

		public new void Dispose() {
			// cleanup resorces
			_app = null;
		}

		#endregion

		#region private members
		private static string GetConfigLocation() {
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
		private static string ParseContent(StreamReader reader) {
			StringBuilder sb = new StringBuilder();
			string line = null;
			// Read and display lines from the file until the end of 
			// the file is reached.
			while ((line = reader.ReadLine()) != null) {
				if (!line.StartsWith("#")) {
					if (line.IndexOf('#') > -1) {
						line = line.Substring(0, line.IndexOf('#'));
					}

					sb.Append(line.TrimEnd(' ', '\t'));
					sb.Append(";");
				}
			}
			return sb.ToString().TrimEnd(';');
		}
		
		#endregion
	}
}
