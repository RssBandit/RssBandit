using System.Xml; 
using System.Xml.Serialization;

namespace NewsComponents.Feed
{
    /// <remarks/>
    public class opmlhead
    {

        private string titleField;

        /// <remarks/>
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    public class opmloutline
    {

        private opmloutline[] outlineField;

        private string titleField;

        private string textField;

        private string idField;

        private string xmlUrlField;

        private string htmlUrlField;

        private string syncXmlUrlField;

        private string folderIdField;

        private bool unseenField;

        private bool unseenFieldSpecified;

        private bool unreadField;

        private bool unreadFieldSpecified;

        private bool privateField;

        private bool privateFieldSpecified;

        private bool checkedByDefaultField;

        private bool checkedByDefaultFieldSpecified;

        private bool inStarterPackField;

        private bool inStarterPackFieldSpecified;

        private string starterPackOrderField;

        public opmloutline()
        {
            this.unseenField = true;
            this.unreadField = true;
            this.privateField = false;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("outline")]
        public opmloutline[] outline
        {
            get
            {
                return this.outlineField;
            }
            set
            {
                this.outlineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string xmlUrl
        {
            get
            {
                return this.xmlUrlField;
            }
            set
            {
                this.xmlUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string htmlUrl
        {
            get
            {
                return this.htmlUrlField;
            }
            set
            {
                this.htmlUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string syncXmlUrl
        {
            get
            {
                return this.syncXmlUrlField;
            }
            set
            {
                this.syncXmlUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        public string folderId
        {
            get
            {
                return this.folderIdField;
            }
            set
            {
                this.folderIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool unseen
        {
            get
            {
                return this.unseenField;
            }
            set
            {
                this.unseenField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unseenSpecified
        {
            get
            {
                return this.unseenFieldSpecified;
            }
            set
            {
                this.unseenFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool unread
        {
            get
            {
                return this.unreadField;
            }
            set
            {
                this.unreadField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unreadSpecified
        {
            get
            {
                return this.unreadFieldSpecified;
            }
            set
            {
                this.unreadFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool @private
        {
            get
            {
                return this.privateField;
            }
            set
            {
                this.privateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool privateSpecified
        {
            get
            {
                return this.privateFieldSpecified;
            }
            set
            {
                this.privateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        public bool checkedByDefault
        {
            get
            {
                return this.checkedByDefaultField;
            }
            set
            {
                this.checkedByDefaultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool checkedByDefaultSpecified
        {
            get
            {
                return this.checkedByDefaultFieldSpecified;
            }
            set
            {
                this.checkedByDefaultFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml")]
        public bool inStarterPack
        {
            get
            {
                return this.inStarterPackField;
            }
            set
            {
                this.inStarterPackField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool inStarterPackSpecified
        {
            get
            {
                return this.inStarterPackFieldSpecified;
            }
            set
            {
                this.inStarterPackFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://newsgator.com/schema/opml", DataType = "integer")]
        public string starterPackOrder
        {
            get
            {
                return this.starterPackOrderField;
            }
            set
            {
                this.starterPackOrderField = value;
            }
        }
    }

    /// <remarks/>
    public partial class opmlbody
    {

        private opmloutline[] outlineField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("outline")]
        public opmloutline[] outline
        {
            get
            {
                return this.outlineField;
            }
            set
            {
                this.outlineField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class opml
    {

        [System.Xml.Serialization.XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[]{ new XmlQualifiedName("ng","http://newsgator.com/schema/opml") } ) ;

        private opmlhead headField;

        private opmloutline[] bodyField;

        /// <remarks/>
        public opmlhead head
        {
            get
            {
                return this.headField;
            }
            set
            {
                this.headField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("outline", IsNullable = false)]
        public opmloutline[] body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }
}
