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
using System.Text.RegularExpressions;

namespace ChannelServices.AdsBlocker.AddIn
{
	/// <summary>
	/// Summary description for UserAdsBlacklist.
	/// </summary>
	public class UserAdsBlacklist: IBlacklist
	{
		private static bool loaded = false;
		private static Regex blackListRegex = null;
		private static string blacklist = null;
		private static readonly object blackListLock = new object();

	    #region IBlacklist Members

		public void Initialize(string newBlackList) {
			lock(blackListLock) {
				if (blacklist == null || blacklist != newBlackList) {
					if (newBlackList != null && newBlackList.Length > 0) {
						blacklist = newBlackList;
						blacklist = blacklist.Replace(";","|");
						blackListRegex = new Regex(blacklist,RegexOptions.IgnoreCase|RegexOptions.Singleline);
					} else {
                        throw new ArgumentNullException("newBlackList");
					}
				}
			}

			loaded = true;
		}

		public ListUpdateState UpdateBlacklist() {
			return ListUpdateState.None;
		}

		public Match IsBlacklisted(Uri uri) {
			if (!loaded || uri == null) 
				return null;
			
			try {
				Match match;
				// we want to remove the Query from the url as it 
				// may contain keywords that easily match the black list. 
				// However, we CHECK against the stripped uri but log the COMPLETE referral!
				string strippedUri = uri.Scheme + "://" + uri.Authority + uri.AbsolutePath;

				lock (blackListLock) {
					match = blackListRegex.Match(strippedUri);
				}

				return match;
			}
			catch (Exception ex) {
				throw new Exception(String.Format("An error occured trying to determine if {0} is blacklisted", uri), ex.InnerException);
			}
		}

		#endregion
	}
}
