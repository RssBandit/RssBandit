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
