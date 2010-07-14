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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using NewsComponents;
using Ninject;
using RssBandit.Common.Logging;
using RssBandit.Core.Storage;
using RssBandit.Core.Storage.Serialization;
using SortOrder=System.Windows.Forms.SortOrder;

namespace RssBandit.WinGui
{

	/// <summary>
	/// Indicates whether a column layout is global, category-wide or feed specific.
	/// </summary>
	public enum LayoutType
	{
		IndividualLayout,
		GlobalFeedLayout,
		GlobalCategoryLayout,
		SearchFolderLayout,
		SpecialFeedsLayout,
	}

	#region ColumnLayoutManager class

	internal class ColumnLayoutManager
	{
		private static readonly log4net.ILog _log = Log.GetLogger(typeof(ColumnLayoutManager));

		private static readonly FeedColumnLayout DefaultFeedColumnLayout =
			new FeedColumnLayout(new[] { "Title", "Flag", "Enclosure", "Date", "Subject" },
								 new[] { 250, 22, 22, 100, 120 }, "Date", SortOrder.Descending,
								 LayoutType.GlobalFeedLayout);

		private static readonly FeedColumnLayout DefaultCategoryColumnLayout =
			new FeedColumnLayout(new[] { "Title", "Subject", "Date", "FeedTitle" }, new[] { 250, 120, 100, 100 },
								 "Date", SortOrder.Descending, LayoutType.GlobalCategoryLayout);

		private static readonly FeedColumnLayout DefaultSearchFolderColumnLayout =
			new FeedColumnLayout(new[] { "Title", "Subject", "Date", "FeedTitle" }, new[] { 250, 120, 100, 100 },
								 "Date", SortOrder.Descending, LayoutType.SearchFolderLayout);

		private static readonly FeedColumnLayout DefaultSpecialFolderColumnLayout =
			new FeedColumnLayout(new[] { "Title", "Subject", "Date", "FeedTitle" }, new[] { 250, 120, 100, 100 },
								 "Date", SortOrder.Descending, LayoutType.SpecialFeedsLayout);

		private static string defaultFeedColumnLayoutKey;
		private static string defaultCategoryColumnLayoutKey;
		private static string defaultSearchFolderColumnLayoutKey;
		private static string defaultSpecialFolderColumnLayoutKey;
		
		private FeedColumnLayoutCollection _layouts;


		/// <summary>
		/// Gets the column layouts.
		/// </summary>
		/// <value>The column layouts.</value>
		public FeedColumnLayoutCollection ColumnLayouts
		{
			get
			{
				if (_layouts == null)
				{
                    _layouts = LoadLayouts(RssBanditApplication.Current.Kernel.Get<IUserRoamingDataService>());
					ValidateAllColumnLayouts();
				}
				return _layouts;
			}
		}

		/// <summary>
		/// Gets or sets the global feed column layout.
		/// </summary>
		/// <value>The global feed column layout.</value>
		public FeedColumnLayout GlobalFeedColumnLayout
		{
			get { return ColumnLayouts[defaultFeedColumnLayoutKey];  }
			set
			{
				if (value == null) return;
				value.LayoutType = LayoutType.GlobalFeedLayout;

				if (!ColumnLayouts[defaultFeedColumnLayoutKey].Equals(value))
				{
					ColumnLayouts[defaultFeedColumnLayoutKey] = value;
					ColumnLayouts.RemoveSimilarLayouts(value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the global category column layout.
		/// </summary>
		/// <value>The global category column layout.</value>
		public FeedColumnLayout GlobalCategoryColumnLayout
		{
			get { return ColumnLayouts[defaultCategoryColumnLayoutKey]; }
			set
			{
				if (value == null) return;
				value.LayoutType = LayoutType.GlobalCategoryLayout;

				if (!ColumnLayouts[defaultCategoryColumnLayoutKey].Equals(value))
				{
					ColumnLayouts[defaultCategoryColumnLayoutKey] = value;
					ColumnLayouts.RemoveSimilarLayouts(value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the global search folder column layout.
		/// </summary>
		/// <value>The global search folder column layout.</value>
		public FeedColumnLayout GlobalSearchFolderColumnLayout
		{
			get { return ColumnLayouts[defaultSearchFolderColumnLayoutKey]; }
			set
			{
				if (value == null) return;
				value.LayoutType = LayoutType.SearchFolderLayout;

				if (!ColumnLayouts[defaultSearchFolderColumnLayoutKey].Equals(value))
				{
					ColumnLayouts[defaultSearchFolderColumnLayoutKey] = value;
					ColumnLayouts.RemoveSimilarLayouts(value);
				}	
			}
		}

		/// <summary>
		/// Gets or sets the global special folder column layout.
		/// </summary>
		/// <value>The global special folder column layout.</value>
		public FeedColumnLayout GlobalSpecialFolderColumnLayout
		{
			get { return ColumnLayouts[defaultSpecialFolderColumnLayoutKey]; }
			set
			{
				if (value == null) return;
				value.LayoutType = LayoutType.SpecialFeedsLayout;
				if (!ColumnLayouts[defaultSpecialFolderColumnLayoutKey].Equals(value))
				{
					ColumnLayouts[defaultSpecialFolderColumnLayoutKey] = value;
					ColumnLayouts.RemoveSimilarLayouts(value);
				}
			}
		}

		/// <summary>
		/// Saves the layouts of this instance.
		/// </summary>
		public void Save()
		{
			if (_layouts != null && _layouts.Modified)
                SaveLayouts(RssBanditApplication.Current.Kernel.Get<IUserRoamingDataService>(), _layouts);
		}

		/// <summary>
		/// Resets the layouts. They are re-loaded from storage on next request
		/// </summary>
		public void Reset()
		{
			_layouts = null;
		}

		private static FeedColumnLayoutCollection LoadLayouts(IClientDataService dataService)
		{
			if (dataService == null)
				throw new ArgumentNullException("dataService");
			try
			{
				return dataService.LoadColumnLayouts();
			} catch (Exception ex)
			{
				_log.Error("Could not load column layouts", ex);
				return new FeedColumnLayoutCollection();
			}
 		}

		private static void SaveLayouts(IClientDataService dataService, FeedColumnLayoutCollection layouts)
		{
			if (dataService == null)
				throw new ArgumentNullException("dataService");

			if (!layouts.Modified)
				return;
			
			try
			{
				dataService.SaveColumnLayouts(layouts);
				layouts.Modified = false;
			}
			catch (Exception ex)
			{
				_log.Error("Could not save column layouts", ex);
			}
		}

		private void ValidateAllColumnLayouts()
		{
			if (defaultFeedColumnLayoutKey == null)
			{
				defaultFeedColumnLayoutKey = ValidateFeedColumnLayout(
					LayoutType.GlobalFeedLayout, DefaultFeedColumnLayout);
			}
			if (defaultCategoryColumnLayoutKey == null)
			{
				defaultCategoryColumnLayoutKey = ValidateFeedColumnLayout(
					LayoutType.GlobalCategoryLayout, DefaultCategoryColumnLayout);
			}
			if (defaultSearchFolderColumnLayoutKey == null)
			{
				defaultSearchFolderColumnLayoutKey = ValidateFeedColumnLayout(
					LayoutType.SearchFolderLayout, DefaultSearchFolderColumnLayout);
			}
			if (defaultSpecialFolderColumnLayoutKey == null)
			{
				defaultSpecialFolderColumnLayoutKey = ValidateFeedColumnLayout(
					LayoutType.SpecialFeedsLayout, DefaultSpecialFolderColumnLayout);
			}

			Save();
		}

		private string ValidateFeedColumnLayout(LayoutType type, FeedColumnLayout defaultLayout)
		{
			foreach (var key in _layouts.Keys)
			{
				if (_layouts[key].LayoutType == type)
				{
					return key;
				}
			}

			string newkey = FeedColumnLayoutCollection.CreateNewKey();
			_layouts.Add(newkey, defaultLayout);
			// feedHandler.SaveColumLayouts();
			return newkey;
		}


	}

	#endregion

	#region FeedColumnLayoutCollection
	/// <summary>
	/// Implements a strongly typed collection of <see cref="FeedColumnLayout"/> key-and-value
	/// pairs that retain their insertion order and are accessible by index and by key.
	/// </summary>
	[Serializable]
	public class FeedColumnLayoutCollection : StatefullKeyItemCollection<string, FeedColumnLayout>
	{
		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class.
		/// </summary>
		public FeedColumnLayoutCollection()
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public FeedColumnLayoutCollection(int capacity)
			: base(capacity)
		{
		}

		#endregion

		#region Similar Layout methods
		/// <summary>
		/// Returns the first similar FeedColumnLayout key (ignoring the
		/// ColumnWidths).
		/// </summary>
		/// <param name="layout">FeedColumnLayout</param>
		/// <returns>key or null if not found</returns>
		public string KeyOfSimilar(FeedColumnLayout layout)
		{
			if (layout == null)
				return null;
			FeedColumnLayoutComparer comparer = Comparer;
			foreach (string key in Keys)
				if (!ReferenceEquals(this[key], layout) && comparer.Equals(this[key], layout))
					return key;
			return null;
		}

		static FeedColumnLayoutComparer _comparer;
		static FeedColumnLayoutComparer Comparer
		{
			get { 
				if (_comparer == null)
					_comparer = new FeedColumnLayoutComparer(true);
				return _comparer;
			}
		}

		/// <summary>
		/// Removes all similar layouts.
		/// </summary>
		/// <param name="layout">The layout.</param>
		public void RemoveSimilarLayouts(FeedColumnLayout layout)
		{
			if (layout == null)
				return;
			FeedColumnLayoutComparer comparer = Comparer;
			List<string> toRemove = new List<string>();
			foreach (string key in Keys)
				if (!ReferenceEquals(this[key], layout) && comparer.Equals(this[key], layout))
					toRemove.Add(key);

			for (int i = 0; i < toRemove.Count; i++)
			{
				string key = toRemove[0];
				Remove(key);
			}
		}
		#endregion

		/// <summary>
		/// Creates the new key.
		/// </summary>
		/// <returns></returns>
		public static string CreateNewKey()
		{
			return Guid.NewGuid().ToString("N");
		}

		class FeedColumnLayoutComparer : IEqualityComparer<FeedColumnLayout>
		{
			private readonly bool ignoreColumnWidths;

			public FeedColumnLayoutComparer(bool ignoreColumnWidths)
			{
				this.ignoreColumnWidths = ignoreColumnWidths;
			}

			#region Implementation of IEqualityComparer<FeedColumnLayout>

			public bool Equals(FeedColumnLayout x, FeedColumnLayout y)
			{
				if (x == null && y == null)
					return true;
				if (x == null || y == null)
					return false;

				if (x.SortOrder != y.SortOrder)
					return false;
				if (x.SortByColumn != y.SortByColumn)
					return false;
				if (x.Columns == null && y.Columns == null)
					return true;
				if (x.Columns == null || y.Columns == null)
					return false;
				if (x.Columns.Count != y.Columns.Count)
					return false;

				if (ignoreColumnWidths)
				{
					for (int i = 0; i < x.Columns.Count; i++)
					{
						if (String.Compare(x.Columns[i], y.Columns[i]) != 0)
							return false;
					}
				}
				else
				{
					for (int i = 0; i < x.Columns.Count; i++)
					{
						if (String.Compare(x.Columns[i], y.Columns[i]) != 0 ||
							x.ColumnWidths[i] != y.ColumnWidths[i])
							return false;
					}
				}

				return true;
			}

			public int GetHashCode(FeedColumnLayout obj)
			{
				StringBuilder sb = new StringBuilder();
				if (obj.Columns != null && obj.Columns.Count > 0)
				{
					for (int i = 0; i < obj.Columns.Count; i++)
					{
						sb.AppendFormat("{0};", obj.Columns[i]);
					}
				}
				if (obj.ColumnWidths != null && obj.ColumnWidths.Count > 0)
				{
					for (int i = 0; i < obj.ColumnWidths.Count; i++)
					{
						sb.AppendFormat("{0};", obj.ColumnWidths[i]);
					}
				}
				sb.AppendFormat("{0};", obj.SortByColumn);
				sb.AppendFormat("{0};", obj.SortOrder);
				sb.AppendFormat("{0};", obj.ArrangeByColumn);
				sb.AppendFormat("{0};", obj.LayoutType);

				return sb.ToString().GetHashCode();
			}

			#endregion
		}
	}
	#endregion

	#region IFeedColumnLayout

	/// <summary>
	/// Summary description for IFeedColumnLayout.
	/// </summary>
	public interface IFeedColumnLayout
	{
		/// <summary>
		/// Gets the sort by column.
		/// </summary>
		/// <value>The sort by column.</value>
		string SortByColumn { get; }
		/// <summary>
		/// Gets the sort order.
		/// </summary>
		/// <value>The sort order.</value>
		SortOrder SortOrder { get; }
		/// <summary>
		/// Gets the arrange by column.
		/// </summary>
		/// <value>The arrange by column.</value>
		string ArrangeByColumn { get; }
		/// <summary>
		/// Gets the columns.
		/// </summary>
		/// <value>The columns.</value>
		IList<string> Columns { get; }
		/// <summary>
		/// Gets the column widths.
		/// </summary>
		/// <value>The column widths.</value>
		IList<int> ColumnWidths { get; }
	}
	#endregion

	#region FeedColumnLayout

	/// <summary>
	/// A feed column layout container
	/// </summary>
	[Serializable]
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	public class FeedColumnLayout : IFeedColumnLayout, ICloneable, ISerializable
	{
		private string _sortByColumn;
		private SortOrder _sortOrder;
		private LayoutType _layoutType;
		private string _arrangeByColumn;
		internal List<string> _columns;
		internal List<int> _columnWidths;

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayout"/> class.
		/// </summary>
		public FeedColumnLayout() :
			this(null, null, null, SortOrder.None, LayoutType.IndividualLayout, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayout"/> class.
		/// </summary>
		/// <param name="columns">The columns.</param>
		/// <param name="columnWidths">The column widths.</param>
		/// <param name="sortByColumn">The sort by column.</param>
		/// <param name="sortOrder">The sort order.</param>
		/// <param name="layoutType">Type of the layout.</param>
		public FeedColumnLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
								SortOrder sortOrder, LayoutType layoutType) :
			this(columns, columnWidths, sortByColumn, sortOrder, layoutType, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayout"/> class.
		/// </summary>
		/// <param name="columns">The columns.</param>
		/// <param name="columnWidths">The column widths.</param>
		/// <param name="sortByColumn">The sort by column.</param>
		/// <param name="sortOrder">The sort order.</param>
		/// <param name="layoutType">Type of the layout.</param>
		/// <param name="arrangeByColumn">The arrange by column.</param>
		public FeedColumnLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
								SortOrder sortOrder, LayoutType layoutType, string arrangeByColumn)
		{
			_columns = columns != null ? new List<string>(columns) : new List<string>();
			_columnWidths = columnWidths != null ? new List<int>(columnWidths) : new List<int>();

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

		/// <summary>
		/// Creates from XML.
		/// </summary>
		/// <param name="xmlString">The XML string.</param>
		/// <returns></returns>
		public static FeedColumnLayout CreateFromXML(string xmlString)
		{
			if (!string.IsNullOrEmpty(xmlString))
			{
				XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof(FeedColumnLayout));
				StringReader reader = new StringReader(xmlString);
				return (FeedColumnLayout)formatter.Deserialize(reader);
			}
			return null;
		}

		/// <summary>
		/// Saves as XML.
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <returns></returns>
		public static string SaveAsXML(FeedColumnLayout layout)
		{
			if (layout == null)
				return null;
			try
			{
				XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof(FeedColumnLayout));
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

		/// <summary>
		/// Gets or sets the type of the layout.
		/// </summary>
		/// <value>The type of the layout.</value>
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


		/// <summary>
		/// Gets the sort by column.
		/// </summary>
		/// <value>The sort by column.</value>
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

		/// <summary>
		/// Gets the sort order.
		/// </summary>
		/// <value>The sort order.</value>
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

		/// <summary>
		/// Gets the arrange by column.
		/// </summary>
		/// <value>The arrange by column.</value>
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

		/// <summary>
		/// Gets the columns.
		/// </summary>
		/// <value>The columns.</value>
		[XmlIgnore]
		public IList<string> Columns
		{
			get
			{
				return _columns;
			}
			set {
				_columns = value != null ? new List<string>(value) : new List<string>();
			}
		}

		/// <summary>
		/// Gets the column widths.
		/// </summary>
		/// <value>The column widths.</value>
		[XmlIgnore]
		public IList<int> ColumnWidths
		{
			get
			{
				return _columnWidths;
			}
			set {
				_columnWidths = value != null ? new List<int>(value) : new List<int>();
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the column list.
		/// </summary>
		/// <value>The column list.</value>
		[XmlArrayItem(typeof(string))]
		public List<string> ColumnList
		{
			get
			{
				return _columns;
			}
			set {
				_columns = value ?? new List<string>();
			}
		}

		/// <summary>
		/// Gets or sets the column width list.
		/// </summary>
		/// <value>The column width list.</value>
		[XmlArrayItem(typeof(int))]
		public List<int> ColumnWidthList
		{
			get
			{
				return _columnWidths;
			}
			set {
				_columnWidths = value ?? new List<int>();
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
		/// they are equal except for column widths.
		/// </summary>
		/// <param name="layout"></param>
		/// <returns><c>bool</c></returns>
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
		/// they are equal except for column widths.
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

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
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

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return
				new FeedColumnLayout(_columns, _columnWidths, _sortByColumn, _sortOrder, _layoutType, _arrangeByColumn);
		}

		#endregion

		#region ISerializable Members

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayout"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		protected FeedColumnLayout(SerializationInfo info, StreamingContext context)
		{
			//int version = info.GetInt32("version");
			_columns = (List<string>)info.GetValue("ColumnList", typeof(List<string>));
			_columnWidths = (List<int>)info.GetValue("ColumnWidthList", typeof(List<int>));
			_sortByColumn = info.GetString("SortByColumn");
			_sortOrder = (SortOrder)info.GetValue("SortOrder", typeof(SortOrder));
			_arrangeByColumn = info.GetString("ArrangeByColumn");
		}


		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">
		/// The caller does not have the required permission.
		/// </exception>
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
	#endregion
}
