#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

namespace NewsComponents
{
	public interface INntpServerDefinition
	{
		/// <remarks/>
		string Name { get; }

		/// <remarks/>
		string DefaultIdentity{ get; }

		/// <remarks/>
		bool PreventDownloadOnRefresh{ get; }

		/// <remarks/>
		string Server{ get; }

		/// <remarks/>
		string AuthUser{ get; }
		/// <remarks/>
		System.Byte[] AuthPassword{ get; }
		/// <remarks/>
		bool UseSecurePasswordAuthentication{ get; }

		/// <remarks/>
		int Port{ get; }
		
		/// <remarks>Makes the 'nntp:' a 'nntps:'</remarks>
		bool UseSSL{ get; }

		/// <remarks/>
		int Timeout{ get; }
	}
}