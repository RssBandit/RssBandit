#region CVS Version Header
/*
 * $Id: FeedColumnLayout.cs,v 1.2 2005/03/16 15:08:22 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/16 15:08:22 $
 * $Revision: 1.2 $
 */
#endregion

using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

using NewsComponents;

namespace NewsComponents.Feed {
	/// <summary>
	/// Summary description for IFeedColumnLayout.
	/// </summary>
	public interface IFeedColumnLayout
	{
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
		private string _arrangeByColumn;
		internal ArrayList _columns;
		internal ArrayList _columnWidths;

		public FeedColumnLayout():this(null, null, null, SortOrder.None) {	}
		public FeedColumnLayout(ICollection columns, ICollection columnWidths, string sortByColumn, SortOrder sortOrder) {
			if (columns != null)
				_columns = new ArrayList(columns);
			else
				_columns = new ArrayList();
			if (columnWidths != null)
				_columnWidths = new ArrayList(columnWidths);
			else
				_columnWidths = new ArrayList();
			_sortByColumn = sortByColumn;
			_sortOrder = sortOrder;
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

		#region ICloneable Members

		public object Clone() {
			return new FeedColumnLayout(_columns, _columnWidths, _sortByColumn, _sortOrder);
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
}
