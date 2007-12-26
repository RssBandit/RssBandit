#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Represents a MIME Type.
	/// See also http://www.ietf.org/rfc/rfc2045.txt
	/// and http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/moniker/overview/appendix_a.asp
	/// </summary>
	[Serializable]
	public class MimeType {

		#region Private Members
		private string   msType;
		private string   msSubType;
		#endregion

		#region Constructors 
		/// <summary>Constructor.</summary>
		public MimeType() {
			msType = msSubType = String.Empty;
		}
		
		/// <summary>Constructor.</summary>
		/// <param name="contentType">Full MIME Content-Type string.</param>
		public MimeType( string contentType ) {	
			this.SplitTypeAndSubType( contentType, out msType, out msSubType );
		}

		/// <summary>Constructor.</summary>
		/// <param name="type">discrete-type or composite-type</param>
		/// <param name="subType">sub-Type</param>
		public MimeType( string type, string subType ) {	
			this.msType    = (type == null ? String.Empty: type);
			this.msSubType = (subType == null ? String.Empty: subType);
		}
		#endregion

		#region Public Statics
		/// <summary>
		/// Gets an empty MimeType instance.
		/// </summary>
		public static readonly MimeType Empty = new MimeType();

		#region CreateFrom dataFileName
		/// <summary>
		/// Gets the MIME type of a file.
		/// </summary>
		/// <param name="dataFileName">File name incl. path</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataFileName is null or empty</exception>
		public static MimeType CreateFrom(string dataFileName) {
			return CreateFrom(dataFileName, MimeType.Empty);
		}
		/// <summary>
		/// Gets the MIME type of a file.
		/// </summary>
		/// <param name="dataFileName">File name incl. path</param>
		/// <param name="mimeProposed">A proposed MIME content type string</param>
		/// <returns>MimeType</returns>
		/// <remarks>If mimeProposed is null or empty, also the file extension is used to detect the MIME type, 
		/// if it cannot be obtained from the file content</remarks>
		/// <exception cref="ArgumentNullException">if dataFileName is null or empty</exception>
		public static MimeType CreateFrom(string dataFileName, string mimeProposed) {
			return CreateFrom(dataFileName, new MimeType(mimeProposed));
		}

		/// <summary>
		/// Gets the MIME type of a file.
		/// </summary>
		/// <param name="dataFileName">File name incl. path</param>
		/// <param name="mimeProposed">A proposed MIME content type</param>
		/// <returns>MimeType</returns>
		/// <remarks>If mimeProposed is null or empty, also the file extension is used to detect the MIME type, 
		/// if it cannot be obtained from the file content</remarks>
		/// <exception cref="ArgumentNullException">if dataFileName is null or empty</exception>
		public static MimeType CreateFrom(string dataFileName, MimeType mimeProposed) {

			if (StringHelper.EmptyOrNull(dataFileName))
				throw new ArgumentNullException("dataFileName");
			
			MimeType mimeRet = MimeType.Empty;

			if (mimeProposed != null) {
				//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed);
				mimeRet = mimeProposed;
			} else {
				mimeRet = MimeType.CreateFromFileExt(dataFileName);
			} 


			// if we can sniff the content, analyze the data contained:
			if (File.Exists(dataFileName)) {
				try {
					using(Stream f = FileHelper.OpenForRead(dataFileName)) {
						return CreateFrom(f, mimeRet);
					}
				} catch (Exception) { /* ignore, continue with conservative detection */ }
			}

			string fileUri = dataFileName;
			try {
				if (fileUri.IndexOf(@"://") < 0)
					fileUri = new Uri(dataFileName).ToString();
			} catch (UriFormatException) {}

			int ret = -1;
			IntPtr suggestPtr = IntPtr.Zero, filePtr = IntPtr.Zero, outPtr = IntPtr.Zero;

			try {
				// see also: http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/moniker/reference/functions/findmimefromdata.asp
				// and: http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/moniker/overview/appendix_a.asp
				filePtr = Marshal.StringToCoTaskMemUni(fileUri);
				ret = FindMimeFromData(IntPtr.Zero , filePtr, null, 0, suggestPtr, 0, out outPtr, 0);
			} catch (Exception) { /* if any, ignore. We should get failure report via "ret" */ }

			if (ret == 0 && outPtr != IntPtr.Zero) {
				return new MimeType(Marshal.PtrToStringUni(outPtr));
			}

			return mimeRet;
		}
		#endregion

		#region CreateFrom stream
		/// <summary>
		/// Gets the MIME type of a stream.
		/// </summary>
		/// <param name="dataStream">Stream to read</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		/// <exception cref="IOException">On IO stream errors</exception>
		/// <exception cref="NotSupportedException">If stream do not support reading
		/// or is not seekable</exception>
		/// <exception cref="InvalidOperationException">If the stream contains no readable data (length zero)</exception>
		public static MimeType CreateFrom(Stream dataStream) {
			return MimeType.CreateFrom(dataStream, MimeType.Empty);
		}
		/// <summary>
		/// Gets the MIME type of a stream.
		/// </summary>
		/// <param name="dataStream">Stream to read</param>
		/// <param name="mimeProposed">A proposed MIME content type string</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		/// <exception cref="IOException">On IO stream errors</exception>
		/// <exception cref="NotSupportedException">If stream do not support reading
		/// or is not seekable</exception>
		/// <exception cref="InvalidOperationException">If the stream contains no readable data (length zero)</exception>
		public static MimeType CreateFrom(Stream dataStream, string mimeProposed) {
			return CreateFrom(dataStream, new MimeType(mimeProposed));
		}

		/// <summary>
		/// Gets the MIME type of a stream.
		/// </summary>
		/// <param name="dataStream">Stream to read</param>
		/// <param name="mimeProposed">A proposed MIME content type</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		/// <exception cref="IOException">On IO stream errors</exception>
		/// <exception cref="NotSupportedException">If stream do not support reading
		/// or is not seekable</exception>
		/// <exception cref="InvalidOperationException">If the stream contains no readable data (length zero)</exception>
		public static MimeType CreateFrom(Stream dataStream, MimeType mimeProposed) {
			if (dataStream == null)
				throw new ArgumentNullException("dataStream");
			
			MimeType mimeRet = (mimeProposed != null ? mimeProposed : MimeType.Empty);
			
			long oldPosition = dataStream.Position;
			// if (dataStream.CanSeek) DO NOT so, bubble up exception(s)
			dataStream.Seek(0, SeekOrigin.Begin);
			
			byte[] bValues = new byte[256];
			int bytesRead = 0;
			bytesRead = dataStream.Read(bValues, 0, bValues.Length);

			// reset to prev. position
			dataStream.Seek(oldPosition, SeekOrigin.Begin);

			if (bytesRead > 0)
				return CreateFrom(bValues, mimeRet);
			else
				throw new InvalidOperationException("Cannot analyze stream content (length Zero)");
		}

		#endregion

		#region CreateFrom bytes

		/// <summary>
		/// Gets the MIME type of a byte array.
		/// </summary>
		/// <param name="dataBytes">Byte array with the content</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		public static MimeType CreateFrom(byte[] dataBytes) {
			return MimeType.CreateFrom(dataBytes, MimeType.Empty);
		}
		/// <summary>
		/// Gets the MIME type of a byte array.
		/// </summary>
		/// <param name="dataBytes">Byte array with the content</param>
		/// <param name="mimeProposed">A proposed MIME content type string</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		public static MimeType CreateFrom(byte[] dataBytes, string mimeProposed) {
			return CreateFrom(dataBytes, new MimeType(mimeProposed));
		}
		/// <summary>
		/// Gets the MIME type of a byte array.
		/// </summary>
		/// <param name="dataBytes">Byte array with the content</param>
		/// <param name="mimeProposed">A proposed MIME content type</param>
		/// <returns>MimeType</returns>
		/// <exception cref="ArgumentNullException">if dataStream is null</exception>
		public static MimeType CreateFrom(byte[] dataBytes, MimeType mimeProposed) {
			if (dataBytes == null || dataBytes.Length == 0)
				throw new ArgumentNullException("dataBytes");

			MimeType mimeRet = (mimeProposed != null ? mimeProposed : MimeType.Empty);

			string sMimeType = null;
			MimeType mimeFnd = MimeType.Empty;
			IntPtr suggestPtr = IntPtr.Zero, outPtr = IntPtr.Zero;

			int ret = FindMimeFromData(IntPtr.Zero , IntPtr.Zero, dataBytes, dataBytes.Length, suggestPtr, 0, out outPtr, 0);

			if (ret == 0 && outPtr != IntPtr.Zero) {
				sMimeType = Marshal.PtrToStringUni(outPtr);
			}

			if (IsAmbigious(sMimeType)) {	// do some more analyzing:
				if (BytesEqual(_magic_write_sign, dataBytes, 0, _magic_write_sign.Length))
					sMimeType = "application/richtext";	// MS Write format
				if (BytesEqual(_magic_rtf_sign, dataBytes, 0, _magic_rtf_sign.Length))
					sMimeType =  "text/richtext";	// RTF format 
				if (BytesEqual(_magic_old_word_sign, dataBytes, 0, _magic_old_word_sign.Length)) 
					sMimeType = "application/msword";	// RTF format
				if (BytesEqual(_magic_ole_sign, dataBytes, 0, _magic_ole_sign.Length)) {
					if (IsOfficeMimeType(mimeRet.ContentType))
						return mimeRet;	// ignore the ambigious mimetype and return the suggested, if office
				}
				if (BytesEqual(_magic_ms_jetdb, dataBytes, 0, _magic_ms_jetdb.Length)) 
					sMimeType = "application/msaccess";	// binary mdb format
				if (BytesEqual(_magic_ms_onenote, dataBytes, 0, _magic_ms_onenote.Length)) 
					sMimeType = "application/msonenote";	// binary onenote format

			}

			mimeFnd = new MimeType(sMimeType);
			if (mimeFnd != MimeType.Empty && !mimeFnd.Equals(mimeRet))
				return mimeFnd;

			return mimeRet;
		}

		#endregion
		
		#region other common CreateFromXXX functions
		/// <summary>
		/// Get a MimeType from a provided file extension.
		/// </summary>
		/// <param name="fileNameExt">File name incl. extension or
		/// extension only: e.g. ".pdf"</param>
		/// <returns>MimeType</returns>
		public static MimeType CreateFromFileExt(string fileNameExt ) {

			// Check/Fix FileNameExt.
			if (StringHelper.EmptyOrNull(fileNameExt))
				return MimeType.Empty;

			string ext = Path.GetExtension(fileNameExt);
			if (StringHelper.EmptyOrNull(ext))
				return MimeType.Empty;

			MimeType m = CreateFromRegisteredApps(ext);
			if (m != MimeType.Empty)
				return m;

			// fall back
			switch(ext) {
				case ".rtf": {
					return new MimeType( "text", "richtext" );
				}
				case ".htm":
				case ".html": {
					return new MimeType( "text", "html" );
				}
				case ".aif":
				case ".aiff": {
					return new MimeType( "audio", "x-aiff" );
				}
				case ".bas":
				case ".basic": {
					return new MimeType( "audio", "basic" );
				}
				case ".wav":
				case ".wave": {
					return new MimeType( "audio", "wav" );
				}
				case ".gif": {
					return new MimeType( "image", "gif" );
				}
				case ".jpeg": {
					return new MimeType( "image", "jpeg" );
				}
				case ".pjpeg": {
					return new MimeType( "image", "pjpeg" );
				}
				case ".tiff": {
					return new MimeType( "image", "tiff" );
				}
				case ".x-png": {
					return new MimeType( "image", "x-png" );
				}
				case ".x-xbitmap": {
					return new MimeType( "image", "x-xbitmap" );
				}
				case ".bmp": {
					return new MimeType( "image", "bmp" );
				}
				case ".jg": {
					return new MimeType( "image", "x-jg" );
				}
				case ".emf": {
					return new MimeType( "image", "x-emf" );
				}
				case ".wmf": {
					return new MimeType( "image", "x-wmf" );
				}
				case ".avi": {
					return new MimeType( "video", "avi" );
				}
				case ".mpeg": {
					return new MimeType( "video", "mpeg" );
				}
				case ".ps":
				case ".postscript": {
					return new MimeType( "application", "postscript" );
				}
				case ".base64": {
					return new MimeType( "application", "base64" );
				}
				case ".macbinhex40": {
					return new MimeType( "application", "macbinhex40" );
				}
				case ".pdf": {
					return new MimeType( "application", "pdf" );
				}
				case ".x-compressed": {
					return new MimeType( "application", "x-compressed" );
				}
				case ".zip": {
					return new MimeType( "application", "x-zip-compressed" );
				}
				case ".gzip": {
					return new MimeType( "application", "x-gzip-compressed" );
				}
				case ".java": {
					return new MimeType( "application", "java" );
				}
				case ".msdownload ": {
					return new MimeType( "application", "x-msdownload" );
				}
				case ".cdf ": {
					return new MimeType( "application", "x-cdf" );
				}
			}
			//------------------------------------------------------------------
			return MimeType.Empty;
		}

		/// <summary>
		/// Creates a MimeType for a file extension using the OS registered applications 
		/// for that file type.
		/// </summary>
		/// <param name="fileExtension">string (e.g. "pdf" or ".pdf")</param>
		/// <returns>MimeType</returns>
		/// <permission cref="RegistryPermission">Read access to HKEY_CLASSES_ROOT\MIME</permission>
		public static MimeType CreateFromRegisteredApps(string fileExtension) {
			if (fileExtension == null)
				return MimeType.Empty;

			string dotExt = (fileExtension.IndexOf(".") >= 0 ? fileExtension: "." + fileExtension);
			string mType = WindowsRegistry.GetMimeTypeString(dotExt);
			if (mType == null)
				return MimeType.Empty;
			
			return new MimeType(mType);
		}
		#endregion

		#endregion

		#region Public Properties 
		/// <summary>
		/// Gets/Sets the MIME type (discrete-type or composite-type)
		/// </summary>
		public string Type {
			set {	msType = value;	}
			get {	return msType;	}
		}

		/// <summary>
		/// Gets/Sets the MIME sub-Type.
		/// </summary>
		public string SubType {
			set { 	msSubType = value;	}
			get {	return msSubType;	}
		}

		/// <summary>
		/// Gets/Sets the Full MIME Content-Type string.
		/// </summary>
		public string ContentType {
			set { SplitTypeAndSubType( value, out msType, out msSubType ); }
			get { return msType + "/" + msSubType; }
		}

		/// <summary>
		/// Gets the CLSID for this MimeType instance.
		/// </summary>
		/// <returns>CLSID (string), or null</returns>
		public string GetCLSID() {
			if (msType == null || msType.Length == 0)
				return null;
			return WindowsRegistry.GetMimeTypeOption(ContentType, "CLSID");
		}

		/// <summary>
		/// Gets the file extension for this MimeType instance.
		/// </summary>
		/// <returns>File extension (string) if found/available, else null</returns>
		public string GetFileExtension() {
			if (msType == null || msType.Length == 0)
				return null;
			return WindowsRegistry.GetMimeTypeOption(ContentType, "Extension");
		}

		/// <summary>
		/// Returns true, if the content of the provided fileName
		/// matches this MimeType. 
		/// You can use this to check if the content of a file matches
		/// this mimetype. This check will NOT take the file extension
		/// into account!
		/// </summary>
		/// <param name="fileName">string</param>
		/// <exception cref="ArgumentNullException">If fileName is null or length of 0</exception>
		/// <returns>true if the file content match this mimetype, else false</returns>
		public bool MatchContentOf(string fileName) {
			if (StringHelper.EmptyOrNull(fileName))
				throw new ArgumentNullException("fileName");
			return (MimeType.CreateFrom(fileName, this.ContentType).Equals(this));
		}
		/// <summary>
		/// Returns true, if the content of the provided stream
		/// matches this MimeType. 
		/// You can use this to check if the content of a stream matches
		/// this mimetype.
		/// </summary>
		/// <remarks>The provided stream must be seekable for this check
		/// to succeed.</remarks>
		/// <param name="stream">Stream</param>
		/// <exception cref="ArgumentNullException">If stream is null</exception>
		/// <returns>true if the stream content match this mimetype, else false</returns>
		public bool MatchContentOf(Stream stream) {
			if (stream == null)
				throw new ArgumentNullException("stream");
			return (MimeType.CreateFrom(stream, this.ContentType).Equals(this));
		}
		/// <summary>
		/// Returns true, if the content of the provided byte array
		/// matches this MimeType. 
		/// You can use this to check if the content of a byte array matches
		/// this mimetype.
		/// </summary>
		/// <param name="bytes">byte[]</param>
		/// <exception cref="ArgumentNullException">If bytes are null or length of 0</exception>
		/// <returns>true if the byte array content match this mimetype, else false</returns>
		public bool MatchContentOf(byte[] bytes) {
			if (bytes == null || bytes.Length == 0)
				throw new ArgumentNullException("bytes");
			return (MimeType.CreateFrom(bytes, this.ContentType).Equals(this));
		}

		#endregion

		#region  Public Methods 
		public override string ToString() {
			return this.ContentType;
		}

		public override bool Equals(object obj) {
			
			if(Object.ReferenceEquals(this, obj)) { return true; }

			MimeType item = obj as MimeType; 

			if(item == null) { return false; }
			
			if (this.ContentType.Equals(item.ContentType)) {
				return true;
			}

			return false; 									
		}

		public override int GetHashCode() {
			if (this.ContentType != null)
				return this.ContentType.GetHashCode ();
			return base.GetHashCode();
		}

		#endregion

		#region Private support functions 
		private void SplitTypeAndSubType( string contentType, out string sType , out string sSubType ) {
			sType  = sSubType  = String.Empty;
			// this construct ensure, it works also with contentType == null, and not containing any "/":
			string[] sArray = String.Concat(contentType, "/").Split(new char[]{'/'});
			sType    = sArray[0].Trim();
			sSubType = sArray[1].Trim();
		}

		#region own analyze definitions for word/excel/ppt
		static string _binaryMime = "application/octet-stream";
		static string _textMime = "text/plain";
		static byte[] _magic_ole_sign = new byte[]{ 0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1};
		static byte[] _magic_rtf_sign = new byte[]{(byte)'{', (byte)'\\', (byte)'r', (byte)'t', (byte)'f'};
		static byte[] _magic_old_word_sign =new byte[]{0xdb,0xa5};
		static byte[] _magic_write_sign =new byte[]{0x31,0xBE};
		static byte[] _magic_ms_jetdb = new byte[]{0x0, 0x01, 0x0, 0x0, (byte)'S', (byte)'t', (byte)'a', (byte)'n', (byte)'d', (byte)'a', (byte)'r', (byte)'d', (byte)' ', (byte)'J', (byte)'e', (byte)'t', (byte)' ', (byte)'D', (byte)'B'};
		static byte[] _magic_ms_onenote = new byte[]{0xE4, 0x52, 0x5C, 0x7B, 0x8C, 0xD8, 0xA7, 0x4D, 0xAE, 0xB1, 0x53, 0x78, 0xD0, 0x29, 0x96,  0xD3};
		#endregion

		private static bool IsAmbigious(string mimeType) {
			if (StringHelper.EmptyOrNull(mimeType))
				return true;
			if (_binaryMime.Equals(mimeType) || _textMime.Equals(mimeType))
				return true;
			return false;
		}

		private static bool IsOfficeMimeType(string mimeType) {
			if (StringHelper.EmptyOrNull(mimeType))
				return false;
			if (mimeType.IndexOf("msword") > 0)
				return true;
			if (mimeType.IndexOf("msexcel") > 0)
				return true;
			if (mimeType.IndexOf("mspowerpoint") > 0)
				return true;
			if (mimeType.IndexOf("ms-publisher") > 0)
				return true;
			if (mimeType.IndexOf("ms-powerpoint") > 0)
				return true;
			if (mimeType.IndexOf("ms-excel") > 0)
				return true;
			return false;
		}

		private static bool BytesEqual(byte[] a, byte[] b, int offset, int length) {
			int lena = a.Length, lenb = b.Length;
			for (int i=offset; i < offset+length && i < lena & i < lenb; i++) {
				if (a[i] != b[i])
					return false; 
			}
			return true;
		}

		#region Interop
		/// <summary>
		/// Determines the Multipurpose Internet Mail Extensions (MIME) type 
		/// from the data provided.
		/// </summary>
		/// <param name="pBC">Pointer to the bind context. This can be set to NULL. </param>
		/// <param name="pwzUrl">Pointer to a string value that contains the URL of the data. 
		/// This can be set to NULL if pBuffer contains the data to be sniffed.</param>
		/// <param name="pBuffer">Pointer to the buffer containing the data to be sniffed. 
		/// This can be set to NULL if pwzUrl contains a valid URL. </param>
		/// <param name="cbSize">Unsigned long integer value that contains the size of the buffer</param>
		/// <param name="pwzMimeProposed">Pointer to a string value containing the proposed MIME type. 
		/// This can be set to NULL.</param>
		/// <param name="dwMimeFlags">Reserved. Must be set to 0.</param>
		/// <param name="ppwzMimeOut">Address of a string value containing the suggested MIME type.</param>
		/// <param name="dwReserved">Reserved. Must be set to 0.</param>
		/// <returns>
		/// Returns one of the following values.
		///   * E_INVALIDARG One or more of the arguments passed to the function were invalid. 
		///   * E_OUTOFMEMORY The function could not allocate enough memory to complete 
		///   the call. 
		///   * NOERROR The call was completed successfully. 
		/// </returns>
		[DllImport("urlmon.dll", CharSet=CharSet.Auto)]
		private static extern int FindMimeFromData (
			IntPtr pBC,
			IntPtr pwzUrl,
			byte[] pBuffer,
			int cbSize,
			IntPtr pwzMimeProposed,
			int dwMimeFlags,
			out IntPtr ppwzMimeOut,
			int dwReserved );
		#endregion

		#endregion

		#region Registry stuff
		
		/// <summary>
		/// Wrap the windows registry access needed for MimeType
		/// </summary>
		class WindowsRegistry {

			/// <summary>
			/// Gets the mimetype (string) for an provided 
			/// file extension (incl. the leading point, e.g. ".pdf")
			/// </summary>
			/// <param name="fileExtension">string (incl. the leading point, e.g. ".pdf")</param>
			/// <returns>Non empty string if found, else null</returns>
			/// <permission cref="RegistryPermission">Read access to HKEY_CLASSES_ROOT\MIME</permission>
			public static string GetMimeTypeString(string fileExtension) {
				if (fileExtension == null)
					return null;

				if (null == AquireReadAccess(@"HKEY_CLASSES_ROOT\MIME"))
					return null;

				RegistryKey typeKey = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type", false);
				if (typeKey == null)
					return null;
			
				CultureInfo c = CultureInfo.InvariantCulture;
				foreach (string keyname in typeKey.GetSubKeyNames()) {
					RegistryKey curKey = typeKey.OpenSubKey(keyname);
					string val = curKey.GetValue("Extension") as string;
					curKey.Close();
					if (String.Compare(fileExtension, val, true, c) == 0) {
						typeKey.Close();
						return keyname;
					}
				}

				typeKey.Close();
				return null;

			}

			/// <summary>
			/// Gets the mimetype option (string) for an provided 
			/// mimetype.
			/// </summary>
			/// <param name="mimeType">string. The mimetype to return an option for</param>
			/// <param name="option">string (incl. the leading point, e.g. ".pdf")</param>
			/// <returns>Non empty string if found, else null</returns>
			/// <permission cref="RegistryPermission">Read access to HKEY_CLASSES_ROOT\MIME</permission>
			public static string GetMimeTypeOption(string mimeType, string option) {
				if (option == null || mimeType == null)
					return null;

				if (null == AquireReadAccess(@"HKEY_CLASSES_ROOT\MIME"))
					return null;

				RegistryKey typeKey = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
				if (typeKey == null)
					return null;
			
				string val = typeKey.GetValue(option) as string;
				typeKey.Close();
				return val;

			}

			private static RegistryPermission AquireReadAccess(string hive) {
				RegistryPermission regPerm = new RegistryPermission(RegistryPermissionAccess.Read, hive);
				regPerm.Demand();
				return regPerm;
			}

		}
		#endregion
	}

}

