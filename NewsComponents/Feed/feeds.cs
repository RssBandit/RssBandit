#region CVS Version Header
/*
 * $Id: feeds.cs,v 1.17 2005/04/24 18:21:34 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/24 18:21:34 $
 * $Revision: 1.17 $
 */
#endregion

using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace NewsComponents.Feed {

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	[System.Xml.Serialization.XmlRootAttribute("feeds", Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/", IsNullable=false)]
	public class feeds {
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("feed", Type = typeof(feedsFeed), IsNullable = false)]
		public ArrayList feed = new ArrayList();

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("category", Type = typeof(category), IsNullable = false)]
		public ArrayList categories = new ArrayList();	

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayAttribute("listview-layouts")]
		[System.Xml.Serialization.XmlArrayItemAttribute("listview-layout", Type = typeof(listviewLayout), IsNullable = false)]
		public ArrayList listviewLayouts = new ArrayList();	

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("server", Type = typeof(NntpServerDefinition), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayAttribute(ElementName = "nntp-servers", IsNullable = false)]
		public ArrayList nntpservers = new ArrayList();	

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("identity", Type = typeof(UserIdentity), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayAttribute(ElementName = "user-identities", IsNullable = false)]
		public ArrayList identities = new ArrayList();	

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("refresh-rate")]
		public int refreshrate;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool refreshrateSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("download-enclosures")]
		public bool downloadenclosures;

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool downloadenclosuresSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("mark-items-read-on-exit")]
		public bool markitemsreadonexit;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool markitemsreadonexitSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("enclosure-folder")]
		public string enclosurefolder;


		///<summary>ID to an FeedColumnLayout</summary>
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("listview-layout")]
		public string listviewlayout;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("max-item-age", DataType="duration")]
		public string maxitemage;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string stylesheet;
    

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;
	}


	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class listviewLayout {
		public listviewLayout() {}
		public listviewLayout(string id,  FeedColumnLayout layout) {
			this.ID = id;
			this.FeedColumnLayout = layout;
		}
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ID;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute()] //?
		public FeedColumnLayout FeedColumnLayout;
	}



	/// <summary>
	/// Summary description for IFeedColumnLayout.
	/// </summary>
	public interface IFeedColumnLayout {
		string SortByColumn { get; }
		SortOrder SortOrder { get ; }
		string ArrangeByColumn { get; }
		IList Columns { get; }
		IList ColumnWidths { get; }
	}

	[Serializable]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class FeedColumnLayout: IFeedColumnLayout, ICloneable, ISerializable {
		private string _sortByColumn;
		private SortOrder _sortOrder;
		private LayoutType _layoutType;
		private string _arrangeByColumn;
		internal ArrayList _columns;
		internal ArrayList _columnWidths;

		public FeedColumnLayout():
			this(null, null, null, SortOrder.None, LayoutType.IndividualLayout, null) {	}
		public FeedColumnLayout(ICollection columns, ICollection columnWidths, string sortByColumn, SortOrder sortOrder, LayoutType layoutType):
			this(columns, columnWidths, sortByColumn, sortOrder, layoutType, null) {	}
		
		public FeedColumnLayout(ICollection columns, ICollection columnWidths, string sortByColumn, SortOrder sortOrder, LayoutType layoutType, string arrangeByColumn) {
			if (columns != null)
				_columns = new ArrayList(columns);
			else
				_columns = new ArrayList();
			if (columnWidths != null)
				_columnWidths = new ArrayList(columnWidths);
			else
				_columnWidths = new ArrayList();

			_sortOrder = SortOrder.None;
			if (sortByColumn != null && _columns.IndexOf(sortByColumn) >= 0) {
				_sortByColumn = sortByColumn;
				_sortOrder = sortOrder;
			}
			if (arrangeByColumn != null && _columns.IndexOf(arrangeByColumn) >= 0) {
				_arrangeByColumn = arrangeByColumn;
			}
			_layoutType = layoutType;
		}

		public static FeedColumnLayout CreateFromXML(string xmlString) {
			if (xmlString != null && xmlString.Length > 0) {
				XmlSerializer formatter = new XmlSerializer(typeof(FeedColumnLayout));
				StringReader reader = new StringReader(xmlString);
				return (FeedColumnLayout)formatter.Deserialize(reader);
			}
			return null;
		}

		public static string SaveAsXML(FeedColumnLayout layout) {
			if (layout == null)
				return null;
			try {
				XmlSerializer formatter = new XmlSerializer(typeof(FeedColumnLayout));
				StringWriter writer = new StringWriter();
				formatter.Serialize(writer, layout);
				return writer.ToString();
			} catch (Exception ex) {
				Trace.WriteLine("SaveAsXML() failed.", ex.Message);
			}
			return null;
		}

		#region IFeedColumnLayout Members

		public LayoutType LayoutType {
			get {	return _layoutType; }
			set { _layoutType = value; }
		}


		public string SortByColumn {
			get {	return _sortByColumn;	}
			set {	_sortByColumn = value;	}
		}

		public SortOrder SortOrder {
			get {	return _sortOrder; }
			set { _sortOrder = value; }
		}

		public string ArrangeByColumn {
			get {	return _arrangeByColumn;	}
			set {	_arrangeByColumn = value;	}
		}

		[XmlIgnore]
		public IList Columns {
			get {	return _columns;	}
			set { 
				if (value != null)
					_columns = new ArrayList(value); 
				else
					_columns = new ArrayList();
			}
		}

		[XmlIgnore]
		public IList ColumnWidths {
			get {	return _columnWidths;	}
			set { 
				if (value != null)
					_columnWidths = new ArrayList(value); 
				else
					_columnWidths = new ArrayList();
			}
		}

		#endregion

		[XmlArrayItem(typeof(string))]
		public ArrayList ColumnList {
			get {	return _columns;	}
			set { 
				if (value != null)
					_columns = value; 
				else
					_columns = new ArrayList();
			}
		}
		[XmlArrayItem(typeof(int))]
		public ArrayList ColumnWidthList {
			get {	return _columnWidths;	}
			set { 
				if (value != null)
					_columnWidths = value; 
				else
					_columnWidths = new ArrayList();
			}
		}

		/// <summary>
		/// Compares two layouts for equality. This method also compares the column widths 
		/// when determining equality. 
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if they are equal</returns>
		public override bool Equals(Object obj){
			return this.Equals(obj, false); 
		}

		/// <summary>
		/// Compares  two layouts for equality.
		/// </summary>
		/// <param name="obj">the objects to compare</param>
		/// <param name="ignoreColumnWidths">indicates whether column widths should be ignored</param>
		/// <returns>true if they are equal</returns>
		public bool Equals(object obj, bool ignoreColumnWidths) {
			if (obj == null)
				return false;
			FeedColumnLayout o = obj as FeedColumnLayout;
			if (o== null)
				return false;
			if (this.SortOrder != o.SortOrder)
				return false;
			if (this.SortByColumn != o.SortByColumn)
				return false;
			if (this._columns == null && o._columns == null)
				return true;
			if (this._columns == null || o._columns == null)
				return false;
			if (this._columns.Count != o._columns.Count)
				return false;

			if(ignoreColumnWidths){

				for (int i = 0; i < this._columns.Count; i++) {
					if (String.Compare((string)this._columns[i], (string)o._columns[i]) != 0 )
						return false;
				}
				
			}else{
			
				for (int i = 0; i < this._columns.Count; i++) {
					if (String.Compare((string)this._columns[i], (string)o._columns[i]) != 0 || 
						(int)this._columnWidths[i] != (int)o._columnWidths[i])
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns true, if the layout is a kind of a feed layout (global, individual) and
		/// they are equal except for column widhts.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns>bool</returns>
		public bool IsSimilarFeedLayout(FeedColumnLayout layout)
		{
			if (layout == null)
				return false;

			if ((_layoutType == LayoutType.IndividualLayout || _layoutType == LayoutType.GlobalFeedLayout) &&
				(layout._layoutType == LayoutType.IndividualLayout || layout._layoutType == LayoutType.GlobalFeedLayout))
				return this.Equals(layout, true);
			return false;
		}

		/// <summary>
		/// Returns true, if the layout is a kind of a category layout (global, individual) and
		/// they are equal except for column widhts.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns>bool</returns>
		public bool IsSimilarCategoryLayout(FeedColumnLayout layout) {
			if (layout == null)
				return false;

			if ((_layoutType == LayoutType.IndividualLayout || _layoutType == LayoutType.GlobalCategoryLayout) &&
				(layout._layoutType == LayoutType.IndividualLayout || layout._layoutType == LayoutType.GlobalFeedLayout))
				return this.Equals(layout, true);
			return false;
		}

		public override int GetHashCode() {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (_columns != null && _columns.Count > 0) {
				for (int i = 0; i < _columns.Count; i++) {
					sb.AppendFormat("{0};", _columns[i]);
				}
			}
			if (_columnWidths != null && _columnWidths.Count > 0) {
				for (int i = 0; i < _columnWidths.Count; i++) {
					sb.AppendFormat("{0};", _columnWidths[i]);
				}
			}
			sb.AppendFormat("{0};", _sortByColumn);
			sb.AppendFormat("{0};", _sortOrder);
			sb.AppendFormat("{0};", _arrangeByColumn);
			sb.AppendFormat("{0};", _layoutType);

			return sb.ToString().GetHashCode();
		}

		#region ICloneable Members

		public object Clone() {
			return new FeedColumnLayout(_columns, _columnWidths, _sortByColumn, _sortOrder, _layoutType, _arrangeByColumn);
		}

		#endregion

		#region ISerializable Members

		protected FeedColumnLayout(SerializationInfo info, StreamingContext context) {
			int version = info.GetInt32("version");
			this._columns = (ArrayList)info.GetValue("ColumnList", typeof (ArrayList));
			this._columnWidths = (ArrayList)info.GetValue("ColumnWidthList", typeof (ArrayList));
			this._sortByColumn = (string)info.GetString("SortByColumn");
			this._sortOrder = (SortOrder)info.GetValue("SortOrder", typeof(SortOrder));
			this._arrangeByColumn = info.GetString("ArrangeByColumn");
		}


		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("version", 1);
			info.AddValue("ColumnList", this._columns);
			info.AddValue("ColumnWidthList", this._columnWidths);
			info.AddValue("SortByColumn", this._sortByColumn);
			info.AddValue("SortOrder", this._sortOrder);
			info.AddValue("ArrangeByColumn", this._arrangeByColumn);
		}

		#endregion
	}



	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class category {
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("mark-items-read-on-exit")]
		public bool markitemsreadonexit;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool markitemsreadonexitSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("download-enclosures")]
		public bool downloadenclosures;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool downloadenclosuresSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("enclosure-folder")]
		public string enclosurefolder;
    
		///<summary>ID to an FeedColumnLayout</summary>
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("listview-layout")]
		public string listviewlayout;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string stylesheet;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("refresh-rate")]
		public int refreshrate;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool refreshrateSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("max-item-age", DataType="duration")]
		public string maxitemage;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public category parent; 

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class feedsFeed {
    
		/// <remarks/>
		public string title;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
		public string link;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("refresh-rate")]
		public int refreshrate;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool refreshrateSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("last-retrieved")]
		public System.DateTime lastretrieved;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool lastretrievedSpecified;
    
		/// <remarks/>
		public string etag;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
		public string cacheurl;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("max-item-age", DataType="duration")]
		public string maxitemage;
    
    
		/// <remarks/>
		[System.Xml.Serialization.XmlArrayAttribute(ElementName = "stories-recently-viewed", IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("story", Type = typeof(System.String), IsNullable = false)]
		public ArrayList storiesrecentlyviewed = new ArrayList();

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayAttribute(ElementName = "deleted-stories", IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("story", Type = typeof(System.String), IsNullable = false)]
		public ArrayList deletedstories = new ArrayList();
  
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("if-modified-since")]
		public System.DateTime lastmodified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool lastmodifiedSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("auth-user")]
		public string authUser;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("auth-password", DataType="base64Binary")]
		public System.Byte[] authPassword;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("listview-layout")]
		public string listviewlayout;
    
		/// <remarks/>
		public string favicon;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("download-enclosures")]
		public bool downloadenclosures;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool downloadenclosuresSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("enclosure-folder")]
		public string enclosurefolder;
    
		/// <remarks/>
		public string stylesheet;
    
		/// <remarks>Reference the corresponding NntpServerDefinition</remarks>
		[System.Xml.Serialization.XmlElementAttribute("news-account")]
		public string newsaccount;
		
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("mark-items-read-on-exit")]
		public bool markitemsreadonexit;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool markitemsreadonexitSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any;
    

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("alert"), System.ComponentModel.DefaultValue(false) ]
		public bool alertEnabled;    
    
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool alertEnabledSpecified;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string category;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;

		/// <remarks>True, if the feed caused an exception on request to prevent sequenced
		/// error reports on every automatic download</remarks>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool causedException {
			get { return causedExceptionCount != 0; }
			set { 
				if (value) {
					causedExceptionCount++;	// raise counter
					lastretrievedSpecified = true;
					lastretrieved = new DateTime(DateTime.Now.Ticks); 
				} else 
					causedExceptionCount = 0;	// reset
			}
		}

		/// <remarks>Number of exceptions caused on requests</remarks>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int causedExceptionCount = 0;

		/// <remarks>Can be used to store any attached data</remarks>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object Tag;

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool containsNewMessages; 
	}

	#region UserIdentity
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class UserIdentity: ICloneable
	{
	
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
		public string Name;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("real-name")]
		public string RealName;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("organization")]
		public string Organization;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("mail-address")]
		public string MailAddress;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("response-address")]
		public string ResponseAddress;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("referrer-url")]
		public string ReferrerUrl;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("signature")]
		public string Signature;

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any;

		#region ICloneable Members

		public object Clone() {
			return this.MemberwiseClone();
		}

		#endregion
	}
	#endregion

	#region NewsServerDefinition
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2004/RSSBandit/feeds/")]
	public class NntpServerDefinition: ICloneable
	{
	
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("name")]
		public string Name;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("default-identity")]
		public string DefaultIdentity;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("prevent-download")]
		public bool PreventDownloadOnRefresh;
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool PreventDownloadOnRefreshSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("server-address")]
		public string Server;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("auth-user")]
		public string AuthUser;
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("auth-password", DataType="base64Binary")]
		public System.Byte[] AuthPassword;
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("auth-use-spa")]
		public bool UseSecurePasswordAuthentication;
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool UseSecurePasswordAuthenticationSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("port-number")]
		public int Port;
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool PortSpecified;
		
		/// <remarks>Makes the 'nntp:' a 'nntps:'</remarks>
		[System.Xml.Serialization.XmlElementAttribute("use-ssl")]
		public bool UseSSL;
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool UseSSLSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("timeout")]
		public int Timeout;
		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool TimeoutSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttributeAttribute()]
		public System.Xml.XmlAttribute[] AnyAttr;

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any;

		#region ICloneable Members

		public object Clone() {
			return this.MemberwiseClone();
		}

		#endregion
	}
	#endregion

}