#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region usings

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Genghis;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using NewsComponents.Utils;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

#endregion

namespace RssBandit.WinGui.Controls {
	
	/// <summary>
	/// Does support general state serialization of on controls
	/// </summary>
	public class StateSerializationHelper 
	{

		#region ctor's

		private StateSerializationHelper() {}
		
		#endregion

		#region ------- PRIVATE METHODS ---------------------------------------

		

		private static string GetKeyOrderArray(Infragistics.Shared.KeyedSubObjectsCollectionBase c, string separator) {
			StringBuilder sb = new StringBuilder();
			Infragistics.Shared.IKeyedSubObject[] a = new Infragistics.Shared.IKeyedSubObject[c.Count];
			c.CopyTo(a, 0);
			for (int i = 0; i < a.Length; i++) {
				Infragistics.Shared.IKeyedSubObject o = a[i];
				if (o.Key == null || o.Key.Length == 0)
					throw new InvalidOperationException("KeyedSubObjectsCollectionBase must have a unique Key.");
				if (i > 0) sb.Append(separator);
				sb.Append(o.Key);
			}
			return sb.ToString();
		}

		#endregion

		#region public members

#if USE_UltraDockManager
		
		private static Version _infragisticsDockingVersion = null;
		public static Version InfragisticsDockingVersion {
			get { 
				if (_infragisticsDockingVersion == null)
					_infragisticsDockingVersion = Assembly.GetAssembly(typeof(UltraDockManager)).GetName().Version;
				return _infragisticsDockingVersion;
			}
		}

		/// <summary>
		/// Saves the current state of a UltraDockManager (including images & texts) to a byte-array.
		/// No exceptions are catched in this method
		/// </summary>
		/// <param name="dockManager">UltraDockManager</param>
		/// <returns>byte[]</returns>
		public static byte[] SaveControlStateToByte(UltraDockManager dockManager) {
			using (MemoryStream stream = SaveDockManager(dockManager, true)) {
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Saves the current state of a UltraDockManager (including images & texts) to a (Xml) string.
		/// No exceptions are catched in this method
		/// </summary>
		/// <param name="dockManager">UltraDockManager</param>
		/// <returns>string (Xml)</returns>
		public static string SaveControlStateToString(UltraDockManager dockManager) {
			using (MemoryStream stream = SaveDockManager(dockManager, false)) {
				StreamReader r = new StreamReader(stream);
				return r.ReadToEnd();
			}
		}

		/// <summary>
		/// Saves the current state of a DockManager (including images & texts) to a byte-array.
		/// No exceptions are catched in this method
		/// </summary>
		public static MemoryStream SaveDockManager(UltraDockManager dockManager, bool asBinary) {
			MemoryStream stream = new MemoryStream();
			if (asBinary)
				dockManager.SaveAsBinary(stream);
			else
				dockManager.SaveAsXML(stream);

			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		/// <summary>
		/// Restores a DockManager using the provided byte-array.
		/// Prior to applying the settings the property 'Text' of all DockabelControlPanes is
		/// saved to a collection and restored after applying the settings from the byte-array.
		/// This avoids that string from a different login-language are restored.
		/// Load failures are ignored.
		/// </summary>
		/// <param name="dockManager">UltraDockManager</param>
		/// <param name="theSettings">byte[]</param>
		public static void LoadControlStateFromByte(UltraDockManager dockManager, byte[] theSettings) {
			if (theSettings == null)
				return;
			if (theSettings.Length == 0)
				return;

			using (Stream stream = new MemoryStream(theSettings)) {
				LoadDockManager(dockManager, stream, true);
			}
		}

		/// <summary>
		/// Restores a DockManager using the provided string.
		/// Prior to applying the settings the property 'Text' of all DockabelControlPanes is
		/// saved to a collection and restored after applying the settings from the byte-array.
		/// This avoids that string from a different login-language are restored.
		/// Load failures are ignored.
		/// </summary>
		/// <param name="dockManager">UltraDockManager</param>
		/// <param name="theSettings">string</param>
		public static void LoadControlStateFromString(UltraDockManager dockManager, string theSettings) {
			if (string.IsNullOrEmpty(theSettings))
				return;

			using (Stream stream = new MemoryStream()) {
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(theSettings);
				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				LoadDockManager(dockManager, stream, false);
			}
		}

		/// <summary>
		/// Restores a DockManager using the provided byte-array.
		/// Prior to applying the settings the property 'Text' of all DockableControlPanes is
		/// saved to a collection and restored after applying the settings from the byte-array.
		/// This avoids that string from a different login-language are restored.
		/// No Exceptions are catched by this method.
		/// </summary>
		/// <param name="dockManager">UltraDockManager</param>
		/// <param name="stream">Stream</param>
		/// <param name="asBinary">bool</param>
		public static void LoadDockManager(UltraDockManager dockManager, Stream stream, bool asBinary) {
			DockAreaPane oDockArea;
			DockableControlPane oDockContPane;
			int i, j;
			Hashtable oTexts;


			//First remember original (current language) strings
			oTexts = new Hashtable();
			for (i = 0; i < dockManager.DockAreas.Count; i++) {
				oDockArea = dockManager.DockAreas[i];
				for (j = 0; j < oDockArea.Panes.Count; j++) {
					oDockContPane = oDockArea.Panes[j] as DockableControlPane;
					if (oDockContPane != null) {
						oTexts.Add(oDockContPane.Control.Name, oDockContPane.Text);
					}
				}
			}

			//Now load the settings
			try {
				if (asBinary)
					dockManager.LoadFromBinary(stream);
				else
					dockManager.LoadFromXML(stream);
			}
			catch (Exception ex) {
				Trace.WriteLine("dockManager.LoadFrom...() failed: " + ex.Message);
				return; // use it as it was initialized on the original form
			}

			//The stream already has the captions stored, so overwrite them
			//with the current ones (could be different language)
			for (i = 0; i < dockManager.DockAreas.Count; i++) {
				oDockArea = dockManager.DockAreas[i];
				for (j = 0; j < oDockArea.Panes.Count; j++) {
					oDockContPane = oDockArea.Panes[j] as DockableControlPane;
					if (oDockContPane != null) {
						if (oTexts.Contains(oDockContPane.Control.Name)) {
							oDockContPane.Text = (string) oTexts[oDockContPane.Control.Name];
						}
					}
				}
			}
		}

#endif


		private static Version _infragisticsToolbarVersion = null;
		public static Version InfragisticsToolbarVersion {
			get { 
				if (_infragisticsToolbarVersion == null)
					_infragisticsToolbarVersion = Assembly.GetAssembly(typeof(UltraToolbarsManager)).GetName().Version;
				return _infragisticsToolbarVersion;
			}
		}

		/// <summary>
		/// Saves the current state of a ToolbarManager (including images & texts) to a byte-array.
		/// No exceptions are catched in this method
		/// </summary>
		/// <param name="toolbarManager">UltraToolbarsManager</param>
		/// <param name="saveUserCustomizations">true, to get also the user customizations saved</param>
		/// <returns>byte[]</returns>
		public static byte[] SaveControlStateToByte(UltraToolbarsManager toolbarManager, bool saveUserCustomizations) {
			using (MemoryStream stream = SaveToolbarManager(toolbarManager, saveUserCustomizations, true)) {
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Saves the current state of a ToolbarManager (including images & texts) to a (Xml) string.
		/// No exceptions are catched in this method
		/// </summary>
		public static string SaveControlStateToString(UltraToolbarsManager toolbarManager, bool saveUserCustomizations) {
			using (MemoryStream stream = SaveToolbarManager(toolbarManager, saveUserCustomizations, false)) {
				StreamReader r = new StreamReader(stream);
				return r.ReadToEnd();
			}
		}

		/// <summary>
		/// Saves the current state of a ToolbarManager (including images & texts) to a byte-array.
		/// No exceptions are catched in this method
		/// </summary>
		/// <param name="toolbarManager">UltraToolbarsManager</param>
		/// <param name="saveUserCustomizations">True, if user customizations should be included</param>
		/// <param name="asBinary">True, if Stream should be a binary one; False for an Xml Stream.</param>
		public static MemoryStream SaveToolbarManager(UltraToolbarsManager toolbarManager, bool saveUserCustomizations, bool asBinary) {
			MemoryStream stream = new MemoryStream();
			if (asBinary)
				toolbarManager.SaveAsBinary(stream, saveUserCustomizations);
			else
				toolbarManager.SaveAsXml(stream, saveUserCustomizations);

			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
		
		/// <summary>
		/// Restores a ToolbarManager using the provided byte-array.
		/// Prior to applying the settings the (shared) properties 'Caption' and 'ToolTip'
		/// of each element in the Tools-Collection
		/// saved  and restored after applying the settings from the byte-array.
		/// This avoids that strings from a different login-language are restored.
		/// No Exceptions are catched by this method.
		/// </summary>
		/// <param name="toolbarManager">UltraToolbarsManager</param>
		/// <param name="theSettings">byte[]</param>
		/// <param name="mediator">The mediator.</param>
		public static void LoadControlStateFromByte(UltraToolbarsManager toolbarManager, byte[] theSettings, CommandMediator mediator) {
			if (theSettings == null)
				return;
			if (theSettings.Length == 0)
				return;

			using (Stream stream = new MemoryStream(theSettings)) {
				LoadToolbarManager(toolbarManager, stream, true, mediator);
			}
		}


		/// <summary>
		/// Restores a ToolbarManager using the provided byte-array.
		/// Prior to applying the settings the (shared) properties 'Caption' and 'ToolTip'
		/// of each element in the Tools-Collection
		/// saved  and restored after applying the settings from the byte-array.
		/// This avoids that strings from a different login-language are restored.
		/// No Exceptions are catched by this method.
		/// </summary>
		/// <param name="toolbarManager">UltraToolbarsManager</param>
		/// <param name="theSettings">string</param>
		/// <param name="mediator">The mediator.</param>
		public static void LoadControlStateFromString(UltraToolbarsManager toolbarManager, string theSettings, CommandMediator mediator) {
			if (string.IsNullOrEmpty(theSettings))
				return;

			using (Stream stream = new MemoryStream()) {
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(theSettings);
				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				LoadToolbarManager(toolbarManager, stream, false, mediator);
			}
		}

		
		struct LocalizedProperties
		{
			public string Caption;
			public string ToolTipText;
			public string StatusText;
			public string DescriptionOnMenu;
			public string Category;
			public string CustomizerCaption;
			public string CustomizerDescription;
			
			public LocalizedProperties(ToolBase tool) {
				Caption = tool.SharedProps.Caption;
				ToolTipText = tool.SharedProps.ToolTipText;
				StatusText = tool.SharedProps.StatusText;
				DescriptionOnMenu = tool.SharedProps.DescriptionOnMenu;
				Category = tool.SharedProps.Category;
				CustomizerCaption= tool.SharedProps.CustomizerCaption;
				CustomizerDescription = tool.SharedProps.CustomizerDescription;
			}
			
			public void Apply(ToolBase tool) {
				tool.SharedProps.Caption = Caption;
				tool.SharedProps.ToolTipText = ToolTipText;
				tool.SharedProps.StatusText = StatusText;
				tool.SharedProps.DescriptionOnMenu = DescriptionOnMenu;
				tool.SharedProps.Category = Category;
				tool.SharedProps.CustomizerCaption = CustomizerCaption;
				tool.SharedProps.CustomizerDescription = CustomizerDescription;
			}
		}
		
		/// <summary>
		/// Restores a ToolbarManager using the provided byte-array.
		/// Prior to applying the settings the (shared) properties 'Caption' and 'ToolTip'
		/// of each element in the Tools-Collection
		/// saved  and restored after applying the settings from the byte-array.
		/// This avoids that strings from a different login-language are restored.
		/// No Exceptions are catched by this method.
		/// </summary>
		/// <param name="toolbarManager">UltraToolbarsManager</param>
		/// <param name="stream">Stream</param>
		/// <param name="asBinary">bool</param>
		/// <param name="mediator">The mediator.</param>
		public static void LoadToolbarManager(UltraToolbarsManager toolbarManager, Stream stream, bool asBinary, CommandMediator mediator) 
		{
			//First remember original (current language) strings
			Hashtable oCaptions = new Hashtable();
			for (int i = 0; i < toolbarManager.Tools.Count; i++) {
				ToolBase oTool = toolbarManager.Tools[i];
				LocalizedProperties props = new LocalizedProperties(oTool);
				oCaptions.Add(oTool.Key, props);
			}

			//Now load the settings
			try {
				if (asBinary)
					toolbarManager.LoadFromBinary(stream);
				else
					toolbarManager.LoadFromXml(stream);
			}
			catch (Exception ex) {
				Trace.WriteLine("toolbarManager.LoadFrom...() failed: " + ex.Message);
				return; // use it as it was initialized on the original form
			}

			//The stream already has the captions stored, so overwrite them
			//with the current ones (could be different language)
			for (int i = 0; i < toolbarManager.Tools.Count; i++) {
				ToolBase oTool = toolbarManager.Tools[i];
				if (oCaptions.Contains(oTool.Key)) {
					LocalizedProperties props = (LocalizedProperties) oCaptions[oTool.Key];
					props.Apply(oTool);
				}
				mediator.ReRegisterCommand(oTool as ICommand);
			}
		}


		private static Version _infragisticsExplorerBarVersion = null;
		public static Version InfragisticsExplorerBarVersion {
			get { 
				if (_infragisticsExplorerBarVersion == null)
					_infragisticsExplorerBarVersion = Assembly.GetAssembly(typeof(UltraExplorerBar)).GetName().Version;
				return _infragisticsExplorerBarVersion;
			}
		}
		
		/// <summary>
		/// Restores a UltraExplorerBar using the provided Preferences.
		/// </summary>
		/// <param name="explorerBar">UltraExplorerBar</param>
		/// <param name="store">Settings</param>
		/// <param name="preferenceID">String. The ID the settings should come from 
		/// (multiple sets may exist in the Preference store)</param>
		/// <exception cref="ArgumentNullException">If any of the parameters is null</exception>
		/// <exception cref="InvalidOperationException">If the collection of groups allow duplicate keys
		///  or any group does not have a unique key</exception>
		public static void LoadExplorerBar(UltraExplorerBar explorerBar, Settings store, string preferenceID) {
			
			if (explorerBar == null)
				throw new ArgumentNullException("explorerBar");
			if (store == null)
				throw new ArgumentNullException("store");
			if (preferenceID == null || preferenceID.Length == 0)
				throw new ArgumentNullException("preferenceID");

			if (explorerBar.Groups.AllowDuplicateKeys)
				throw new InvalidOperationException("UltraExplorerBarGroupsCollection must provide unique Keys to support Load/Save settings operations.");

//			explorerBar.LoadFromXml(@"D:\expl.xml");
//			return;

			Preferences prefReader = store.GetSubnode(preferenceID);
			int version = prefReader.GetInt32("version", 0);	// make the impl. extendable
			if (version < 1)
				return;	// wrong version

			Rectangle dimensions = new Rectangle();
			dimensions.X = prefReader.GetInt32("Location.X", explorerBar.Location.X);
			dimensions.Y = prefReader.GetInt32("Location.Y", explorerBar.Location.Y);
			dimensions.Width = prefReader.GetInt32("Size.Width", explorerBar.Size.Width);
			dimensions.Height = prefReader.GetInt32("Size.Height", explorerBar.Size.Height);
			
			if (explorerBar.Dock == DockStyle.None && explorerBar.Anchor == AnchorStyles.None)
				explorerBar.Bounds = dimensions;

			explorerBar.NavigationMaxGroupHeaders = prefReader.GetInt32("MaxGroupHeaders", explorerBar.NavigationMaxGroupHeaders);

			// no groups: nothing more to initialize
			if (explorerBar.Groups.Count == 0)
				return;

			// First handle order of groups.
			// build the default order array:
			string defaultOrder = GetKeyOrderArray(explorerBar.Groups, ";");

			// read saved order:
			string orderArray = prefReader.GetString("groupOrder", defaultOrder);
			ArrayList groupOrder = new ArrayList(orderArray.Split(new char[]{';'}));
			for (int i = 0; i < groupOrder.Count; i++) {
				string key = (string)groupOrder[i];
				if (explorerBar.Groups.Exists(key) && 
					explorerBar.Groups.IndexOf(key) != i && 
					i < explorerBar.Groups.Count) 
				{	// restore:
					UltraExplorerBarGroup group = explorerBar.Groups[key];
					explorerBar.Groups.Remove(group);
					explorerBar.Groups.Insert(i, group);
				}
			}

			string selectedGroup = prefReader.GetString("selected", explorerBar.SelectedGroup.Key);
			
			for (int i = 0; i < explorerBar.Groups.Count; i++) {
				
				UltraExplorerBarGroup group = explorerBar.Groups[i];
				string key = String.Format("group.{0}", i);
				if (group.Key != null && group.Key.Length > 0)
					key = String.Format("group.{0}", group.Key);
				
				group.Visible = prefReader.GetBoolean(String.Format("{0}.Visible", key), group.Visible);

				if (selectedGroup == key)
					group.Selected = true;
			}

		}

		/// <summary>
		/// Saves a UltraExplorerBar using the provided Preferences store.
		/// </summary>
		/// <param name="explorerBar">UltraExplorerBar</param>
		/// <param name="store">Settings</param>
		/// <param name="preferenceID">String. The ID the settings should come from 
		/// (multiple sets may exist in the Preference store)</param>
		/// <exception cref="ArgumentNullException">If any of the parameters is null</exception>
		/// <exception cref="InvalidOperationException">If the collection of groups allow duplicate keys
		///  or any group does not have a unique key</exception>
		public static void SaveExplorerBar(UltraExplorerBar explorerBar, Settings store, string preferenceID) {
			
			if (explorerBar == null)
				throw new ArgumentNullException("explorerBar");
			if (store == null)
				throw new ArgumentNullException("store");
			if (preferenceID == null || preferenceID.Length == 0)
				throw new ArgumentNullException("preferenceID");

			if (explorerBar.Groups.AllowDuplicateKeys)
				throw new InvalidOperationException("UltraExplorerBarGroupsCollection must provide unique Keys to support Load/Save settings operations.");
			
//			explorerBar.SaveAsXml(@"D:\expl.xml");
//			return;
			
			Preferences prefWriter = store.GetSubnode(preferenceID);
			prefWriter.SetProperty("version", 1);		// make the impl. extendable

			prefWriter.SetProperty("Location.X", explorerBar.Location.X);
			prefWriter.SetProperty("Location.Y", explorerBar.Location.Y);
			prefWriter.SetProperty("Size.Width", explorerBar.Size.Width);
			prefWriter.SetProperty("Size.Height", explorerBar.Size.Height);
			
			prefWriter.SetProperty("MaxGroupHeaders", explorerBar.NavigationMaxGroupHeaders);

			// no groups: nothing more to write
			if (explorerBar.Groups.Count == 0) {
				prefWriter.SetProperty("groupOrder", null);
				return;
			}

			// build/write the order array:
			prefWriter.SetProperty("groupOrder", GetKeyOrderArray(explorerBar.Groups, ";"));
			
			for (int i = 0; i < explorerBar.Groups.Count; i++) {
				
				UltraExplorerBarGroup group = explorerBar.Groups[i];
				string key = String.Format("group.{0}", i);
				if (group.Key != null && group.Key.Length > 0)
					key = String.Format("group.{0}", group.Key);
				
				if (group.Selected)
					prefWriter.SetProperty("selected", key);

				prefWriter.SetProperty(String.Format("{0}.Visible", key), group.Visible);

			}

		}

		#endregion
		
	}

}
#region CVS Version Log
/*
 * $Log: StateSerializationHelper.cs,v $
 * Revision 1.4  2006/12/15 13:31:00  t_rendelmann
 * reworked to make dynamic menus work after toolbar gets loaded from .settings.xml
 *
 * Revision 1.3  2006/12/12 12:04:24  t_rendelmann
 * finished: all toolbar migrations; save/restore/customization works
 *
 * Revision 1.2  2006/11/30 12:05:29  t_rendelmann
 * changed; next version with the new menubar and the main toolbar migrated to IG - still work in progress
 *
 * Revision 1.1  2006/10/10 17:43:30  t_rendelmann
 * feature: added a commandline option to allow users to reset the UI (don't init from .settings.xml);
 * fixed: explorer bar state was not saved/restored, corresponding menu entries hold the wrong state on explorer group change
 *
 */
#endregion
