#region CVS Version Header
/*
 * $Id: WinGUIWidgetHelpers.cs,v 1.72 2005/05/08 17:03:22 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:22 $
 * $Revision: 1.72 $
 */
#endregion

#region usings
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml; 
using System.Xml.Serialization;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Net;
using System.Text;

using RssBandit.AppServices;
using RssBandit.WinGui.Interfaces;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Utility;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Utils;
using Genghis;
#endregion

namespace RssBandit.WinGui.Utility
{

	#region FontColorHelper
	/// <summary>
	/// Used item states that have different font/color settings
	/// </summary>
	public enum FontStates: int {
		Read = 0,
		Unread = 1,
		Flag = 2,
		Referrer = 3,
		Error = 4,
	}

	/// <summary>
	/// Maintain Font and Color infos used to highlight items in the UI
	/// </summary>
	public class FontColorHelper {
		// one default font, but different font styles/colors
		protected static Font	_defaultFont = new Font(Control.DefaultFont, Control.DefaultFont.Style);

		protected static FontStyle[]	_fontStyles = new FontStyle[5]{FontStyle.Regular,FontStyle.Italic, FontStyle.Bold, FontStyle.Regular, FontStyle.Regular};
		protected static Color []		_foreColors = new Color[5]{SystemColors.ControlText, SystemColors.ControlText, SystemColors.ControlText, Color.Blue, Color.Red};

		public static Font DefaultFont { get { return _defaultFont; } set { _defaultFont = value; } }

		public static Font NormalFont { get { return new Font(_defaultFont, NormalStyle); }  }
		public static Font HighlightFont { get { return new Font(_defaultFont, HighlightStyle); }  }
		public static Font UnreadFont { get { return new Font(_defaultFont, UnreadStyle); } }
		public static Font ReferenceFont { get { return new Font(_defaultFont, ReferenceStyle); }  }
		public static Font FailureFont { get { return new Font(_defaultFont, FailureStyle); }  }

		public static FontStyle NormalStyle {   get { return _fontStyles[0]; }    set { _fontStyles[0] = value;} 	}
		public static FontStyle HighlightStyle { get { return _fontStyles[1]; } set { _fontStyles[1] = value; }}
		public static FontStyle UnreadStyle {   get { return _fontStyles[2]; }   set { _fontStyles[2] = value; }}
		public static FontStyle ReferenceStyle {   get { return _fontStyles[3]; }    set { _fontStyles[3] = value;} }
		public static FontStyle FailureStyle {   get { return _fontStyles[4]; }    set { _fontStyles[4] = value;} }

		public static Color NormalColor {   get { return _foreColors[0]; }	set { _foreColors[0] = value;} }
		public static Color HighlightColor { get { return _foreColors[1]; }	set { _foreColors[1] = value; }}
		public static Color UnreadColor {   get { return _foreColors[2]; }	set { _foreColors[2] = value; }}
		public static Color ReferenceColor { get { return _foreColors[3]; }	set { _foreColors[3] = value;} }
		public static Color FailureColor { get { return _foreColors[4]; }	set { _foreColors[4] = value;} }

		/// <summary>
		/// Used to fix the font size issue with some UI widgets (e.g. TreeView)
		/// </summary>
		/// <returns></returns>
		public static Font FontWithMaxWidth() {
			int max = 0, index = 0;
			for (int i = 0; i < _fontStyles.Length; i++) {
				int m = (int) (_fontStyles[i] & ( FontStyle.Bold | FontStyle.Italic));
				if (m > max) {
					max = m; index = i;
				}
			}
			 return new Font(_defaultFont, _fontStyles[index]);
		}

		/// <summary>
		/// Return a new Font with the FontStyle of leadingFont and style merged
		/// if the styles are differ. The leadingFont else.
		/// </summary>
		/// <param name="leadingFont"></param>
		/// <param name="style"></param>
		/// <returns>Font</returns> 
		public static Font MergeFontStyles(Font leadingFont, FontStyle style) {
			if (leadingFont.Style == style)
				return leadingFont;
			return new Font(leadingFont, leadingFont.Style | style);
		}
	}

	#endregion

	#region UrlCompletionExtender

	public class UrlCompletionExtender {
		// used for Alt-Enter completion
		private string[] urlTemplates = new string[] {
			"http://www.{0}.com/",
			"http://www.{0}.net/",
			"http://www.{0}.org/",
			"http://www.{0}.info/",
		};
		private Form ownerForm;
		private IButtonControl ownerCancelButton;
		private int lastExpIndex = -1;
		private string toExpand = null;

		public UrlCompletionExtender(Form f) {
			if (f != null && f.CancelButton != null) {
				ownerForm = f;
				ownerCancelButton = f.CancelButton;
			}
		}
		
		public void Add(Control monitorControl) {
			this.Add(monitorControl, false);
		}		
		public void Add(Control monitorControl, bool includeFileCompletion) {
			if (monitorControl != null) {
				Utils.ApplyUrlCompletionToControl(monitorControl, includeFileCompletion);
				monitorControl.KeyDown += new KeyEventHandler(OnMonitorControlKeyDown);
				if (ownerForm != null && ownerCancelButton != null) {
					monitorControl.Enter += new EventHandler(OnMonitorControlEnter);
					monitorControl.Leave += new EventHandler(OnMonitorControlLeave);
				}
			}
		}

		private void ResetExpansion() {
			lastExpIndex = -1;
			toExpand = null;
		}

		private void RaiseExpansionIndex() {
			lastExpIndex = (++lastExpIndex % urlTemplates.Length);
		}

		private void OnMonitorControlKeyDown(object sender, KeyEventArgs e) {
			Control ctrl = sender as Control;
			if (ctrl == null) return;
			
			TextBox tb = sender as TextBox;
			ComboBox cb = sender as ComboBox;

			bool alt = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
			if (e.KeyCode == Keys.Return && alt) {
				if (lastExpIndex < 0 || toExpand == null) {
					string txt = ctrl.Text;
					if (txt.Length > 0 && txt.IndexOfAny(new char[]{':', '.', '/'}) < 0) {
						toExpand = txt;
						RaiseExpansionIndex();
					}
				}
				if (lastExpIndex >= 0 && toExpand != null) {
					ctrl.Text = String.Format(urlTemplates[lastExpIndex], toExpand);
					if (tb != null) 
						tb.SelectionStart = ctrl.Text.Length;
					if (cb != null && cb.DropDownStyle != ComboBoxStyle.DropDownList) 
						cb.SelectionStart = cb.Text.Length;
					RaiseExpansionIndex();
				}
			} else {
				ResetExpansion();
			}
		}

		private void OnMonitorControlLeave(object sender, EventArgs e) {
			ownerForm.CancelButton = ownerCancelButton;		// restore, if not yet done
		}

		private void OnMonitorControlEnter(object sender, EventArgs e) {
			ownerForm.CancelButton = null;	// drop
		}
	}
	#endregion

	#region CultureChanger

	/// <summary>
	/// Helper class to temporary switch the current thread culture.
	/// </summary>
	/// <example>
	/// <code>
	///		using (CultureChanger cc = new CultureChanger("en-US")) {
	///			// do things with an en-US culture
	///		}
	///		// go on with the previous thread culture
	/// </code>
	/// </example>
	public class CultureChanger: IDisposable {
		private CultureInfo _oldCulture;
		public CultureChanger(CultureInfo culture)
		{
			_oldCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = culture;
		}
		
		public CultureChanger(string culture):this(new CultureInfo(culture)) {}
		private CultureChanger(){}

		#region IDisposable Members

		public void Dispose() {
			Thread.CurrentThread.CurrentCulture = _oldCulture;
		}

		#endregion
	}
	#endregion

	#region UrlFormatter
	/// <summary>
	/// Supports Url encoded formatting of parameters, that can contain 
	/// encoding directives: {0:&lt;encoding&gt;}
	/// e.g. {0:euc-jp}
	/// </summary>
	public class UrlFormatter: IFormatProvider, ICustomFormatter 	
	{
		
		#region IFormatProvider Members

		public object GetFormat(Type formatType) {
			if (formatType == typeof (ICustomFormatter)) {
				return this;
			}
			else {
				return null;
			}
		}

		#endregion

		#region ICustomFormatter Members

		public string Format(string format, object arg, IFormatProvider formatProvider) {
			string s = arg as string;
			if (s == null) {
				return String.Empty;
			}
			if (format == null) {
				return String.Format("{0}", System.Web.HttpUtility.UrlEncode(s));
			}
			try {
				Encoding encoding = Encoding.GetEncoding(format);
				return String.Format("{0}", System.Web.HttpUtility.UrlEncode(s, encoding));
			}
			catch (NotSupportedException) {
				return String.Format("{0}", System.Web.HttpUtility.UrlEncode(s));
			}
		}

		#endregion
	}
	#endregion

	#region EventsHelper
	/// <summary>
	/// When publishing events in C#, you need to test that the delegate has targets. 
	/// You also must handle exceptions the subscribers throw, otherwise, the publishing 
	/// sequence is aborted. You can iterate over the delegate’s internal invocation list 
	/// and handle individual exceptions that way. The zip file contains a generic 
	/// helper class called EventsHelper that does just that. 
	/// EventsHelper can publish to any delegate, accepting any collection of parameters. 
	/// EventsHelper can also publish asynchronously and concurrently to the subscribers 
	/// using the thread pool, turning any subscriber’s target method into a fire-and-forget 
	/// method
	/// </summary>
	/// <remarks>Thanks to http://www.idesign.net/ </remarks>
	public class EventsHelper {
		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(EventsHelper));

		public static void Fire(Delegate del,params object[] args) {
			Delegate temp = del;
			if(temp == null) {
				return;
			}
			Delegate[] delegates = temp.GetInvocationList();
			foreach(Delegate sink in delegates) {
				try {
					sink.DynamicInvoke(args);
				}
				catch (Exception sinkEx) {
					_log.Error(String.Format("Calling '{0}.{1}' caused an exception." , temp.Method.DeclaringType.FullName, temp.Method.Name ), sinkEx);
				} 
			}
		}

		public static void FireAsync(Delegate del,params object[] args) {
			Delegate temp = del;
			if(temp == null) {
				return;
			}
			Delegate[] delegates = del.GetInvocationList();
			AsyncFire asyncFire;
			foreach(Delegate sink in delegates) {
				asyncFire = new AsyncFire(InvokeDelegate);
				asyncFire.BeginInvoke(sink,args,null,null);
			}
		}

		delegate void AsyncFire(Delegate del,object[] args);
		[OneWay]
		static void InvokeDelegate(Delegate del,object[] args) {
			del.DynamicInvoke(args);
		}

	}

	#endregion

	#region Settings
	/// <summary>
	/// Helper to save/restore Gui Settings (other than User Preferences).
	/// This includes such things like Window size and position, panel sizes, dock layout etc.
	/// </summary>
	public class Settings: Preferences {
		
		static System.Collections.Specialized.StringDictionary userStore;
		static bool userStoreModified;
		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(Settings));

		public Settings(string path):base(path) {


			if (userStore == null) {
				userStore = new System.Collections.Specialized.StringDictionary();
                
				// Load preferences.
				Deserialize();
				userStoreModified = false;
			}
		}

		public override object GetProperty(string name, object defaultValue, Type returnType) {
			string value = userStore[Path + name];
			if (value == null)
				return defaultValue;
			try {
				return Convert.ChangeType(value, returnType);
			}
			catch (Exception e) {
				_log.Debug("Settings: The property " + name + " could not be converted to the intended type (" + returnType + ").  Using defaults.");
				_log.Debug("Settings: The exception was:", e);
				return defaultValue;
			}
		}

		public override void SetProperty(string name, object value) {
			if (value == null && userStore.ContainsKey(Path + name)) {
				userStore.Remove(Path + name);
				return;
			}
			userStore[Path + name] = Convert.ToString(value);
			userStoreModified = true;
		}
        
		/// <summary>
		/// Flushes any outstanding properties to disk.</summary>
		public override void Flush() {
			Serialize();
		}
        
		/// <summary>Close resources and flush content (if needed) </summary>
		public new void Close() {
			base.Close();
			Flush();
		}

		private static FileStream CreateSettingsStream() {
			return FileHelper.OpenForWrite(System.IO.Path.Combine(RssBanditApplication.GetUserPath(), ".settings.xml"));
		}

		/// <summary>
		/// Opens a read-only stream on the backing store.</summary>
		/// <returns>
		/// A stream to read from.</returns>
		private static FileStream OpenSettingsStream() {
			return FileHelper.OpenForRead(System.IO.Path.Combine(RssBanditApplication.GetUserPath(), ".settings.xml"));
		}
        
		/// <summary>Deserializes to the userStore hashtable from an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private static void Deserialize() {
			XmlTextReader reader = null;
			try {
				reader = new XmlTextReader( OpenSettingsStream() );
                
				// Read name/value pairs.
				while (reader.Read()) {
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "property") {
						string name = reader.GetAttribute("name");
						string value = reader.ReadString();
						userStore[name] = value;
					}
				}
                
				reader.Close();
			}
			catch (Exception e) {
				// Release all resources held by the XmlTextReader.
				if (reader != null)
					reader.Close();
                
				// Report exception.
				_log.Debug("Settings: There was an error while deserializing from Settings Storage.  Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		/// <summary>Serializes the userStore hashtable to an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private static void Serialize() {
			if (userStoreModified == false)
				return;
                
			XmlTextWriter writer = null;
			try {
				writer = new XmlTextWriter(CreateSettingsStream(), null/* Encoding.Unicode */);
				
				// Write stream to console.
				//XmlTextWriter writer = new XmlTextWriter(Console.Out);

				// Use indentation for readability.
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

				writer.WriteStartDocument(true);
				writer.WriteStartElement("settings");

				// Write properties.
				foreach (System.Collections.DictionaryEntry entry in userStore) {
					writer.WriteStartElement("property");
					writer.WriteAttributeString("name", (string) entry.Key);
					writer.WriteString((string) entry.Value);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();

				// No longer modified compared to the copy on disk.
				userStoreModified = false;
			}
			catch (Exception e) {
				// Release all resources held by the XmlTextWriter.
				if (writer != null)
					writer.Close();
                
				// Report exception.
				_log.Debug("Settings: There was an error while serializing to Storage.  Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}
	}//end Settings class
	#endregion

	#region WebTabState
	internal class WebTabState: ITabState
	{
		private string _title;
		private string _currentUrl;
		private bool _canGoBack;
		private bool _canGoForward;

		public WebTabState()
		{
			_title        = String.Empty;
			_currentUrl   = String.Empty;
			_canGoBack    = false;
			_canGoForward = false;
		}
		public WebTabState(string title, string currentUrl):this()
		{
			_title        = title;
			_currentUrl   = currentUrl;
		}

		#region Implementation of ITabState
		public bool CanClose
		{
			get { return true; }
			set {}
		}

		public bool CanGoBack
		{
			get { return _canGoBack;  }
			set { _canGoBack = value; }
		}

		public bool CanGoForward
		{
			get { return _canGoForward;  }
			set { _canGoForward = value; }
		}

		public string Title
		{
			get	{ return _title; }
			set {_title = value; }
		}
		public string Url
		{
			get	{ return _currentUrl;  }
			set { _currentUrl = value; }
		}

		#endregion

	}
	#endregion

	#region TreeNodesSortHelper
	internal class TreeNodesSortHelper: IComparer {
		private System.Windows.Forms.SortOrder _sortOrder;

		public TreeNodesSortHelper():this(System.Windows.Forms.SortOrder.Ascending) {}
		public TreeNodesSortHelper(System.Windows.Forms.SortOrder sortOrder) {
			_sortOrder = sortOrder;
		}

		public void InitFromConfig(string section, Settings reader) {
			_sortOrder = (System.Windows.Forms.SortOrder)reader.GetInt32(section+"/SubscribedFeedNodes.Sorter.SortOrder", (int)System.Windows.Forms.SortOrder.Descending);
		}

		public void SaveToConfig(string section, Settings writer) {
			writer.SetProperty(section+"/SubscribedFeedNodes.Sorter.SortOrder", (int)this._sortOrder);
		}

		public System.Windows.Forms.SortOrder Sorting {
			get { return _sortOrder;  }
			set { _sortOrder = value; }
		}

		#region Implementation of IComparer
		public virtual int Compare(object x, object y) {
			
			if (_sortOrder == System.Windows.Forms.SortOrder.None)
				return 0;

			if (Object.ReferenceEquals(x, y))
				return 0;
			if (x == null) 
				return -1;
			if (y == null) 
				return 1;

			FeedTreeNodeBase n1 = (FeedTreeNodeBase)x;
			FeedTreeNodeBase n2 = (FeedTreeNodeBase)y;

			// root entries are not sorted: they stay there where they are inserted on creation:
			if (n1.Type == FeedNodeType.Root && n2.Type == FeedNodeType.Root)
				return 0;

			if (n1.Type == n2.Type)
				return (_sortOrder == System.Windows.Forms.SortOrder.Ascending ? String.Compare(n1.Key, n2.Key) : String.Compare(n2.Key, n1.Key));

			if (n1.Type == FeedNodeType.Category)
				return -1;

			if (n2.Type == FeedNodeType.Category)
				return 1;

			if (n1.Type == FeedNodeType.FinderCategory)
				return -1;

			if (n2.Type == FeedNodeType.FinderCategory)
				return 1;
			
			return 0;
		}

		#endregion

	}
	#endregion

	#region TreeHelper
	internal class TreeHelper {
		
		public static void CopyNodes(TreeNode[] nodes, TreeView destinationTree) {
			CopyNodes(nodes, destinationTree, false);			
		}		
		public static void CopyNodes(TreeNode[] nodes, TreeView destinationTree, bool isChecked) {
			if (nodes == null) return;
			if (destinationTree == null) return;

			destinationTree.BeginUpdate();
			destinationTree.Nodes.Clear();
			foreach (TreeNode n in nodes) {
				int i = destinationTree.Nodes.Add((TreeNode)n.Clone());
				destinationTree.Nodes[i].Tag = n.Tag;
				destinationTree.Nodes[i].Checked = isChecked;
				if (n.Nodes.Count > 0)
					CopyNodes(n.Nodes, destinationTree.Nodes[i], isChecked);
			}
			destinationTree.EndUpdate();
		}

		public static void CopyNodes(TreeNode node, TreeView destinationTree) {
			CopyNodes(node, destinationTree, false);
		}
		public static void CopyNodes(TreeNode node, TreeView destinationTree, bool isChecked) {
			if (node == null) return;
			if (destinationTree == null) return;

			destinationTree.BeginUpdate();
			destinationTree.Nodes.Clear();
			int i = destinationTree.Nodes.Add((TreeNode)node.Clone());
			destinationTree.Nodes[i].Tag = node.Tag;
			destinationTree.Nodes[i].Checked = isChecked;
			if (node.Nodes.Count > 0)
				CopyNodes(node.Nodes, destinationTree.Nodes[i], isChecked);
			destinationTree.EndUpdate();
		}

		private static void CopyNodes(ICollection nodes, TreeNode parent, bool isChecked) {
			foreach (TreeNode n in nodes) {
				int i = parent.Nodes.Add((TreeNode)n.Clone());
				parent.Nodes[i].Tag = n.Tag;
				parent.Nodes[i].Checked= isChecked;
				if (n.Nodes.Count > 0)
					CopyNodes(n.Nodes, parent.Nodes[i], isChecked);
			}

		}

		/// <summary>
		/// We assume: leave nodes are nodes with a Tag != null.
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="folders"></param>
		/// <param name="leaveNodes"></param>
		public static void GetCheckedNodes(TreeNode startNode, ArrayList folders, ArrayList leaveNodes) {
			if (startNode == null) 
				return;
			
			if (startNode.Checked) {

				if (startNode.Tag == null) {
					folders.Add(startNode);
					return;	// all childs are checked. They will be resolved later dynamically
				} else
					leaveNodes.Add(startNode);
			}

			foreach (TreeNode n in startNode.Nodes) {
				GetCheckedNodes(n, folders, leaveNodes);
			}
		}

		/// <summary>
		/// Fills the checkedNodes list with all nodes that are checked.
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="checkedNodes">List will be filled with nodes that are checked</param>
		public static void GetCheckedNodes(TreeNode startNode, ArrayList checkedNodes) {
			if (startNode == null) 
				return;
			
			if (startNode.Checked) {
				if (startNode.Tag != null) 
					checkedNodes.Add(startNode);
			}

			if (startNode.Nodes.Count > 0) {
				foreach (TreeNode n in startNode.Nodes) {
					GetCheckedNodes(n, checkedNodes);
				}
			}
		}

		/// <summary>
		/// We assume: leave nodes are nodes with a Tag != null.
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="folders"></param>
		/// <param name="leaveNodeTags"></param>
		/// <param name="separator"></param>
		public static void SetCheckedNodes(TreeNode startNode, ArrayList folders, ArrayList leaveNodeTags, char[] separator) {
			if (startNode == null) 
				return;

			if (startNode.Tag == null) {
				
				string catName = BuildCategoryStoreName(startNode, separator);
				if (catName != null && folders != null) {
					foreach (string folder in folders) {
						if (catName.Equals(folder)) {
							startNode.Checked = true;
							PerformOnCheckStateChanged(startNode);
							break;
						}
					}
				}
			
			} else {
				
				if (leaveNodeTags != null) {
					foreach (string tagString in leaveNodeTags) {
						if (tagString != null && tagString.Equals(startNode.Tag)) {
							startNode.Checked = true;
							PerformOnCheckStateChanged(startNode);
							break;
						}
					}
				}

			}
			
			foreach (TreeNode n in startNode.Nodes) {
				SetCheckedNodes(n, folders, leaveNodeTags, separator);
			}
		}

		/// <summary>
		/// Helper that builds the full path trimmed category name (without root caption)
		/// </summary>
		/// <param name="theNode">a TreeNode</param>
		/// <param name="separator">char[]</param>
		/// <returns>Category name in this form: 
		/// 'Main Category\Sub Category\...\catNode Category' without the root node caption.
		/// </returns>
		public static string BuildCategoryStoreName(TreeNode theNode, char[] separator) {
			string s = theNode.FullPath.Trim();
			string[] a = s.Split(separator);

			if (a.GetLength(0) > 1)
				return String.Join(@"\",a, 1,a.GetLength(0)-1);
			
			return null;	
		}

		public static void PerformOnCheckStateChanged(TreeNode node) {
			if (node != null) {
				if (node.Nodes.Count > 0) {
					TreeHelper.CheckChildNodes(node, node.Checked);
				}
				TreeHelper.CheckParentNodes(node, node.Checked);
			}
		}

		public static void CheckChildNodes(TreeNode parent, bool isChecked) {
			foreach (TreeNode child in parent.Nodes) {
				child.Checked = isChecked;
				if (child.Nodes.Count > 0)
					CheckChildNodes(child, isChecked);
			}
		}
		public static void CheckParentNodes(TreeNode node, bool isChecked) {
			if (node == null) return;
			if (node.Parent == null) return;
			foreach (TreeNode child in node.Parent.Nodes) {
				if (child.Checked != isChecked) {
					node.Parent.Checked = false;
					CheckParentNodes(node.Parent, isChecked);
					return;	// not all childs have the same state
				}
			}
			node.Parent.Checked = isChecked;
			CheckParentNodes(node.Parent, isChecked);
		}

	}
	#endregion

	#region moved code/just for ref.
//	internal class ListViewSortHelper: IComparer
//	{
//		private int _columnIndex;
//		private SortOrder _sortOrder;
//		private bool _useDateCompare;
//
//		public ListViewSortHelper():this(0, SortOrder.Ascending, false) {}
//		public ListViewSortHelper(int sortColumnIndex, SortOrder sortOrder, bool isDate)
//		{
//			_columnIndex = sortColumnIndex;
//			_sortOrder = sortOrder;
//			_useDateCompare = isDate;
//		}
//
//		public IComparer NewsItemComparer() {
////			if (_useDateCompare) {
////				return RssHelper.GetComparer(_sortOrder == SortOrder.Descending);
////			}
//			return RssHelper.GetComparer();
//		}
//
//		public void InitFromConfig(string section, Settings reader) {
//			_sortOrder = (SortOrder)reader.GetInt32(section+"/NewsItemListview.Sorter.SortOrder", (int)SortOrder.Descending);
//			_useDateCompare = reader.GetBoolean(section+"/NewsItemListview.Sorter.UseDateCompare", false);
//			_columnIndex = reader.GetInt32(section+"/NewsItemListview.Sorter.ColumnIndex", 0);
//		}
//
//		public void SaveToConfig(string section, Settings writer) {
//			writer.SetProperty(section+"/NewsItemListview.Sorter.SortOrder", (int)this._sortOrder);
//			writer.SetProperty(section+"/NewsItemListview.Sorter.UseDateCompare", _useDateCompare);
//			writer.SetProperty(section+"/NewsItemListview.Sorter.ColumnIndex", this._columnIndex);
//		}
//
//		public int ColumnIndex
//		{
//			get { return _columnIndex;  }
//			set { _columnIndex = value; }
//		}
//		public SortOrder Sorting
//		{
//			get { return _sortOrder;  }
//			set { _sortOrder = value; }
//		}
//
//		public bool DateCompare
//		{
//			get { return _useDateCompare;  }
//			set { _useDateCompare = value; }
//		}
//
//		public FlaggedListViewSortHelper FlaggedItemsSorter {
//			get { 
//				return new FlaggedListViewSortHelper(this._columnIndex, this._sortOrder, this._useDateCompare);
//			}
//		}
//
//		#region Graphics
//
//		Bitmap upBM, downBM;    // the 2 bitmaps used for drawing the triangles
//		private Bitmap GetBitmap(bool ascending) {
//			Bitmap bm = new Bitmap(8, 8);
//			Graphics gfx = Graphics.FromImage(bm);
//
//			Pen lightPen = SystemPens.ControlLightLight;
//			Pen shadowPen = SystemPens.ControlDark;
//
//			gfx.FillRectangle(SystemBrushes.ControlLight, 0, 0, 8, 8);
//
//			if (ascending) {
//				// Draw triangle pointing upwards
//				gfx.DrawLine(lightPen, 0, 7, 7, 7);
//				gfx.DrawLine(lightPen, 7, 7, 4, 0);
//				gfx.DrawLine(shadowPen, 3, 0, 0, 7);
//			}
//			else {
//				gfx.DrawLine(lightPen, 4, 7, 7, 0);
//				gfx.DrawLine(shadowPen, 3, 7, 0, 0);
//				gfx.DrawLine(shadowPen, 0, 0, 7, 0);
//			}
//
//			gfx.Dispose();
//
//			return bm;
//		}
//
//		/// <summary>
//		/// RefreshSortMarks uses LVM_GETHEADER, HDM_GEITEM and HDM_SETITEM to manipulate the header control
//		/// of the underlying listview control.
//		/// </summary>
//		/// <param name="listView"></param>
//		public void RefreshSortMarks(ListView listView) {
//			int LVM_GETHEADER = 0x1000 + 31;
//			int HDM_GETITEM = 0x1200 + 11;
//			int HDM_SETITEM = 0x1200 + 12;
//			int HDI_FORMAT =  0x0004;
//			int HDI_BITMAP =  0x0010;
//			int HDI_TEXT = 0x0002;
//            
//			IntPtr hHeader = Win32.SendMessage3(listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
//            
//			if (!hHeader.Equals(IntPtr.Zero)) {
//				if (upBM == null) {
//					upBM = GetBitmap(true);
//					downBM = GetBitmap(false); 
//				}
//
//				Win32.HDITEM item = new Win32.HDITEM();
//				item.mask = HDI_FORMAT | HDI_BITMAP | HDI_TEXT;
//				item.cchTextMax = 255;
//
//				for (int i = 0; i < listView.Columns.Count; i++) {
//					IntPtr result = Win32.SendMessage2(hHeader, HDM_GETITEM, new IntPtr(i), item);
//					if (result.ToInt32() > 0) {
//						//item.pszText = listView.Columns[i].Text;
//						item.pszText = Marshal.StringToHGlobalAuto(listView.Columns[i].Text);
//						item.mask = HDI_FORMAT | HDI_TEXT | HDI_BITMAP;
//						item.hbm = IntPtr.Zero;
//                        
//						if (i == _columnIndex) {
//							item.fmt |= 0x7000;
//							if (_sortOrder == SortOrder.Ascending)
//								item.hbm = upBM.GetHbitmap();
//							else
//								item.hbm = downBM.GetHbitmap();
//						}
//						else {
//							item.fmt |= 0x4000;
//						}
//						Win32.SendMessage2(hHeader, HDM_SETITEM, new IntPtr(i), item);
//					}
//				}
//			}
//		}
//
//		#endregion
//
//		#region Implementation of IComparer
//		public virtual int Compare(object x, object y)
//		{
//			ThreadedListViewItem row1, row2;
//
//			row1 = (ThreadedListViewItem)x;
//			row2 = (ThreadedListViewItem)y;
//
//			// immediatly called after adding subitems, so we test:
//			if (row1.SubItems.Count <= _columnIndex || row2.SubItems.Count <= _columnIndex)
//				return 0;
//
//			if (row1.IndentLevel != row2.IndentLevel)
//				return 0;
//
//			if (row1.IndentLevel > 0 || row2.IndentLevel > 0)
//				return 0;
//
//			int returnValue = 0;
//
//			if (_useDateCompare)	{
//				if (row1.Key != null && row2.Key != null) {
//					DateTime d1 = ((NewsItem)row1.Key).Date;
//					DateTime d2 = ((NewsItem)row2.Key).Date;
//					returnValue = (_sortOrder == SortOrder.Ascending ? DateTime.Compare(d1, d2): DateTime.Compare(d2, d1));
//				}
//			}
//			else {
//				string s1 = row1.SubItems[_columnIndex].Text;
//				string s2 = row2.SubItems[_columnIndex].Text;
//				returnValue = (_sortOrder == SortOrder.Ascending ? String.Compare(s1, s2) : String.Compare(s2, s1));
//			}
//
//			// bubble up: unread first (ignore sortOrder)
//			if (returnValue == 0 && row1.Key != null && row2.Key != null) {		// equal, compare if they are also read
//				if (((NewsItem)row1.Key).BeenRead != ((NewsItem)row2.Key).BeenRead)
//					returnValue = (!((NewsItem)row1.Key).BeenRead ? -1: 1);
//			}
//			
//			return returnValue;
//		}
//
//		#endregion
//
//	}
//
//	internal class FlaggedListViewSortHelper: ListViewSortHelper, IComparer {
//		public FlaggedListViewSortHelper():base(0,SortOrder.Ascending, false) {}
//		public FlaggedListViewSortHelper(int sortColumnIndex, SortOrder sortOrder, bool isDate):base(sortColumnIndex,sortOrder,isDate) {}
//
//		#region IComparer Members
//
//		public override int Compare(object x, object y) {
//			// we asume, the view is grouped by the flags
//			ThreadedListViewItem row1 = (ThreadedListViewItem)x;
//			ThreadedListViewItem row2 = (ThreadedListViewItem)y;
//			NewsItem item1 = (NewsItem)row1.Key;
//			NewsItem item2 = (NewsItem)row2.Key;
//
//			// immediatly called after adding subitems, so we test:
//			if (row1.SubItems.Count <= base.ColumnIndex || row2.SubItems.Count <= base.ColumnIndex)
//				return 0;
//
//			if (row1.IndentLevel != row2.IndentLevel)
//				return 0;
//
//			if (item1 == null || item2 == null)
//				return base.Compare(x,y);
//
//			if (item1.FlagStatus == item2.FlagStatus)
//				return base.Compare(x,y);
//
//			if (base.Sorting == SortOrder.Ascending) {
//				if ((int)item1.FlagStatus < (int)item2.FlagStatus)
//					return -1;
//				if ((int)item1.FlagStatus > (int)item2.FlagStatus)
//					return 1;
//			} else {
//				if ((int)item1.FlagStatus < (int)item2.FlagStatus)
//					return 1;
//				if ((int)item1.FlagStatus > (int)item2.FlagStatus)
//					return -1;
//			}
//
//			return 0;
//
//		}
//
//		#endregion
//	}

//	// internet connection states
//	[Flags]public enum INetState {
//		Invalid = 0,
//		DisConnected = 1,
//		Connected = 2,
//		Offline = 4, 
//		Online = 8
//	}
	#endregion

	#region Utils
	internal class Utils {

		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(Utils));

		// some probe Urls, used by CurrentINetState() (no, that is NOT my favourites list... ;-)
		// They have better ping timings than all the other....
		private static string[] probeUrls = new string[]{
															"http://www.w3c.org/",	"http://www.google.com/",
															"http://www.heise.de/", "http://www.nyi.com/",
															"http://www.olm.net"
														};

		[DllImport("wininet.dll", SetLastError=true)]
		private extern static bool InternetGetConnectedState(out int flags, int reserved);

		[DllImport("wininet.dll", SetLastError=true)]
		private extern static bool InternetCheckConnection(string url, int flags, int reserved);
		// the only possible flag for InternetCheckConnection()
		private const int FLAG_ICC_FORCE_CONNECTION = 0x01;

		[DllImport("wininet.dll", SetLastError=true)]
		private static extern bool InternetSetOption(IntPtr hInternet, uint option, IntPtr buffer,	int bufferLength);
		[DllImport("wininet.dll", SetLastError=true)]
		private static extern bool InternetSetOption(IntPtr hInternet, uint option, ref INTERNET_CONNECTED_INFO buffer,	int bufferLength);

		[StructLayout(LayoutKind.Sequential)] 
			private struct INTERNET_CONNECTED_INFO {
			public uint dwConnectedState,
				dwFlags;
		}

		// Internet options
		private const uint INTERNET_OPTION_SETTINGS_CHANGED = 39;
		private const uint INTERNET_STATE_CONNECTED = 0x00000001;
		private const uint INTERNET_STATE_DISCONNECTED_BY_USER = 0x00000010;
		private const uint ISO_FORCE_DISCONNECTED = 0x00000001;
		private const uint INTERNET_OPTION_CONNECTED_STATE = 50;

		// Flags for InternetGetConnectedState and Ex
		[Flags]private enum InternetStates {
			INTERNET_CONNECTION_MODEM  =         0x01,
			INTERNET_CONNECTION_LAN    =         0x02,
			INTERNET_CONNECTION_PROXY  =         0x04,
			INTERNET_CONNECTION_MODEM_BUSY =     0x08,  /* no longer used */
			INTERNET_RAS_INSTALLED       =       0x10,
			INTERNET_CONNECTION_OFFLINE  =       0x20,
			INTERNET_CONNECTION_CONFIGURED =     0x40
		}

		[Flags]private enum NetworkAliveFlags {
			NETWORK_ALIVE_LAN = 0x1,		// net card connection
			NETWORK_ALIVE_WAN = 0x2,		// RAS connection
			NETWORK_ALIVE_AOL = 0x4		// AOL
		}

		[DllImport("sensapi.dll", SetLastError=true)]
		private static extern bool IsNetworkAlive(ref int flags);

		/// <summary>
		/// Used to count internally to decide when we should make a forced INetState test
		/// </summary>
		private static int fullInternetStateTestCounter = 0;

		/// <summary>
		/// Figures out, if we are connected to the Internet.
		/// First it try to use the SENSAPI to do the work (see also 
		/// http://msdn.microsoft.com/msdnmag/issues/02/08/SENS/default.aspx).
		/// As this article describes, it does not make sense to use the SENS TCP/IP 
		/// notifications. So we test by a timer calling this function again and again.
		/// If SENSAPI fails, we fall back to the impl. based on a KB article: Q242558
		/// http://support.microsoft.com/default.aspx?scid=kb;en-us;242558
		/// </summary>
		/// <param name="currentProxy">The current proxy to be used.</param>
		/// <param name="forceFullTest">true to enforce a full connection state test</param>
		/// <returns>INetState</returns>
		public static INetState CurrentINetState(IWebProxy currentProxy, bool forceFullTest) {
			
			int f = 0;
			INetState state = INetState.Invalid;

			bool offline = false;
			bool connected = false;
			
			try {
				connected = InternetGetConnectedState(out f, 0);
			} catch (Exception ex) {
				Common.Logging.Log.Error("InternetGetConnectedState() API call failed with error: " + Marshal.GetLastWin32Error(), ex);
			}

			InternetStates flags =  (InternetStates)f;

			Common.Logging.Log.Info("InternetGetConnectedState() returned " + connected.ToString());

			// not sure here, if we are really connected. 
			// So we test it explicitly.
			if (connected) {	
				// first try throw "SENS" API. If it fails, we use the conservative Url test method :)
				bool sensApiSucceeds = true;
				try {
					int tmp = 0;	// NetworkAliveFlags
					if (!IsNetworkAlive(ref tmp)) {
						connected = false;
					}
				} catch (Exception ex) {	// catch all
					Common.Logging.Log.Error("IsNetworkAlive() API call failed with error: " + Marshal.GetLastWin32Error(), ex);
					sensApiSucceeds = false;
				}

				// above tests are not always returning the correct results (e.g. on W2K I tested)
				// so we enforce periodically a request of a web page
				fullInternetStateTestCounter++;
				if (fullInternetStateTestCounter >= 2) {
					forceFullTest = true;
					fullInternetStateTestCounter = 0;
				}

				if (!sensApiSucceeds || forceFullTest) {
					connected = ApiCheckConnection(currentProxy);
					if (!connected) {
						connected = FrameworkCheckConnection(currentProxy);
					}
				}

			} else {	// not connected
				
				if ((flags & InternetStates.INTERNET_CONNECTION_MODEM) != InternetStates.INTERNET_CONNECTION_MODEM) {
					connected = ApiCheckConnection(currentProxy);
					if (!connected) {
						connected = FrameworkCheckConnection(currentProxy);
					}
				} else {
					Common.Logging.Log.Info("InternetGetConnectedState() flag INTERNET_CONNECTION_MODEM is set. Give up further tests...");
				}

			}
			
			state |= connected ? INetState.Connected: INetState.DisConnected;

			offline = ((flags & InternetStates.INTERNET_CONNECTION_OFFLINE) == InternetStates.INTERNET_CONNECTION_OFFLINE);
			state |= offline ? INetState.Offline: INetState.Online;
			
			return state;
		}
		
		public static bool ApiCheckConnection(IWebProxy proxy) {
			//TODO: how about the proxy if we call the API function?
			string url = probeUrls[new Random().Next(0,probeUrls.GetUpperBound(0))];	
			try {
				Common.Logging.Log.Info("ApiCheckConnection('"+url+"') ");
				if (InternetCheckConnection(url, 0, 0)) 
					return true;
			} catch (Exception ex) {
				Common.Logging.Log.Error("ApiCheckConnection('"+url+"') failed with error: " + Marshal.GetLastWin32Error(), ex);
			}
			Common.Logging.Log.Info("ApiCheckConnection() returns false");
			return false;
		}

		public static bool FrameworkCheckConnection(IWebProxy proxy) {
			string url = probeUrls[new Random().Next(0,probeUrls.GetUpperBound(0))];	

			if (proxy == null) 
				proxy = GlobalProxySelection.GetEmptyWebProxy(); 
							
			try {
				Common.Logging.Log.Info("FrameworkCheckConnection('"+url+"') ");
				using (HttpWebResponse response = (HttpWebResponse)NewsComponents.Net.AsyncWebRequest.GetSyncResponseHeadersOnly(url, proxy, 20 * 1000)) {	
					if (response != null && String.Compare(response.Method, "HEAD") == 0) {	// success
						response.Close();
						return true;
					}
				}
			} catch (Exception ex) {
				Common.Logging.Log.Error("FrameworkCheckConnection('"+url+"') ", ex);
			}

			Common.Logging.Log.Info("FrameworkCheckConnection() returns false");
			return false;
		}

		public static void SetIEOffline(bool modeOffline) {

			INTERNET_CONNECTED_INFO ci = new INTERNET_CONNECTED_INFO();
			
			if(modeOffline) {
				ci.dwConnectedState = INTERNET_STATE_DISCONNECTED_BY_USER;
				ci.dwFlags = ISO_FORCE_DISCONNECTED;
			} else {
				ci.dwConnectedState = INTERNET_STATE_CONNECTED;
			}

			InternetSetOption(IntPtr.Zero, INTERNET_OPTION_CONNECTED_STATE, ref
				ci, Marshal.SizeOf(typeof(INTERNET_CONNECTED_INFO)));

			RefreshIESettings();

		}

		private static void RefreshIESettings() {
  
			InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED,
				IntPtr.Zero, 0);
		}


		private static int[] dayIndexMap = new int[]{1,2,3,4,5,6,7,14,21,30,60,90,180,270,365};
		
		public static TimeSpan MaxItemAgeFromIndex(int index) {
			if (index < 0) {
				return TimeSpan.Zero;
			} else if (index > dayIndexMap.Length-1) {
				return TimeSpan.MinValue;	// unlimited
			} else {
				return TimeSpan.FromDays(dayIndexMap[index]); 
			}
		}

		public static int MaxItemAgeToIndex(TimeSpan timespan) {
			int maxItemAgeDays = Math.Abs(timespan.Days);
			if (maxItemAgeDays <= dayIndexMap[6]) {	// 0..7 days
				// Need to ensure that we return a positive value
				if (maxItemAgeDays == 0) {
					return 0;
				}
				return  maxItemAgeDays - 1;
			} else if (maxItemAgeDays > dayIndexMap[6] && maxItemAgeDays <= dayIndexMap[7]) {
				return  7;	// 14 days
			} else if (maxItemAgeDays > dayIndexMap[7] && maxItemAgeDays <= dayIndexMap[8]) {
				return  8;	// 21 days
			} else if (maxItemAgeDays > dayIndexMap[8] && maxItemAgeDays <= dayIndexMap[9]) {
				return  9;	// 1 month
			} else if (maxItemAgeDays > dayIndexMap[9] && maxItemAgeDays <= dayIndexMap[10]) {
				return  10;	// 2 month
			} else if (maxItemAgeDays > dayIndexMap[10] && maxItemAgeDays <= dayIndexMap[11]) {
				return  11;	// 1 quarter
			} else if (maxItemAgeDays > dayIndexMap[11] && maxItemAgeDays <= dayIndexMap[12]) {
				return  12;	// 2 quarter
			} else if (maxItemAgeDays > dayIndexMap[12] && maxItemAgeDays <= dayIndexMap[13]) {
				return  13;	// 3 quarter
			} else if (maxItemAgeDays > dayIndexMap[13] && maxItemAgeDays <= dayIndexMap[14]) {
				return  14;	// 1 year
			} else if (maxItemAgeDays > dayIndexMap[14] || timespan.Equals(TimeSpan.MinValue)) {
				return  15; // unlimited
			} else
				return   9;	// 30 days, one month
		}

		public static TimeSpan MapRssSearchItemAge(int index) {
			switch (index) {
				case 0: return new TimeSpan(1,0,0);	// 1 hour
				case 1: return new TimeSpan(2,0,0);	// 2 hours
				case 2: return new TimeSpan(3,0,0);	// 3 hours
				case 3: return new TimeSpan(4,0,0);	// 4 hours
				case 4: return new TimeSpan(5,0,0);	// 5 hours
				case 5: return new TimeSpan(6,0,0);	// 6 hours
				case 6: return new TimeSpan(12,0,0);	// 12 hours
				case 7: return new TimeSpan(18,0,0);	// 18 hours
				case 8: return new TimeSpan(24,0,0);	// 1 day
				case 9: return new TimeSpan(2*24,0,0);	// 2 days
				case 10: return new TimeSpan(3*24,0,0);	// 3 days
				case 11: return new TimeSpan(4*24,0,0);	// 4 days
				case 12: return new TimeSpan(5*24,0,0);	// 5 days
				case 13: return new TimeSpan(6*24,0,0);	// 6 days
				case 14: return new TimeSpan(7*24,0,0);	// 7 days
				case 15: return new TimeSpan(14*24,0,0);	// 14 days
				case 16: return new TimeSpan(21*24,0,0);	// 21 days
				case 17: return new TimeSpan(30*24,0,0);	// 1 month
				case 18: return new TimeSpan(60*24,0,0);	// 2 month
				case 19: return new TimeSpan(91*24,0,0);	// 1 quarter
				case 20: return new TimeSpan(2*91*24,0,0);	// 2 quarters
				case 21: return new TimeSpan(3*91*24,0,0);	// 3 quarters
				case 22: return new TimeSpan(365*24,0,0);	// 1 year
				case 23: return new TimeSpan(2*365*24,0,0);	// 2 years
				case 24: return new TimeSpan(3*365*24,0,0);	// 3 years
				case 25: return new TimeSpan(5*365*24,0,0);	// 5 years
				default:
					return TimeSpan.MinValue;
			}
		}

		public static int MapRssSearchItemAge(TimeSpan age) {
			switch ((int)age.TotalHours) {	// returns the index used within the comboBox
				case 1: return 0;	// 1 hour
				case 2: return 1;	// 2 hours
				case 3: return 2;	// 3 hours
				case 4: return 3;	// 4 hours
				case 5: return 4;	// 5 hours
				case 6: return 5;	// 6 hours
				case 12: return 6;	// 12 hours
				case 18: return 7;	// 18 hours
				case 24: return 8;	// 1 day
				case 2*24: return 9;	// 2 days
				case 3*24: return 10;	// 3 days
				case 4*24: return 11;	// 4 days
				case 5*24: return 12;	// 5 days
				case 6*24: return 13;	// 6 days
				case 7*24: return 14;	// 7 days
				case 14*24: return 15;	// 14 days
				case 21*24: return 16;	// 21 days
				case 30*24: return 17;	// 1 month
				case 60*24: return 18;	// 2 month
				case 91*24: return 19;	// 1 quarter
				case 2*91*24: return 20;	// 2 quarters
				case 3*91*24: return 21;	// 3 quarters
				case 365*24: return 22;	// 1 year
				case 2*365*24: return 23;	// 2 years
				case 3*365*24: return 24;	// 3 years
				case 5*365*24: return 25;	// 5 years
				default:
					return 0;
			}

		}

		public static void ApplyUrlCompletionToControl(Control control) {
			ApplyUrlCompletionToControl(control, false);
		}
		
		public static void ApplyUrlCompletionToControl(Control control, bool includeFileCompletion) {
			try {
				ShellLib.ShellAutoComplete ac = new ShellLib.ShellAutoComplete();
			
				if (control is ComboBox) {
					// set combo handle
					ShellLib.ShellApi.ComboBoxInfo info = new ShellLib.ShellApi.ComboBoxInfo();
					info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
					if (ShellLib.ShellApi.GetComboBoxInfo(control.Handle, ref info)) {
						if (info.hwndEdit != IntPtr.Zero)
							ac.EditHandle = info.hwndEdit;
						else {
							Common.Logging.Log.Debug("ApplyUrlCompletionToControl()::ComboBox must have the DropDown style!");
							return;
						}
					} 
				} else {
					ac.EditHandle = control.Handle;
				}
				// set options
				ac.ACOptions = ShellLib.ShellAutoComplete.AutoCompleteOptions.None;
				ac.ACOptions |= ShellLib.ShellAutoComplete.AutoCompleteOptions.AutoSuggest;
				ac.ACOptions |= ShellLib.ShellAutoComplete.AutoCompleteOptions.AutoAppend;
				ac.ACOptions |= ShellLib.ShellAutoComplete.AutoCompleteOptions.FilterPreFixes;
				ac.ACOptions |= (control.RightToLeft == RightToLeft.Yes ) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.RtlReading : 0;

				// set source
				ShellLib.IObjMgr multi = (ShellLib.IObjMgr)ShellLib.ShellAutoComplete.GetACLMulti();
				multi.Append(ShellLib.ShellAutoComplete.GetACLHistory());
				multi.Append(ShellLib.ShellAutoComplete.GetACLMRU());
				if (includeFileCompletion)
					multi.Append(ShellLib.ShellAutoComplete.GetACListISF());
				ac.ListSource = multi;
			
				// activate AutoComplete
				ac.SetAutoComplete(true, "http://www./%s/.com");

			} catch (Exception ex) {
				_log.Fatal("ApplyUrlCompletionToControl() failed on Control " + control.Name + ". No completion will be available there.", ex);
			}
		}

		/// <summary>
		/// Returns whether Windows XP Visual Styles are currently enabled
		/// </summary>
		public static bool VisualStylesEnabled {
			get {
				OperatingSystem os = System.Environment.OSVersion;

				// check if the OS id XP or higher
				if (os.Platform == PlatformID.Win32NT && ((os.Version.Major == 5 && os.Version.Minor >= 1) || os.Version.Major > 5)) {
					// are themes enabled
					if (UxTheme.IsThemeActive() && UxTheme.IsAppThemed()) {
						Win32.DLLVERSIONINFO version = new Win32.DLLVERSIONINFO();
						version.cbSize = Marshal.SizeOf(typeof(Win32.DLLVERSIONINFO));

						// are we using Common Controls v6
						if (Win32.DllGetVersion(ref version) == 0) {
							return (version.dwMajorVersion > 5);
						}
					}
				}

				return false;
			}
		}
	}// Utils

	#endregion

	#region RootNode
	internal class RootNode: FeedTreeNodeBase
	{
		private static ContextMenu _popup = null;	// share one context menu

		public RootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Root, false, imageIndex, selectedImageIndex) 
		{
			_popup = menu; 
			base.Editable = false;
		}

		public override object Clone() {
			return new RootNode(this.Key, this.ImageIndex, this.SelectedImageIndex, _popup);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType)
		{
			return (nsType == FeedNodeType.Feed || nsType == FeedNodeType.Category);
		}
		public override void PopupMenu(System.Drawing.Point screenPos)
		{ /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu()
		{
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}

		#endregion
	}
	#endregion

	#region CategoryNode
	internal class CategoryNode: FeedTreeNodeBase
	{
		private static ContextMenu _popup = null;	// share one context menu

		public CategoryNode(string text):base(text,FeedNodeType.Category, true, 2, 3) {}
		public CategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Category, true, imageIndex, selectedImageIndex) 
		{
			_popup = menu; 
		}

		public override object Clone() {
			return new CategoryNode(this.Key, this.ImageIndex, this.SelectedImageIndex, _popup);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType)
		{
			return (nsType == FeedNodeType.Category || nsType == FeedNodeType.Feed);
		}
		public override void PopupMenu(System.Drawing.Point screenPos)
		{ /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu()
		{
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}
		#endregion
	}
	#endregion

	#region FeedNode
	internal class FeedNode: FeedTreeNodeBase
	{
		private static ContextMenu _popup = null;	// share one context menu

		public FeedNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Feed, true, imageIndex, selectedImageIndex) 
		{
			_popup = menu; 
		}

		public override object Clone() {
			return new FeedNode(this.Key, this.ImageIndex, this.SelectedImageIndex, _popup);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType)
		{	// no childs allowed
			return false;
		}
		public override void PopupMenu(System.Drawing.Point screenPos)
		{ /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu()
		{
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}

		#endregion
	}
	#endregion

	#region SpecialRootNode
	/// <summary>
	/// Root node to hold special folders, like Feed Errors and Flagged Item nodes.
	/// </summary>
	internal class SpecialRootNode:FeedTreeNodeBase {
		private ContextMenu _popup = null;						// context menu

		public SpecialRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Root, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
			base.Editable = false;
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
		 	// some childs allowed
			if (nsType == FeedNodeType.Finder ||
					nsType == FeedNodeType.SmartFolder
				)
				return true;

			return false;
		}
		public override void PopupMenu(System.Drawing.Point screenPos) {
		  /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu() {
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}

		#endregion

	}
	#endregion

	#region WasteBasketNode
	/// <summary>
	/// Stores the deleted items.
	/// </summary>
	internal class WasteBasketNode: SmartFolderNodeBase {

		public WasteBasketNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {}

	}
	#endregion

	#region SentItemsNode
	/// <summary>
	/// Stores the sent items (reply comments, NNTP Posts).
	/// </summary>
	internal class SentItemsNode: SmartFolderNodeBase {

		public SentItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {	}

	}
	#endregion

	#region FinderSearchNodes
	[Serializable]
	public class FinderSearchNodes {
		
		[XmlArrayItem(typeof(RssFinder))]
		public ArrayList RssFinderNodes = new ArrayList();
		
		public FinderSearchNodes(FeedTreeNodeBase[] nodes) {
			foreach (FeedTreeNodeBase node in nodes) {
				this.GetFinders(node);
			}
		}
		public FinderSearchNodes() {	}

		public void SetScopeResolveCallback(RssFinder.SearchScopeResolveCallback resolver) {
			foreach (RssFinder f in RssFinderNodes) {
				f.ScopeResolver = resolver;
			}
		}

		/// <summary>
		/// Iterate recursivly to get all finders from the treenode collection(s)
		/// </summary>
		/// <param name="startNode"></param>
		private void GetFinders(FeedTreeNodeBase startNode) {
			if (startNode == null)
				return;
			if (startNode.Nodes.Count == 0) {
				FinderNode agn = startNode as FinderNode;
				if (agn != null)
					this.RssFinderNodes.Add(agn.Finder);
			} else {
				foreach (FeedTreeNodeBase node in startNode.Nodes) {
					this.GetFinders(node);
				}
			}
		}
		
	}
	#endregion

	#region FinderRootNode
	/// <summary>
	/// Root node to hold persistent FinderNodes.
	/// </summary>
	internal class FinderRootNode:FeedTreeNodeBase {
		private ContextMenu _popup = null;						// context menu
		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(FinderRootNode));

		public FinderRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Root, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
			base.Editable = false;
		}

		public void InitFromFinders(ArrayList finderList, ContextMenu menu) {
			
			Hashtable categories = new Hashtable();
			
			foreach (RssFinder finder in finderList) {
				FeedTreeNodeBase parent = this;

				finder.Container = null;	// may contain old node references on a re-populate call

				if (finder.FullPath.IndexOf("\\") > 0) {	// one with category
					
					string[] a = finder.FullPath.Split(new char[]{'\\'});
					int aLen = a.GetLength(0);
					string sCat = String.Join(@"\",a, 0, aLen-1);

					if (categories.ContainsKey(sCat)) {
						parent = (FeedTreeNodeBase)categories[sCat];
					} else {	// create category/categories
						
						StringBuilder sb = new StringBuilder();
						sb.Append(a[0]);
						for (int i = 0; i <= aLen - 2; i++) {
							sCat = sb.ToString();
							if (categories.ContainsKey(sCat)) {
								parent = (FeedTreeNodeBase)categories[sCat];
							} else {
								FeedTreeNodeBase cn = new FinderCategoryNode(a[i], 2, 3, menu);	// menu???
								categories.Add(sCat, cn);
								parent.Nodes.Add(cn);
								parent = cn;
							}
							sb.Append("\\" + a[i+1]);
						}
					}
				}

				FinderNode n = new FinderNode(finder.Text, 10, 10, menu);
				n.Finder = finder;		// interconnect
				finder.Container = n;
				parent.Nodes.Add(n);
			}//foreach finder
		
		}


		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			// some childs allowed
			if (nsType == FeedNodeType.Finder)
				return true;

			return false;
		}
		public override void PopupMenu(System.Drawing.Point screenPos) {
			/*
			  if (_popup != null)
				  _popup.TrackPopup(screenPos);
			  */
		}
		public override void UpdateContextMenu() {
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}

		#endregion

	}
	#endregion

	#region FinderCategoryNode
	internal class FinderCategoryNode: FeedTreeNodeBase {
		private static ContextMenu _popup = null;	// share one context menu

		public FinderCategoryNode(string text):base(text,FeedNodeType.FinderCategory, true, 2, 3) {}
		public FinderCategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.FinderCategory, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
		}


		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return (nsType == FeedNodeType.FinderCategory || nsType == FeedNodeType.Finder);
		}
		public override void PopupMenu(System.Drawing.Point screenPos) {
		  /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu() {
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}
		#endregion
	}
	#endregion

	#region ExceptionReportNode
	/// <summary>
	/// Stores the feed failures and exceptions reported while retrieving.
	/// </summary>
	internal class ExceptionReportNode: SmartFolderNodeBase {

		public ExceptionReportNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(null, text, imageIndex, selectedImageIndex, menu) {
		}

		#region Overrides of ISmartFolder to delegate item handling to ExceptionManager instance
		public override void MarkItemRead(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
					break;
				}
			}
		}

		public override void MarkItemUnread(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = false;
					break;
				}
			}
		}

		public override bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}

		public override int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
					if (!ri.BeenRead) i++;
				}
				return i;
			}
		}

		public override ArrayList Items {
			get {	return ExceptionManager.GetInstance().Items;	}
		}

		public override void Add(NewsItem item) {	// not the preferred way to add exceptions, but impl. the interface
			ExceptionManager.GetInstance().Add(item);
		}
		public override void Remove(NewsItem item) {
			ExceptionManager.GetInstance().Remove(item);
		}

		public override bool Modified {
			get { return ExceptionManager.GetInstance().Modified;  }
			set { ExceptionManager.GetInstance().Modified = value; }
		}

		#endregion

	}
	#endregion

	#region FlaggedItemsNode
	/// <summary>
	/// Stores the various flagged items.
	/// </summary>
	internal class FlaggedItemsNode: SmartFolderNodeBase {
		private Flagged flagsFiltered = Flagged.None;

		public FlaggedItemsNode(Flagged flag, LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, text, imageIndex, selectedImageIndex, menu) {
			flagsFiltered = flag;
		}

		public Flagged FlagFilter {
			get { return flagsFiltered; }
		}

		#region Some Overrides of ISmartFolder to filter items by flags
		public override void MarkItemRead(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in base.itemsFeed.Items) {
				if (item.Equals(ri) && ri.FlagStatus == flagsFiltered) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
					break;
				}
			}
		}

		public override void MarkItemUnread(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in base.itemsFeed.Items) {
				if (item.Equals(ri) && ri.FlagStatus == flagsFiltered) {
					ri.BeenRead = false;
					break;
				}
			}
		}

		public override bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in base.itemsFeed.Items) {
					if (!ri.BeenRead  && ri.FlagStatus == flagsFiltered) return true;
				}
				return false;
			}
		}

		public override int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in base.itemsFeed.Items) {
					if (!ri.BeenRead && ri.FlagStatus == flagsFiltered) i++;
				}
				return i;
			}
		}

		public override ArrayList Items {
			get {	
				ArrayList a = new ArrayList(base.itemsFeed.Items.Count);
				foreach (NewsItem ri in base.itemsFeed.Items) {
					if (ri.FlagStatus == flagsFiltered)
						a.Add(ri);	
				}
				return a;
			}
		}

		#endregion

	}
	#endregion

	#region FinderNode
	/// <summary>
	/// Represents a search folder in the tree.
	/// </summary>
	public class FinderNode:FeedTreeNodeBase, ISmartFolder {
		private ContextMenu _popup = null;						// context menu
		private LocalFeedsFeed itemsFeed;
		private Hashtable items = Hashtable.Synchronized(new Hashtable(17));	// do we need the syncronized version?
		private RssFinder finder = null;

		public FinderNode():base() {}
		public FinderNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Finder, true, imageIndex, selectedImageIndex) {
			itemsFeed = new LocalFeedsFeed(
				"http://localhost/rssbandit/searchfolder?id="+Guid.NewGuid(),
				text,	String.Empty, false);
			_popup = menu; 
		}
		
		public RssFinder Finder {
			get { return finder;	}
			set { finder = value;  }
		}

		public string InternalFeedLink {
			get { return itemsFeed.link; }
		}

		public bool Contains(NewsItem item) {	
			return items.ContainsKey(item);
		}

		public void Clear() {	
			items.Clear();
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
		 	// no childs allowed
			return false;
		}
		public override void PopupMenu(System.Drawing.Point screenPos) {
		  /*
			if (_popup != null)
				_popup.TrackPopup(screenPos);
			*/
		}
		public override void UpdateContextMenu() {
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}

		#endregion

		#region ISmartFolder Members

		public bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in items.Keys) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}

		public int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in items.Keys) {
					if (!ri.BeenRead) i++;
				}
				return i;
			}
		}

		public void MarkItemRead(NewsItem item) {
			if (item == null) return;
			if (items.ContainsKey(item)) {
				NewsItem ri = (NewsItem)items[item];
				if (!ri.BeenRead) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
				}
			}
		}

		public void MarkItemUnread(NewsItem item) {
			if (item == null) return;
			if (items.ContainsKey(item)) {
				NewsItem ri = (NewsItem)items[item];
				if (ri.BeenRead) {
					ri.BeenRead = false;
					base.UpdateReadStatus(this, 1);
				}
			}
		}

		public ArrayList Items {
			get {	
				return new ArrayList(items.Keys);	
			}
		}

		public void Add(NewsItem item) {	
			if (item == null) return;
			if (!items.ContainsKey(item))
				items.Add(item, null);
		}
		public void Remove(NewsItem item) {
			if (item == null) return;
			if (items.ContainsKey(item))
				items.Remove(item);
		}

		public void UpdateReadStatus() {
			base.UpdateReadStatus(this, this.NewMessagesCount);
		}

		public bool Modified {
			get { return itemsFeed.Modified;  }
			set { itemsFeed.Modified = value; }
		}

		#endregion

	}
	#endregion

	#region RssFinder
	/// <summary>
	/// Class get's serialized to persist defined searches.
	/// </summary>
	[Serializable]
	public class RssFinder 	{

		/// <summary>
		/// Called to resolve the array of category paths and
		/// feed Urls. The callback should return a array of feedsFeed, that contains
		/// the valid scope set. 
		/// </summary>
		public delegate feedsFeed[] SearchScopeResolveCallback(ArrayList categoryPaths, ArrayList feedUrls);

		#region private ivars
		private SearchCriteriaCollection searchCriterias = null;
		private feedsFeed[] searchScope = new feedsFeed[]{};
		private ArrayList categoryPathScope, feedUrlScope;
		private SearchScopeResolveCallback resolve;
		private FinderNode container;
		private bool doHighlight, dynamicItemContent = false, dynamicItemContentChecked = false, isInitialized = false;
		private string fullpathname;
		private string externalSearchUrl = null, externalSearchPhrase = null;
		private bool externalResultMergedWithLocal = false;
		#endregion

		#region ctor's
		public RssFinder(){
			dynamicItemContentChecked = false;
			categoryPathScope = new ArrayList();
			feedUrlScope = new ArrayList();
			searchCriterias = new SearchCriteriaCollection();
		}
		public RssFinder(FinderNode resultContainer, SearchCriteriaCollection criterias, ArrayList categoryPathScope, ArrayList feedUrlScope, SearchScopeResolveCallback resolveSearchScope,  bool doHighlight):this(){
			this.container = resultContainer; 
			if (resultContainer != null)
				this.fullpathname = resultContainer.FullPath;
			
			if (criterias != null)
				this.searchCriterias = criterias;

			if (categoryPathScope != null)
				this.categoryPathScope = categoryPathScope;
			if (feedUrlScope != null)
				this.feedUrlScope = feedUrlScope;
			
			this.resolve = resolveSearchScope;
			this.doHighlight = doHighlight;
			this.dynamicItemContent = this.CheckForDynamicItemContent();
		}
		#endregion

		#region public properties/methods
		[XmlIgnore]
		public string Text {
			get { 
				if (container != null)
					return container.Text;
				string[] a = fullpathname.Split(new char[]{'\\'});
				return a[a.GetLength(0)-1];
			}
			set { 
				if (container != null)
					container.Text = value;
			}
		}

		public string FullPath {
			get {
				if (container != null) {
					string s = container.FullPath.Trim();
					string[] a = s.Split(new char[]{'\\'});
					if (a.GetLength(0) > 1)
						return String.Join(@"\",a, 1, a.GetLength(0)-1);
			
					return s;	// name only
				} else {
					return fullpathname;
				}
			}
			set {
				fullpathname = value;
			}
		}

		public SearchCriteriaCollection SearchCriterias {
			get { 
				RaiseScopeResolver();
				return searchCriterias;	
			}
			set { 
				searchCriterias = value;  
				this.dynamicItemContent = this.CheckForDynamicItemContent();
			}
		}

		[XmlArray("category-scopes"), XmlArrayItem("category", Type = typeof(System.String), IsNullable = false)]
		public ArrayList CategoryPathScope {
			get { return categoryPathScope;}
			set { categoryPathScope = value; }
		}
		[XmlArray("feedurl-scopes"), XmlArrayItem("feedurl", Type = typeof(System.String), IsNullable = false)]
		public ArrayList FeedUrlScope {
			get { return feedUrlScope;}
			set { feedUrlScope = value;  }
		}

		[XmlIgnore()]
		public feedsFeed[] SearchScope {
			get { 
				RaiseScopeResolver();
				return searchScope;	
			}
			set { searchScope = value;  }
		}

		[XmlIgnore()]
		public bool HasDynamicItemContent {
			get { 
				if (!dynamicItemContentChecked)
					this.dynamicItemContent = this.CheckForDynamicItemContent();

				return this.dynamicItemContent;	
			}
		}

		public bool DoHighlight {
			get { return doHighlight;	}
			set { doHighlight = value;  }
		}

		[XmlIgnore()]
		public bool ExternalResultMerged {
			get { return externalResultMergedWithLocal;	}
			set { externalResultMergedWithLocal = value;  }
		}
		[XmlIgnore()]
		public string ExternalSearchUrl {
			get { return externalSearchUrl;	}
			set { externalSearchUrl = value;  }
		}
		[XmlIgnore()]
		public string ExternalSearchPhrase {
			get { return externalSearchPhrase;	}
			set { externalSearchPhrase = value;  }
		}

		[XmlIgnore()]
		public FinderNode Container {
			get { return container;		}
			set { container = value;	}
		}

		[XmlIgnore()]
		public SearchScopeResolveCallback ScopeResolver {
			get { return resolve;		}
			set { resolve = value;	}
		}

		public void SetSearchScope(ArrayList categoryPathScope, ArrayList feedUrlScope) {
			this.categoryPathScope = categoryPathScope;
			this.feedUrlScope = feedUrlScope;
			this.isInitialized = false;
		}

		/// <summary>
		/// Call it on every change of a category name or deletion.
		/// </summary>
		/// <param name="oldCategoryPath">Old category name</param>
		/// <param name="newCategoryPath">New category name. If null, it is recognized as deleted</param>
		public void NotifyCategoryChanged(string oldCategoryPath, string newCategoryPath) {
			categoryPathScope.Remove(oldCategoryPath);
			if (newCategoryPath != null)
				categoryPathScope.Add(newCategoryPath);
		}
		/// <summary>
		/// Call it on every change of a feed Url or deletion.
		/// </summary>
		/// <param name="oldFeedUrl">Old feed Url</param>
		/// <param name="newFeedUrl">New feed Url. If null, it is recognized as deleted</param>
		public void NotifyFeedUrlChanged(string oldFeedUrl, string newFeedUrl) {
			feedUrlScope.Remove(oldFeedUrl);
			if (newFeedUrl != null)
				feedUrlScope.Add(newFeedUrl);
		}

		#endregion

		#region private properties/methods

		private void RaiseScopeResolver() {
			if (this.resolve != null && !isInitialized) {
				this.searchScope = resolve(categoryPathScope, feedUrlScope);
				isInitialized = true;
			}
		}

		private bool CheckForDynamicItemContent() {
			dynamicItemContentChecked = false;
			bool isDynamic = false;
			if (this.searchCriterias == null || this.searchCriterias.Count == 0)
				return isDynamic;

			foreach (ISearchCriteria icriteria in this.searchCriterias) {
				if (icriteria is SearchCriteriaAge) {
					isDynamic = true;
					break;
				}
				if (icriteria is SearchCriteriaProperty) {
					isDynamic = true;
					break;
				}
			}
			dynamicItemContentChecked = true;
			return isDynamic;
		}

		#endregion
	}

	#endregion
}
