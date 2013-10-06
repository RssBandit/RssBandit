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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using NewsComponents;
using NewsComponents.Collections;
using RssBandit.WinGui;

namespace RssBandit.Core.Storage.Serialization
{
	[Serializable]
	public class StatefullKeyItemCollection<TK, TI>: ObservableKeyItemCollection<TK, TI>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StatefullKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		public StatefullKeyItemCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatefullKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public StatefullKeyItemCollection(int capacity) : base(capacity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatefullKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		protected StatefullKeyItemCollection(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="StatefullKeyItemCollection&lt;TK, TI&gt;"/> is modified.
		/// </summary>
		/// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
		public bool Modified { get; set; }

		protected override void CollectionWasChanged(KeyItemChange change, int position)
		{
			Modified = true;
		}

	}

	#region Column layouts

	/// <summary>
	/// Column Layout Definitions serializable root class
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("column-layouts", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableColumnLayoutDefinitions
	{
		/// <summary/>
		[XmlElement("column-layout", Type = typeof(listviewLayout), IsNullable = false)]
		public List<listviewLayout> ColumnLayouts = new List<listviewLayout>();
	}

	/// <remarks/>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	public class listviewLayout
	{
		public listviewLayout()
		{
		}

		public listviewLayout(string id, IFeedColumnLayout layout)
		{
			ID = id;
			FeedColumnLayout = (FeedColumnLayout)layout;
		}

		/// <remarks/>
		[XmlAttribute]
		public string ID;

		/// <remarks/>
		[XmlAnyAttribute]
		public XmlAttribute[] AnyAttr;

		/// <remarks/>
		[XmlElement] //?
			public FeedColumnLayout FeedColumnLayout;
	}

	#endregion

	#region Identities

	/// <summary>
	/// Identities serializable root class
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("root", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableIdentities
	{
		/// <summary/>
		[XmlArrayItem("identity", Type = typeof (UserIdentity), IsNullable = false)]
		[XmlArray(ElementName = "user-identities", IsNullable = false)]
		public List<UserIdentity> identities = new List<UserIdentity>();
	}

	#region UserIdentity

	/// <remarks/>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	public class UserIdentity : IUserIdentity, ICloneable
	{
		/// <remarks/>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <remarks/>
		[XmlElement("real-name")]
		public string RealName { get; set; }

		/// <remarks/>
		[XmlElement("organization")]
		public string Organization { get; set; }

		/// <remarks/>
		[XmlElement("mail-address")]
		public string MailAddress { get; set; }

		/// <remarks/>
		[XmlElement("response-address")]
		public string ResponseAddress { get; set; }

		/// <remarks/>
		[XmlElement("referrer-url")]
		public string ReferrerUrl { get; set; }

		/// <remarks/>
		[XmlElement("signature")]
		public string Signature { get; set; }

		/// <remarks/>
		[XmlAnyAttribute]
		public XmlAttribute[] AnyAttr;

		/// <remarks/>
		[XmlAnyElement]
		public XmlElement[] Any;

		#region ICloneable Members

		public object Clone()
		{
			return MemberwiseClone();
		}

		#endregion

		public override string ToString()
		{
			return Name;
		}
	}

	#endregion

	#endregion

}
