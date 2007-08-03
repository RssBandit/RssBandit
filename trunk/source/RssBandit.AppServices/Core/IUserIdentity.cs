#region CVS Version Header
/*
 * $Id: IUserIdentity.cs,v 1.1 2005/09/08 13:42:45 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/09/08 13:42:45 $
 * $Revision: 1.1 $
 */
#endregion

namespace NewsComponents
{
	/// <summary>
	/// IUserIdentity interface
	/// </summary>
	public interface IUserIdentity
	{
	
		/// <remarks/>
		string Name { get; }

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("real-name")]
			string RealName { get; }

			/// <remarks/>
			string Organization { get; }

			/// <remarks/>
			string MailAddress { get; }

			/// <remarks/>
			string ResponseAddress { get; }

			/// <remarks/>
			string ReferrerUrl { get; }

			/// <remarks/>
			string Signature { get; }

	}
}
