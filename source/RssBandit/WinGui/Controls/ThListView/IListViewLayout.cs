#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// Summary description for IListViewLayout.
	/// </summary>
	public interface IListViewLayout
	{
		string SortByColumn { get; }
		SortOrder SortOrder { get ; }
		IList Columns { get; }
		IList ColumnWidths { get; }
		bool Modified { get; }
	}

	[Serializable]
	public class ListViewLayout: IListViewLayout, ICloneable {
		private string _sortByColumn;
		private SortOrder _sortOrder;
		internal ArrayList _columns;
		internal ArrayList _columnWidths;
		private bool _modified;

		public ListViewLayout():this(null, null, null, SortOrder.None) {	}
		public ListViewLayout(ICollection columns, ICollection columnWidths, string sortByColumn, SortOrder sortOrder) {
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

		public static ListViewLayout CreateFromXML(string xmlString) {
			if (xmlString != null && xmlString.Length > 0) {
				XmlSerializer formatter = new XmlSerializer(typeof(ListViewLayout));
				StringReader reader = new StringReader(xmlString);
				return (ListViewLayout)formatter.Deserialize(reader);
			}
			return null;
		}

		public static string SaveAsXML(ListViewLayout layout) {
			if (layout == null)
				return null;
			try {
				XmlSerializer formatter = new XmlSerializer(typeof(ListViewLayout));
				StringWriter writer = new StringWriter();
				formatter.Serialize(writer, layout);
				return writer.ToString();
			} catch (Exception ex) {
				Trace.WriteLine("SaveAsXML() failed.", ex.Message);
			}
			return null;
		}

		#region IListViewLayout Members

		public string SortByColumn {
			get {	return _sortByColumn;	}
			set {	_sortByColumn = value;	}
		}

		public System.Windows.Forms.SortOrder SortOrder {
			get {	return _sortOrder; }
			set { _sortOrder = value; }
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

		[XmlIgnore]
		public bool Modified {
			get {	return _modified;	}
			set {	_modified = value;	}
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

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			ListViewLayout o = obj as ListViewLayout;
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
			for (int i = 0; i < this._columns.Count; i++) {
				if (String.Compare((string)this._columns[i], (string)o._columns[i]) != 0 || 
					(int)this._columnWidths[i] != (int)o._columnWidths[i])
					return false;
			}
			return true;
		}

		#region ICloneable Members

		public object Clone() {
			return new ListViewLayout(_columns, _columnWidths, _sortByColumn, _sortOrder);
		}

		#endregion
	}
}
