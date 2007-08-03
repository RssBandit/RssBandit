#region CVS Version Header
/*
 * $Id: INntpServerDefinition.cs,v 1.1 2005/09/08 13:42:45 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/09/08 13:42:45 $
 * $Revision: 1.1 $
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