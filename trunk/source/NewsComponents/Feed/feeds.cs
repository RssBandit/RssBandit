#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NewsComponents.Feed
{
    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    [XmlRoot("feeds", Namespace=NamespaceCore.Feeds_vCurrent, IsNullable=false)]
    public class feeds
    {
        /// <remarks/>
        [XmlElement("feed", Type = typeof (NewsFeed), IsNullable = false)]
        public List<NewsFeed> feed = new List<NewsFeed>();

        /// <remarks/>
        [XmlArrayItem("category", Type = typeof (category), IsNullable = false)]
        public List<category> categories = new List<category>();

        /// <remarks/>
        [XmlArray("listview-layouts")]
        [XmlArrayItem("listview-layout", Type = typeof (listviewLayout), IsNullable = false)]
        public List<listviewLayout> listviewLayouts = new List<listviewLayout>();

        /// <remarks/>
        [XmlArrayItem("server", Type = typeof (NntpServerDefinition), IsNullable = false)]
        [XmlArray(ElementName = "nntp-servers", IsNullable = false)]
        public List<NntpServerDefinition> nntpservers = new List<NntpServerDefinition>();

        /// <remarks/>
        [XmlArrayItem("identity", Type = typeof (UserIdentity), IsNullable = false)]
        [XmlArray(ElementName = "user-identities", IsNullable = false)]
        public List<UserIdentity> identities = new List<UserIdentity>();

        /// <remarks/>
        [XmlAttribute("refresh-rate")]
        public int refreshrate;

        /// <remarks/>
        [XmlIgnore]
        public bool refreshrateSpecified;

        /// <remarks/>
        [XmlAttribute("create-subfolders-for-enclosures"), DefaultValue(false)]
        public bool createsubfoldersforenclosures;

        /// <remarks/>
        [XmlIgnore]
        public bool createsubfoldersforenclosuresSpecified;

        /// <remarks/>
        [XmlAttribute("download-enclosures")]
        public bool downloadenclosures;

        /// <remarks/>
        [XmlIgnore]
        public bool downloadenclosuresSpecified;


        /// <remarks/>
        [XmlAttribute("enclosure-cache-size-in-MB")]
        public int enclosurecachesize;

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurecachesizeSpecified;

        /// <remarks/>
        [XmlAttribute("num-enclosures-to-download-on-new-feed")]
        public int numtodownloadonnewfeed;

        /// <remarks/>
        [XmlIgnore]
        public bool numtodownloadonnewfeedSpecified;


        /// <remarks/>
        [XmlAttribute("enclosure-alert")]
        public bool enclosurealert;

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurealertSpecified;


        /// <remarks/>
        [XmlAttribute("mark-items-read-on-exit")]
        public bool markitemsreadonexit;

        /// <remarks/>
        [XmlIgnore]
        public bool markitemsreadonexitSpecified;

        /// <remarks/>
        [XmlAttribute("enclosure-folder")]
        public string enclosurefolder;


        /// <remarks/>
        [XmlAttribute("podcast-folder")]
        public string podcastfolder;

        /// <remarks/>
        [XmlAttribute("podcast-file-exts")]
        public string podcastfileexts;


        ///<summary>ID to an FeedColumnLayout</summary>
        /// <remarks/>
        [XmlAttribute("listview-layout")]
        public string listviewlayout;

        /// <remarks/>
        [XmlAttribute("max-item-age", DataType="duration")]
        public string maxitemage;

        /// <remarks/>
        [XmlAttribute]
        public string stylesheet;


        /// <remarks/>
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttr;
    }


    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class listviewLayout
    {
        public listviewLayout()
        {
        }

        public listviewLayout(string id, FeedColumnLayout layout)
        {
            ID = id;
            FeedColumnLayout = layout;
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


    /// <summary>
    /// Summary description for IFeedColumnLayout.
    /// </summary>
    public interface IFeedColumnLayout
    {
        string SortByColumn { get; }
        SortOrder SortOrder { get; }
        string ArrangeByColumn { get; }
        IList<string> Columns { get; }
        IList<int> ColumnWidths { get; }
    }

    [Serializable]
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class FeedColumnLayout : IFeedColumnLayout, ICloneable, ISerializable
    {
        private string _sortByColumn;
        private SortOrder _sortOrder;
        private LayoutType _layoutType;
        private string _arrangeByColumn;
        internal List<string> _columns;
        internal List<int> _columnWidths;

        public FeedColumnLayout() :
            this(null, null, null, SortOrder.None, LayoutType.IndividualLayout, null)
        {
        }

        public FeedColumnLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
                                SortOrder sortOrder, LayoutType layoutType) :
                                    this(columns, columnWidths, sortByColumn, sortOrder, layoutType, null)
        {
        }

        public FeedColumnLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
                                SortOrder sortOrder, LayoutType layoutType, string arrangeByColumn)
        {
            if (columns != null)
                _columns = new List<string>(columns);
            else
                _columns = new List<string>();
            if (columnWidths != null)
                _columnWidths = new List<int>(columnWidths);
            else
                _columnWidths = new List<int>();

            _sortOrder = SortOrder.None;
            if (sortByColumn != null && _columns.IndexOf(sortByColumn) >= 0)
            {
                _sortByColumn = sortByColumn;
                _sortOrder = sortOrder;
            }
            if (arrangeByColumn != null && _columns.IndexOf(arrangeByColumn) >= 0)
            {
                _arrangeByColumn = arrangeByColumn;
            }
            _layoutType = layoutType;
        }

        public static FeedColumnLayout CreateFromXML(string xmlString)
        {
            if (xmlString != null && xmlString.Length > 0)
            {
                XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof (FeedColumnLayout));
                StringReader reader = new StringReader(xmlString);
                return (FeedColumnLayout) formatter.Deserialize(reader);
            }
            return null;
        }

        public static string SaveAsXML(FeedColumnLayout layout)
        {
            if (layout == null)
                return null;
            try
            {
                XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof (FeedColumnLayout));
                StringWriter writer = new StringWriter();
                formatter.Serialize(writer, layout);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SaveAsXML() failed.", ex.Message);
            }
            return null;
        }

        #region IFeedColumnLayout Members

        public LayoutType LayoutType
        {
            get
            {
                return _layoutType;
            }
            set
            {
                _layoutType = value;
            }
        }


        public string SortByColumn
        {
            get
            {
                return _sortByColumn;
            }
            set
            {
                _sortByColumn = value;
            }
        }

        public SortOrder SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
            }
        }

        public string ArrangeByColumn
        {
            get
            {
                return _arrangeByColumn;
            }
            set
            {
                _arrangeByColumn = value;
            }
        }

        [XmlIgnore]
        public IList<string> Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                if (value != null)
                    _columns = new List<string>(value);
                else
                    _columns = new List<string>();
            }
        }

        [XmlIgnore]
        public IList<int> ColumnWidths
        {
            get
            {
                return _columnWidths;
            }
            set
            {
                if (value != null)
                    _columnWidths = new List<int>(value);
                else
                    _columnWidths = new List<int>();
            }
        }

        #endregion

        [XmlArrayItem(typeof (string))]
        public List<string> ColumnList
        {
            get
            {
                return _columns;
            }
            set
            {
                if (value != null)
                    _columns = value;
                else
                    _columns = new List<string>();
            }
        }

        [XmlArrayItem(typeof (int))]
        public List<int> ColumnWidthList
        {
            get
            {
                return _columnWidths;
            }
            set
            {
                if (value != null)
                    _columnWidths = value;
                else
                    _columnWidths = new List<int>();
            }
        }

        /// <summary>
        /// Compares two layouts for equality. This method also compares the column widths 
        /// when determining equality. 
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if they are equal</returns>
        public override bool Equals(Object obj)
        {
            return Equals(obj, false);
        }

        /// <summary>
        /// Compares  two layouts for equality.
        /// </summary>
        /// <param name="obj">the objects to compare</param>
        /// <param name="ignoreColumnWidths">indicates whether column widths should be ignored</param>
        /// <returns>true if they are equal</returns>
        public bool Equals(object obj, bool ignoreColumnWidths)
        {
            if (obj == null)
                return false;
            FeedColumnLayout o = obj as FeedColumnLayout;
            if (o == null)
                return false;
            if (SortOrder != o.SortOrder)
                return false;
            if (SortByColumn != o.SortByColumn)
                return false;
            if (_columns == null && o._columns == null)
                return true;
            if (_columns == null || o._columns == null)
                return false;
            if (_columns.Count != o._columns.Count)
                return false;

            if (ignoreColumnWidths)
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (String.Compare(_columns[i], o._columns[i]) != 0)
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (String.Compare(_columns[i], o._columns[i]) != 0 ||
                        _columnWidths[i] != o._columnWidths[i])
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
                return Equals(layout, true);
            return false;
        }

        /// <summary>
        /// Returns true, if the layout is a kind of a category layout (global, individual) and
        /// they are equal except for column widhts.
        /// </summary>
        /// <param name="layout"></param>
        /// <returns>bool</returns>
        public bool IsSimilarCategoryLayout(FeedColumnLayout layout)
        {
            if (layout == null)
                return false;

            if ((_layoutType == LayoutType.IndividualLayout || _layoutType == LayoutType.GlobalCategoryLayout) &&
                (layout._layoutType == LayoutType.IndividualLayout || layout._layoutType == LayoutType.GlobalFeedLayout))
                return Equals(layout, true);
            return false;
        }

        public override int GetHashCode()
        {
            StringBuilder sb = new StringBuilder();
            if (_columns != null && _columns.Count > 0)
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    sb.AppendFormat("{0};", _columns[i]);
                }
            }
            if (_columnWidths != null && _columnWidths.Count > 0)
            {
                for (int i = 0; i < _columnWidths.Count; i++)
                {
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

        public object Clone()
        {
            return
                new FeedColumnLayout(_columns, _columnWidths, _sortByColumn, _sortOrder, _layoutType, _arrangeByColumn);
        }

        #endregion

        #region ISerializable Members

        protected FeedColumnLayout(SerializationInfo info, StreamingContext context)
        {
            //int version = info.GetInt32("version");
            _columns = (List<string>) info.GetValue("ColumnList", typeof (List<string>));
            _columnWidths = (List<int>) info.GetValue("ColumnWidthList", typeof (List<int>));
            _sortByColumn = info.GetString("SortByColumn");
            _sortOrder = (SortOrder) info.GetValue("SortOrder", typeof (SortOrder));
            _arrangeByColumn = info.GetString("ArrangeByColumn");
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);
            info.AddValue("ColumnList", _columns);
            info.AddValue("ColumnWidthList", _columnWidths);
            info.AddValue("SortByColumn", _sortByColumn);
            info.AddValue("SortOrder", _sortOrder);
            info.AddValue("ArrangeByColumn", _arrangeByColumn);
        }

        #endregion
    }


    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class category
    {
        /// <remarks/>
        [XmlAttribute("mark-items-read-on-exit")]
        public bool markitemsreadonexit;

        /// <remarks/>
        [XmlIgnore]
        public bool markitemsreadonexitSpecified;

        /// <remarks/>
        [XmlAttribute("download-enclosures")]
        public bool downloadenclosures;

        /// <remarks/>
        [XmlIgnore]
        public bool downloadenclosuresSpecified;

        /// <remarks/>
        [XmlAttribute("enclosure-folder")]
        public string enclosurefolder;

        ///<summary>ID to an FeedColumnLayout</summary>
        /// <remarks/>
        [XmlAttribute("listview-layout")]
        public string listviewlayout;

        /// <remarks/>
        [XmlAttribute]
        public string stylesheet;

        /// <remarks/>
        [XmlAttribute("refresh-rate")]
        public int refreshrate;

        /// <remarks/>
        [XmlIgnore]
        public bool refreshrateSpecified;

        /// <remarks/>
        [XmlAttribute("max-item-age", DataType="duration")]
        public string maxitemage;

        /// <remarks/>
        [XmlText]
        public string Value;

        /// <remarks/>
        [XmlIgnore]
        public category parent;

        /// <remarks/>
        [XmlAttribute("enclosure-alert"), DefaultValue(false)]
        public bool enclosurealert;

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurealertSpecified;

        /// <remarks/>
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttr;
    }

    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class NewsFeed
    {
        /// <remarks/>
        public string title;

        /// <remarks/>
        [XmlElement(DataType="anyURI")]
        public string link;

        private string _id;

        /// <remarks/>
        [XmlAttribute]
        public string id
        {
            get
            {
                if (_id == null || _id.Length == 0)
                    _id = Guid.NewGuid().ToString("N");
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <remarks/>
        [XmlElement("refresh-rate")]
        public int refreshrate;

        /// <remarks/>
        [XmlIgnore]
        public bool refreshrateSpecified;

        /// <remarks/>
        [XmlElement("last-retrieved")]
        public DateTime lastretrieved;

        /// <remarks/>
        [XmlIgnore]
        public bool lastretrievedSpecified;

        /// <remarks/>
        public string etag;

        /// <remarks/>
        [XmlElement(DataType="anyURI")]
        public string cacheurl;

        /// <remarks/>
        [XmlElement("max-item-age", DataType="duration")]
        public string maxitemage;


        /// <remarks/>
        [XmlArray(ElementName = "stories-recently-viewed", IsNullable = false)]
        [XmlArrayItem("story", Type = typeof (String), IsNullable = false)]
        public List<string> storiesrecentlyviewed = new List<string>();

        /// <remarks/>
        [XmlArray(ElementName = "deleted-stories", IsNullable = false)]
        [XmlArrayItem("story", Type = typeof (String), IsNullable = false)]
        public List<string> deletedstories = new List<string>();


        /// <remarks/>
        [XmlElement("if-modified-since")]
        public DateTime lastmodified;

        /// <remarks/>
        [XmlIgnore]
        public bool lastmodifiedSpecified;

        /// <remarks/>
        [XmlElement("auth-user")]
        public string authUser;

        /// <remarks/>
        [XmlElement("auth-password", DataType="base64Binary")]
        public Byte[] authPassword;

        /// <remarks/>
        [XmlElement("listview-layout")]
        public string listviewlayout;

        /// <remarks/>
        public string favicon;

        /// <remarks/>
        [XmlElement("download-enclosures")]
        public bool downloadenclosures;

        /// <remarks/>
        [XmlIgnore]
        public bool downloadenclosuresSpecified;

        /// <remarks/>
        [XmlElement("enclosure-folder")]
        public string enclosurefolder;

        /// <remarks/>
        [XmlAttribute("replace-items-on-refresh")]
        public bool replaceitemsonrefresh;

        /// <remarks/>
        [XmlIgnore]
        public bool replaceitemsonrefreshSpecified;

        /// <remarks/>
        public string stylesheet;

        /// <remarks>Reference the corresponding NntpServerDefinition</remarks>
        [XmlElement("news-account")]
        public string newsaccount;

        /// <remarks/>
        [XmlElement("mark-items-read-on-exit")]
        public bool markitemsreadonexit;

        /// <remarks/>
        [XmlIgnore]
        public bool markitemsreadonexitSpecified;

        /// <remarks/>
        [XmlAnyElement]
        public XmlElement[] Any;


        /// <remarks/>
        [XmlAttribute("alert"), DefaultValue(false)]
        public bool alertEnabled;

        /// <remarks/>
        [XmlIgnore]
        public bool alertEnabledSpecified;


        /// <remarks/>
        [XmlAttribute("enclosure-alert"), DefaultValue(false)]
        public bool enclosurealert;

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurealertSpecified;

        /// <remarks/>
        [XmlAttribute]
        public string category;

        /// <remarks/>
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttr;

        /// <remarks>True, if the feed caused an exception on request to prevent sequenced
        /// error reports on every automatic download</remarks>
        [XmlIgnore]
        public bool causedException
        {
            get
            {
                return causedExceptionCount != 0;
            }
            set
            {
                if (value)
                {
                    causedExceptionCount++; // raise counter
                    lastretrievedSpecified = true;
                    lastretrieved = new DateTime(DateTime.Now.Ticks);
                }
                else
                    causedExceptionCount = 0; // reset
            }
        }

        /// <remarks>Number of exceptions caused on requests</remarks>
        [XmlIgnore]
        public int causedExceptionCount = 0;

        /// <remarks>Can be used to store any attached data</remarks>
        [XmlIgnore]
        public object Tag;

        /// <remarks/>
        [XmlIgnore]
        public bool containsNewMessages;

        /// <remarks/>
        [XmlIgnore]
        public bool containsNewComments;

        /// <summary>
        /// Gets the value of a particular wildcard element. If the element is not found then 
        /// null is returned
        /// </summary>
        /// <param name="namespaceUri"></param>
        /// <param name="localName"></param>
        /// <returns>The value of the wildcard element obtained by calling XmlElement.InnerText
        /// or null if the element is not found. </returns>
        public string GetElementWildCardValue(string namespaceUri, string localName)
        {
            foreach (XmlElement element in Any)
            {
                if (element.LocalName == localName && element.NamespaceURI == namespaceUri)
                    return element.InnerText;
            }
            return null;
        }

        /// <summary>
        /// Tests to see if two NewsFeed objects represent the same feed. 
        /// </summary>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            NewsFeed feed = obj as NewsFeed;

            if (feed == null)
            {
                return false;
            }

            if (link.Equals(feed.link))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a hashcode for a NewsFeed object. 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return link.GetHashCode();
        }
    }

    #region UserIdentity

    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class UserIdentity : IUserIdentity, ICloneable
    {
        private string name;

        /// <remarks/>
        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string realName;

        /// <remarks/>
        [XmlElement("real-name")]
        public string RealName
        {
            get
            {
                return realName;
            }
            set
            {
                realName = value;
            }
        }

        private string organization;

        /// <remarks/>
        [XmlElement("organization")]
        public string Organization
        {
            get
            {
                return organization;
            }
            set
            {
                organization = value;
            }
        }

        private string mailAddress;

        /// <remarks/>
        [XmlElement("mail-address")]
        public string MailAddress
        {
            get
            {
                return mailAddress;
            }
            set
            {
                mailAddress = value;
            }
        }

        private string responseAddress;

        /// <remarks/>
        [XmlElement("response-address")]
        public string ResponseAddress
        {
            get
            {
                return responseAddress;
            }
            set
            {
                responseAddress = value;
            }
        }

        private string referrerUrl;

        /// <remarks/>
        [XmlElement("referrer-url")]
        public string ReferrerUrl
        {
            get
            {
                return referrerUrl;
            }
            set
            {
                referrerUrl = value;
            }
        }

        private string signature;

        /// <remarks/>
        [XmlElement("signature")]
        public string Signature
        {
            get
            {
                return signature;
            }
            set
            {
                signature = value;
            }
        }

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
    }

    #endregion

    #region NewsServerDefinition

    /// <remarks/>
    [XmlType(Namespace=NamespaceCore.Feeds_vCurrent)]
    public class NntpServerDefinition : INntpServerDefinition, ICloneable
    {
        private string name;

        /// <remarks/>
        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string defaultIdentity;

        /// <remarks/>
        [XmlElement("default-identity")]
        public string DefaultIdentity
        {
            get
            {
                return defaultIdentity;
            }
            set
            {
                defaultIdentity = value;
            }
        }

        private bool preventDownloadOnRefresh;

        /// <remarks/>
        [XmlElement("prevent-download")]
        public bool PreventDownloadOnRefresh
        {
            get
            {
                return preventDownloadOnRefresh;
            }
            set
            {
                preventDownloadOnRefresh = value;
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public bool PreventDownloadOnRefreshSpecified;

        private string server;

        /// <remarks/>
        [XmlElement("server-address")]
        public string Server
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
            }
        }

        private string authUser;

        /// <remarks/>
        [XmlElement("auth-user")]
        public string AuthUser
        {
            get
            {
                return authUser;
            }
            set
            {
                authUser = value;
            }
        }

        private Byte[] authPassword;

        /// <remarks/>
        [XmlElement("auth-password", DataType="base64Binary")]
        public Byte[] AuthPassword
        {
            get
            {
                return authPassword;
            }
            set
            {
                authPassword = value;
            }
        }

        private bool useSecurePasswordAuthentication;

        /// <remarks/>
        [XmlElement("auth-use-spa")]
        public bool UseSecurePasswordAuthentication
        {
            get
            {
                return useSecurePasswordAuthentication;
            }
            set
            {
                useSecurePasswordAuthentication = value;
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public bool UseSecurePasswordAuthenticationSpecified;

        private int port;

        /// <remarks/>
        [XmlElement("port-number")]
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public bool PortSpecified;

        private bool useSSL;

        /// <remarks>Makes the 'nntp:' a 'nntps:'</remarks>
        [XmlElement("use-ssl")]
        public bool UseSSL
        {
            get
            {
                return useSSL;
            }
            set
            {
                useSSL = value;
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public bool UseSSLSpecified;

        private int timeout;

        /// <remarks/>
        [XmlElement("timeout")]
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public bool TimeoutSpecified;

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
    }

    #endregion
}

#region CVS Version Log

/*
 * $Log: feeds.cs,v $
 * Revision 1.31  2006/12/16 23:15:51  carnage4life
 * Fixed issue where comment feeds get confused when a comment is deleted from the feed,
 *
 * Revision 1.30  2006/12/12 16:20:56  carnage4life
 * Fixed issue where Attachments/Podcasts option "Enable alert window for new downl. attachments" did not get persisted
 *
 * Revision 1.29  2006/12/09 22:57:03  carnage4life
 * Added support for specifying how many podcasts downloaded from new feeds
 *
 * Revision 1.28  2006/11/22 16:29:00  carnage4life
 * Fixed issue where ThreadAbortException throws a user facing error when loading a stylesheet
 *
 * Revision 1.27  2006/11/21 17:25:53  carnage4life
 * Made changes to support options for Podcasts
 *
 * Revision 1.26  2006/11/20 22:26:20  carnage4life
 * Added support for most of the Podcast and Attachment options except for podcast file extensions and copying podcasts to a specified folder
 *
 * Revision 1.25  2006/11/19 03:11:10  carnage4life
 * Added support for persisting podcast settings when changed in the Preferences dialog
 *
 * Revision 1.24  2006/10/28 23:10:00  carnage4life
 * Added "Attachments/Podcasts" to Feed Properties and Category properties dialogs.
 *
 * Revision 1.23  2006/10/05 15:46:29  t_rendelmann
 * rework: now using XmlSerializerCache everywhere to get the XmlSerializer instance
 *
 * Revision 1.22  2006/10/05 08:00:13  t_rendelmann
 * refactored: use string constants for our XML namespaces
 *
 * Revision 1.21  2006/08/18 19:10:57  t_rendelmann
 * added an "id" XML attribute to the NewsFeed. We need it to make the feed items (feeditem.id + feed.id) unique to enable progressive indexing (lucene)
 *
 */

#endregion