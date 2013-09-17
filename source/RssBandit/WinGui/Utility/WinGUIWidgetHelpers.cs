#region CVS Version Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml; 
using System.Xml.Serialization;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Net;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinStatusBar;
using Infragistics.Win.UltraWinToolbars;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using RssBandit.Common.Logging;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Interfaces;
using RssBandit.Xml;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Utils;

#endregion

namespace RssBandit.WinGui.Utility {

	#region UrlCompletionExtender

	/// <summary>
	/// Used for Ctrl-Enter completion, similar to IE url combobox
	/// </summary>
	public class UrlCompletionExtender 
	{
		
		private string[] urlTemplates = new[] {
												"http://www.{0}.com/",
												"http://www.{0}.net/",
												"http://www.{0}.org/",
												"http://www.{0}.info/",
		};
		private readonly Form ownerForm;
		private readonly IButtonControl ownerCancelButton;
		private int lastExpIndex = -1;
		private string toExpand;

		public UrlCompletionExtender(Form f) {
			if (f != null && f.CancelButton != null) {
				ownerForm = f;
				ownerCancelButton = f.CancelButton;
			}
		}
		
			
		public void Add(Control monitorControl) 
		{
			if (monitorControl != null) 
			{
				monitorControl.KeyDown += OnMonitorControlKeyDown;
				if (ownerForm != null && ownerCancelButton != null) {
					monitorControl.Enter += OnMonitorControlEnter;
					monitorControl.Leave += OnMonitorControlLeave;
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

			bool ctrlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
			if (e.KeyCode == Keys.Return && ctrlKeyPressed) {
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
	public sealed class CultureChanger: IDisposable {
		
		/// <summary>
		/// Gets the CultureChanger for the invariant culture.
		/// </summary>
		/// <value>The invariant culture.</value>
		public static CultureChanger InvariantCulture {
			get { return new CultureChanger(String.Empty); }
		}

		private readonly CultureInfo _oldCulture;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CultureChanger"/> class.
		/// </summary>
		/// <param name="culture">The culture.</param>
		public CultureChanger(CultureInfo culture) {
			_oldCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = culture;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CultureChanger"/> class.
		/// </summary>
		/// <param name="culture">The culture.</param>
		public CultureChanger(string culture):this(new CultureInfo(culture)) {}
		
		#region IDisposable Members

		public void Dispose() {
			Thread.CurrentThread.CurrentCulture = _oldCulture;
			GC.SuppressFinalize(this);
		}

		#endregion
	}
	#endregion

	#region UrlFormatter
    /*
	/// <summary>
	/// Supports Url encoded formatting of parameters, that can contain 
	/// encoding directives: {0:&lt;encoding&gt;}
	/// e.g. {0:euc-jp}
	/// </summary>
	public class UrlFormatter: IFormatProvider, ICustomFormatter {
		
		#region IFormatProvider Members

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof (ICustomFormatter)) {
				return this;
			}
			return null;
		}

		#endregion

		#region ICustomFormatter Members

		public string Format(string format, object arg, IFormatProvider formatProvider) {
			string s = arg as string;
			if (s == null) {
				return String.Empty;
			}

            return Uri.EscapeUriString(s);
            return String.Format("{0}", Uri.EscapeUriString(s));
            //if (format == null) {
            //    return String.Format("{0}", Uri.EscapeUriString(s));
            //}
            //try {
            //    Encoding encoding = Encoding.GetEncoding(format);
            //    return String.Format("{0}", System.Web.HttpUtility.UrlEncode(s, encoding));
            //}
            //catch (NotSupportedException) {
            //    return String.Format("{0}", Uri.EscapeUriString(s));
            //}
		}

		#endregion
	} */
	#endregion
    
	#region EventsHelper
	/// <summary>
	/// When publishing events in C#, you need to test that the delegate has targets. 
	/// You also must handle exceptions the subscribers throw, otherwise, the publishing 
	/// sequence is aborted. You can iterate over the delegate’s internal invocation list 
	/// and handle individual exceptions that way. 
	/// This generic helper class called EventsHelper that does just that. 
	/// EventsHelper can publish to any delegate, accepting any collection of parameters. 
	/// EventsHelper can also publish asynchronously and concurrently to the subscribers 
	/// using the thread pool, turning any subscriber’s target method into a fire-and-forget 
	/// method.
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
				asyncFire = InvokeDelegate;
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

	#region WebTabState

	public class SerializableWebTabState{
		
		[XmlArrayAttribute("urls")]
		[XmlArrayItemAttribute("url", Type = typeof(String), IsNullable = false)]
		public ArrayList Urls = new ArrayList();	
		

		/// <summary>
		/// Saves the SerializableWebTabState instance to specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="s"></param>
		public static void Save(Stream stream, SerializableWebTabState s) {
			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(
				typeof(SerializableWebTabState), RssBanditNamespace.BrowserTabState);
			serializer.Serialize(stream, s);
		}
		
		/// <summary>
		/// Loads the SerializableWebTabState from specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public static SerializableWebTabState Load(Stream stream) {
			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(
				typeof(SerializableWebTabState), RssBanditNamespace.BrowserTabState);
			return (SerializableWebTabState)serializer.Deserialize(stream); 
		}
		
	}

	internal class TextImageItem: ITextImageItem {
		private readonly Image image;
		private readonly string text;

		public TextImageItem(string text, Image image) {
			this.text = text;
			this.image = image;
		}

		#region ITextImageItem Members

		public Image Image {
			get { return this.image; }
		}

		public string Text {
			get { return this.text; }
		}

		#endregion
	}
	internal class WebTabState: ITabState {
		private static readonly ITextImageItem[] EmptyHistoryItems = new ITextImageItem[]{};
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
#if PHOENIX
		public ITextImageItem[] GoBackHistoryItems
		{
			get { return EmptyHistoryItems; }
			set {  }
		}

		public ITextImageItem[] GoForwardHistoryItems
		{
			get { return EmptyHistoryItems; }
			set {  }
		}
#endif
		public ITextImageItem CurrentHistoryItem
		{
			get; set;
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
#if !PHOENIX
        public ITextImageItem[] GoBackHistoryItems(int maxItems)
        {
            //TODO: impl. by IEControl
            return EmptyHistoryItems;
        }

        public ITextImageItem[] GoForwardHistoryItems(int maxItems)
        {
            //TODO: impl. by IEControl
            return EmptyHistoryItems;
        }
#endif
		#endregion

	}
	#endregion

	#region Utils
	internal class Utils {

		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(Utils));

		// some probe Urls, used by CurrentINetState() (no, that is NOT my favourites list... ;-)
		// They have better ping timings than all the other....
		private static string[] probeUrls = new string[]{
			"http://www.w3c.org/",	"http://www.google.com/",
			"http://www.heise.de/", "http://www.nyse.com/",
			"http://www.olm.net"
		};

		static Random probeUrlRandomizer = new Random();

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
		private static int fullInternetStateTestCounter;

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

			bool connected = false;
			
			try {
				connected = InternetGetConnectedState(out f, 0);
			} catch (Exception ex) {
				_log.Error("InternetGetConnectedState() API call failed with error: " + Marshal.GetLastWin32Error(), ex);
			}

			InternetStates flags =  (InternetStates)f;

			//_log.Info("InternetGetConnectedState() returned " + connected.ToString());

			//InternetCheckConnection(url, FLAG_ICC_FORCE_CONNECTION, 0) solves the wakness with Vista/Win7:
			////Some people have reported problems with return value of InternetGetConnectedState 
			////on Windows Vista
			//if (!connected && Win32.IsOSWindowsVista){
			//	connected = true;
			//}

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
					_log.Error("IsNetworkAlive() API call failed with error: " + Marshal.GetLastWin32Error(), ex);
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
					_log.Info("InternetGetConnectedState() flag INTERNET_CONNECTION_MODEM is set. Give up further tests...");
				}

			}
			
			state |= connected ? INetState.Connected: INetState.DisConnected;

			if (connected)
			{
				// also consider on-/offline state
				bool offline = ((flags & InternetStates.INTERNET_CONNECTION_OFFLINE) == InternetStates.INTERNET_CONNECTION_OFFLINE);
				state |= offline ? INetState.Offline: INetState.Online;
			}

			return state;
		}
		
		private static string GetProbeUrl() {
			return probeUrls[probeUrlRandomizer.Next(0, probeUrls.GetUpperBound(0))];
		}

		public static bool ApiCheckConnection(IWebProxy proxy) {
			//TODO: how about the proxy if we call the API function?
			string url = GetProbeUrl();	
			try {
				//_log.Info("ApiCheckConnection('"+url+"') ");
				if (InternetCheckConnection(url, FLAG_ICC_FORCE_CONNECTION, 0)) 
					return true;
			} catch (Exception ex) {
				_log.Error("ApiCheckConnection('"+url+"') failed with error: " + Marshal.GetLastWin32Error(), ex);
			}
			//_log.Info("ApiCheckConnection() returns false");
			return false;
		}

		public static bool FrameworkCheckConnection(IWebProxy proxy) {
			string url = GetProbeUrl();	

			if (proxy == null) 
				proxy = WebRequest.DefaultWebProxy; 
							
			try {
				//_log.Info("FrameworkCheckConnection('"+url+"') ");
				using (HttpWebResponse response = (HttpWebResponse)NewsComponents.Net.SyncWebRequest.GetResponseHeadersOnly(url, proxy, 3 * 60 * 1000)) {	
					if (response != null && String.Compare(response.Method, "HEAD") == 0) {	// success
						response.Close();
						return true;
					}
				}
			} catch (WebException ex) {
				_log.Error("FrameworkCheckConnection('"+url+"') ", ex);
				if (ex.Status == WebExceptionStatus.Timeout)
					return true;	// try again later on another probeUrl maybe
			} catch (Exception ex) {
				_log.Error("FrameworkCheckConnection('"+url+"') ", ex);
			}

			//_log.Info("FrameworkCheckConnection() returns false");
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

		#region RefreshRateStrings 
		
		private static List<string> _refreshRateStrings;
		/// <summary>
		/// Gets the refresh rate string captions as a list.
		/// </summary>
		/// <value>The list of strings.</value>
		public static List<string> RefreshRateStrings
		{
			get
			{
				if (_refreshRateStrings == null)
				{
					_refreshRateStrings = new List<string>(15);
					_refreshRateStrings.AddRange(
						new[]{"0", "15", "30", "45", "60", "75", "90","105", "120", "240", "480", "720", "1440"});
				}
				return _refreshRateStrings;
			}
		}
		#endregion

		#region MaxItemAgeStrings

		private static List<string> _maxItemAgeStrings;
		/// <summary>
		/// Gets the max. item age resource captions as a list.
		/// </summary>
		/// <value>The max. item age strings.</value>
		public static List<string> MaxItemAgeStrings
		{
			get
			{
				if (_maxItemAgeStrings == null)
				{
					_maxItemAgeStrings = new List<string>(25);
					_maxItemAgeStrings.AddRange(
						new[]
						{
							DR.ComboMaxItemAge_Item,
							DR.ComboMaxItemAge_Item1,
							DR.ComboMaxItemAge_Item2,
							DR.ComboMaxItemAge_Item3,
							DR.ComboMaxItemAge_Item4,
							DR.ComboMaxItemAge_Item5,
							DR.ComboMaxItemAge_Item6,
							DR.ComboMaxItemAge_Item7,
							DR.ComboMaxItemAge_Item8,
							DR.ComboMaxItemAge_Item9,
							DR.ComboMaxItemAge_Item10,
							DR.ComboMaxItemAge_Item11,
							DR.ComboMaxItemAge_Item12,
							DR.ComboMaxItemAge_Item13,
							DR.ComboMaxItemAge_Item14,
							DR.ComboMaxItemAge_Item15,
						});
				}
				return _maxItemAgeStrings;
			}
		}
			
		private static int[] dayIndexMap = new int[]{1,2,3,4,5,6,7,14,21,30,60,90,180,270,365};
		
		public static TimeSpan MaxItemAgeFromIndex(int index)
		{
			if (index < 0) {
				return TimeSpan.Zero;
			}
			if (index > dayIndexMap.Length-1) {
				return TimeSpan.MinValue;	// unlimited
			}
			return TimeSpan.FromDays(dayIndexMap[index]);
		}

		public static int MaxItemAgeToIndex(TimeSpan timespan) {
			int maxItemAgeDays = Math.Abs(timespan.Days);
			if (maxItemAgeDays <= dayIndexMap[6]) {	// 0..7 days
				// Need to ensure that we return a positive value
				if (maxItemAgeDays == 0) {
					return 0;
				}
				return  maxItemAgeDays - 1;
			}
			if (maxItemAgeDays > dayIndexMap[6] && maxItemAgeDays <= dayIndexMap[7]) {
				return  7;	// 14 days
			}
			if (maxItemAgeDays > dayIndexMap[7] && maxItemAgeDays <= dayIndexMap[8]) {
				return  8;	// 21 days
			}
			if (maxItemAgeDays > dayIndexMap[8] && maxItemAgeDays <= dayIndexMap[9]) {
				return  9;	// 1 month
			}
			if (maxItemAgeDays > dayIndexMap[9] && maxItemAgeDays <= dayIndexMap[10]) {
				return  10;	// 2 month
			}
			if (maxItemAgeDays > dayIndexMap[10] && maxItemAgeDays <= dayIndexMap[11]) {
				return  11;	// 1 quarter
			}
			if (maxItemAgeDays > dayIndexMap[11] && maxItemAgeDays <= dayIndexMap[12]) {
				return  12;	// 2 quarter
			}
			if (maxItemAgeDays > dayIndexMap[12] && maxItemAgeDays <= dayIndexMap[13]) {
				return  13;	// 3 quarter
			}
			if (maxItemAgeDays > dayIndexMap[13] && maxItemAgeDays <= dayIndexMap[14]) {
				return  14;	// 1 year
			}
			if (maxItemAgeDays > dayIndexMap[14] || timespan.Equals(TimeSpan.MinValue)) {
				return  15; // unlimited
			}
			return   9;	// 30 days, one month
		}

		#endregion

		#region RssSearchItemAgeStrings

		private static List<string> _rssSearchItemAgeStrings;
		/// <summary>
		/// Gets the RSS search item age resource captions as a list.
		/// </summary>
		/// <value>The RSS search item age strings.</value>
		public static List<string> RssSearchItemAgeStrings
		{
			get
			{
				if (_rssSearchItemAgeStrings == null)
				{
					_rssSearchItemAgeStrings = new List<string>(25);
					_rssSearchItemAgeStrings.AddRange(new[]
					                                  {
					                                  	SR.SearchPanel_comboRssSearchItemAge_1_hour,
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,2),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,2),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,3),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,4),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,5),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,6),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,12),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_hours,18),
					                                  	SR.SearchPanel_comboRssSearchItemAge_1_day,
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,2),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,3),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,4),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,5),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,6),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,7),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,14),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_days,21),
					                                  	SR.SearchPanel_comboRssSearchItemAge_1_month,
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_months,2),
					                                  	SR.SearchPanel_comboRssSearchItemAge_1_quarter,
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_quarters,2),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_quarters,3),
					                                  	SR.SearchPanel_comboRssSearchItemAge_1_year,
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_years,2),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_years,3),
					                                  	String.Format(SR.SearchPanel_comboRssSearchItemAge_x_years,5)
					                                  });
				}
				return _rssSearchItemAgeStrings;
			}
		}

		/// <summary>
		/// Maps the RSS search item age list index to a TimeSpan.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public static TimeSpan RssSearchItemAgeToTimeSpan(int index) {
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

		/// <summary>
		/// Maps the RSS search item age TimeSpan to a list index.
		/// </summary>
		/// <param name="age">The age.</param>
		/// <returns></returns>
		public static int RssSearchItemAgeToIndex(TimeSpan age) {
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

		#endregion
		
	}// Utils

	#endregion

    #region CachedImageLocator
    /// <summary>
    /// Helper class used to locate cached versions of images for use in offline mode
    /// </summary>
    internal class CachedImageLocater
    {
        /// <summary>
        /// Returns the location of the matched URL from the local IE cache. If the file isn't found in the cache then it returns 
        /// the original URL. 
        /// </summary>
        /// <param name="m">The image URL</param>
        /// <returns>The location of the image from the browser cache or the original URL if not cached</returns>
        internal string GetCachedImageLocation(System.Text.RegularExpressions.Match m)
        {
            string src = m.Groups[1].ToString();
            //handle case where regex starts or ends with quote character
            string test = src.Trim(new char[] { '"', '\'' });

            ArrayList results = WinInetAPI.FindUrlCacheEntries(test);

            if(results.Count == 0){ 
                return m.Groups[0].ToString();
            }

            WinInetAPI.INTERNET_CACHE_ENTRY_INFO entry = (WinInetAPI.INTERNET_CACHE_ENTRY_INFO) results[0];
            return m.Groups[0].ToString().Replace(src, entry.lpszLocalFileName);
        }
    }

    #endregion 

	#region FinderSearchNodes
	[Serializable]
	public class FinderSearchNodes {
		
		[XmlArrayItem(typeof(RssFinder))]
		public ArrayList RssFinderNodes = new ArrayList(2);
		
		public FinderSearchNodes(TreeFeedsNodeBase[] nodes) {
			foreach (TreeFeedsNodeBase node in nodes) {
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
		private void GetFinders(TreeFeedsNodeBase startNode) {
			if (startNode == null)
				return;
			if (!startNode.HasNodes) {
				FinderNode agn = startNode as FinderNode;
				if (agn != null)
					this.RssFinderNodes.Add(agn.Finder);
			} else {
				foreach (TreeFeedsNodeBase node in startNode.Nodes) {
					this.GetFinders(node);
				}
			}
		}
		
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
		/// feed Urls. The callback should return a array of NewsFeed, that contains
		/// the valid scope set. 
		/// </summary>
		public delegate INewsFeed[] SearchScopeResolveCallback(ArrayList categoryPaths, ArrayList feedUrls);

		#region private ivars
		private SearchCriteriaCollection searchCriterias;
		private INewsFeed[] searchScope = new INewsFeed[]{};
		private ArrayList categoryPathScope, feedUrlScope;
		private SearchScopeResolveCallback resolve;
		private FinderNode container;
		private bool doHighlight, dynamicItemContent, dynamicItemContentChecked, isInitialized;
		private string fullpathname;
		private string externalSearchUrl, externalSearchPhrase;
		private bool externalResultMergedWithLocal;
		#endregion

		#region ctor's
		public RssFinder(){
			dynamicItemContentChecked = false;
			categoryPathScope = new ArrayList(1);
			feedUrlScope = new ArrayList(1);
			searchCriterias = new SearchCriteriaCollection();
			ShowFullItemContent = true;
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
		public bool IsPersisted {
			get { if (container != null) 
					  return !container.IsTempFinderNode; 
				return false;
			}	
		}
		
		[XmlIgnore]
		public string Text {
			get { 
				if (container != null)
					return container.Text;
				string[] a = fullpathname.Split(FeedSource.CategorySeparator.ToCharArray());
				return a[a.GetLength(0)-1];
			}
			set { 
				if (container != null)
					container.Text = value;
			}
		}

		public string FullPath {
			get
			{
				if (container != null) {
					string s = container.FullPath.Trim();
					string[] a = s.Split(FeedSource.CategorySeparator.ToCharArray());
					if (a.GetLength(0) > 1)
						return String.Join(FeedSource.CategorySeparator,a, 1, a.GetLength(0)-1);
			
					return s;	// name only
				}
				return fullpathname;
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

		[XmlArray("category-scopes"), XmlArrayItem("category", Type = typeof(String), IsNullable = false)]
		public ArrayList CategoryPathScope {
			get { return categoryPathScope;}
			set { categoryPathScope = value; }
		}
		[XmlArray("feedurl-scopes"), XmlArrayItem("feedurl", Type = typeof(String), IsNullable = false)]
		public ArrayList FeedUrlScope {
			get { return feedUrlScope;}
			set { feedUrlScope = value;  }
		}

		[XmlIgnore]
		public INewsFeed[] SearchScope {
			get { 
				RaiseScopeResolver();
				return searchScope;	
			}
			set { searchScope = value;  }
		}

		[XmlIgnore]
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

		[XmlIgnore]
		public bool ExternalResultMerged {
			get { return externalResultMergedWithLocal;	}
			set { externalResultMergedWithLocal = value;  }
		}
		[XmlIgnore]
		public string ExternalSearchUrl {
			get { return externalSearchUrl;	}
			set { externalSearchUrl = value;  }
		}
		[XmlIgnore()]
		public string ExternalSearchPhrase {
			get { return externalSearchPhrase;	}
			set { externalSearchPhrase = value;  }
		}

		[XmlIgnore]
		public FinderNode Container {
			get { return container;		}
			set { container = value;	}
		}

		[XmlIgnore]
		public SearchScopeResolveCallback ScopeResolver {
			get { return resolve;		}
			set { resolve = value;	}
		}

		[XmlAttribute("show-full-item-content"), System.ComponentModel.DefaultValue(true)]
		public bool ShowFullItemContent;

		/// <remarks/>
		[XmlAnyAttributeAttribute]
		public XmlAttribute[] AnyAttr;

		public void SetSearchScope(ArrayList categoryPathScopeList, ArrayList feedUrlScopeList) {
			this.categoryPathScope = categoryPathScopeList;
			this.feedUrlScope = feedUrlScopeList;
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

	#region CertificateHelper
	internal static class CertificateHelper
	{
		public static void ShowCertificate(X509Certificate2 certificate, IntPtr hwndParent)
		{
			if (certificate != null)
				X509Certificate2UI.DisplayCertificate(certificate);
		}

		public static void ShowCertificate(X509Certificate certificate)
		{
			if (certificate == null)
				return;

			string certFilename = Path.Combine(Path.GetTempPath(), certificate.GetHashCode() + ".temp.cer");

			try
			{
				if (File.Exists(certFilename))
					File.Delete(certFilename);

				using (Stream stream = FileHelper.OpenForWrite(certFilename))
				{
					var writer = new BinaryWriter(stream);
					writer.Write(certificate.GetRawCertData());
					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception ex)
			{
				ExceptionManager.Publish(ex);
				return;
			}

			try
			{
				if (File.Exists(certFilename))
				{
					Process p = Process.Start(certFilename);
					p.WaitForExit(); // to enble delete the temp file
				}
			}
			finally
			{
				if (File.Exists(certFilename))
					File.Delete(certFilename);
			}
		}

		public static X509Certificate2 SelectCertificate(string message)
		{
			try
			{
				X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
				X509Certificate2Collection fcollection = store.Certificates;
				
				X509Certificate2Collection collection =
					X509Certificate2UI.SelectFromCollection(
						fcollection,
						SR.Certificate_SelectionDialog_Title, message,
						X509SelectionFlag.SingleSelection);
				
				if (collection.Count > 0)
					return collection[0];
				
			} catch (Exception ex)
			{
				Log.Error("Failed to select a client certificate", ex);
				//RssBanditApplication.S
			}
			return null;
		}
	}
	#endregion

	#region WidgetFormatter
	
	public enum UiStyle
	{
		WindowsXP,
		WindowsVista,
		Office2003,
		Office2007,
		Default = Office2003,
	}

	/// <summary>
	/// Helper to style all controls (esp. Infragistics)
	/// in a unique manner. IG use different enums for each
	/// control but with very similar enum values on each of them.
	/// Goal is to allow a "switch" UI Style from XP/Vista (if no
	/// Office is installed) and Office 2003/2007.
	/// </summary>
	class WidgetFormatter
	{
		//TODO: (TR) stillwork in progress!!!
		private readonly UiStyle style;

		public WidgetFormatter(UiStyle style)
		{
			this.style = style;
		}

		public void Format(object c)
		{
			if (c is UltraToolbar)
				Format(c as UltraToolbar);
			if (c is UltraExplorerBar)
				Format(c as UltraExplorerBar);
			if (c is UltraStatusBar)
				Format(c as UltraStatusBar);
		}

		void Format(UltraToolbarBase t)
		{
			switch (style)
			{
				case UiStyle.WindowsXP:
					t.ToolbarsManager.Style = ToolbarStyle.OfficeXP;break;
				case UiStyle.WindowsVista:
					t.ToolbarsManager.Style = ToolbarStyle.WindowsVista;break;
				case UiStyle.Office2007:
					t.ToolbarsManager.Style = ToolbarStyle.Office2007;break;
				default:
					t.ToolbarsManager.Style = ToolbarStyle.Office2003;break;
			}
		}

		void Format(UltraExplorerBar t)
		{
			switch (style)
			{
				case UiStyle.WindowsXP:
					t.ViewStyle = UltraExplorerBarViewStyle.XP;break;
				case UiStyle.WindowsVista:
					t.ViewStyle = UltraExplorerBarViewStyle.Office2007;break;
				case UiStyle.Office2007:
					t.ViewStyle = UltraExplorerBarViewStyle.Office2007;break;
				default:
					t.ViewStyle = UltraExplorerBarViewStyle.Office2003;break;
			}
		}

		void Format(UltraStatusBar t)
		{
			switch (style)
			{
				case UiStyle.WindowsXP:
					t.ViewStyle = ViewStyle.Office2003; break;
				case UiStyle.WindowsVista:
					t.ViewStyle = ViewStyle.Office2007; break;
				case UiStyle.Office2007:
					t.ViewStyle = ViewStyle.Office2007; break;
				default:
					t.ViewStyle = ViewStyle.Office2003; break;
			}
		}
	}

	#endregion
}
