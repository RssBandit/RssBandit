using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

///<summary>
///Encapsulates access to alternative data streams of an NTFS file.
///Adapted from a C++ sample by Dino Esposito,
///http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnfiles/html/ntfs5.asp
///</summary>
namespace NewsComponents.Utils {
	/// <summary>
	/// Wraps the API functions, structures and constants.
	/// </summary>
	internal class Kernel32 
	{
		public const char STREAM_SEP = ':';
		public const int INVALID_HANDLE_VALUE = -1;
		public const int MAX_PATH = 256;
		
		[Flags()] public enum FileFlags : uint
		{
			WriteThrough = 0x80000000,
			Overlapped = 0x40000000,
			NoBuffering = 0x20000000,
			RandomAccess = 0x10000000,
			SequentialScan = 0x8000000,
			DeleteOnClose = 0x4000000,
			BackupSemantics = 0x2000000,
			PosixSemantics = 0x1000000,
			OpenReparsePoint = 0x200000,
			OpenNoRecall = 0x100000
		}

		[Flags()] public enum FileAccessAPI : uint
		{
			GENERIC_READ = 0x80000000,
			GENERIC_WRITE = 0x40000000
		}
		/// <summary>
		/// Provides a mapping between a System.IO.FileAccess value and a FileAccessAPI value.
		/// </summary>
		/// <param name="Access">The <see cref="System.IO.FileAccess"/> value to map.</param>
		/// <returns>The <see cref="FileAccessAPI"/> value.</returns>
		public static FileAccessAPI Access2API(FileAccess Access) 
		{
			FileAccessAPI lRet = 0;
			if ((Access & FileAccess.Read)==FileAccess.Read) lRet |= FileAccessAPI.GENERIC_READ;
			if ((Access & FileAccess.Write)==FileAccess.Write) lRet |= FileAccessAPI.GENERIC_WRITE;
			return lRet;
		}

		[StructLayout(LayoutKind.Sequential)] public struct LARGE_INTEGER 
		{
			public int Low;
			public int High;

			public long ToInt64() 
			{
				return (long)High * 4294967296 + (long)Low;
			}
		}

		[StructLayout(LayoutKind.Sequential)] public struct WIN32_STREAM_ID 
		{
			public int dwStreamID;
			public int dwStreamAttributes;
			public LARGE_INTEGER Size;
			public int dwStreamNameSize;
		}
		
		[DllImport("kernel32")] public static extern IntPtr CreateFile(string Name, FileAccessAPI Access, FileShare Share, int Security, FileMode Creation, FileFlags Flags, int Template);
		[DllImport("kernel32")] public static extern bool DeleteFile(string Name);
		[DllImport("kernel32")] public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32")] public static extern bool BackupRead(IntPtr hFile, IntPtr pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref int Context);
		[DllImport("kernel32")] public static extern bool BackupRead(IntPtr hFile, ref WIN32_STREAM_ID pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref int Context);
		[DllImport("kernel32")] public static extern bool BackupSeek(IntPtr hFile, int dwLowBytesToSeek, int dwHighBytesToSeek, ref int dwLow, ref int dwHigh, ref int Context);
	}

	/// <summary>
	/// Encapsulates a single alternative data stream for a file.
	/// </summary>
	public class StreamInfo 
	{
		private FileStreams _parent;
		private string _name;
		private long _size;

		internal StreamInfo(FileStreams Parent, string Name, long Size) 
		{
			_parent = Parent;
			_name = Name;
			_size = Size;
		}

		/// <summary>
		/// The name of the stream.
		/// </summary>
		public string Name 
		{
			get { return _name; }
		}
		/// <summary>
		/// The size (in bytes) of the stream.
		/// </summary>
		public long Size 
		{
			get { return _size; }
		}
		
		public override string ToString() 
		{
			return String.Format("{1}{0}{2}{0}$DATA", Kernel32.STREAM_SEP, _parent.FileName, _name);
		}
		public override bool Equals(Object o) 
		{
			if (o is StreamInfo) 
			{
				StreamInfo f = (StreamInfo)o;
				return (f._name.Equals(_name) && f._parent.Equals(_parent));
			}
			else if (o is string) 
			{
				return ((string)o).Equals(ToString());
			}
			else
				return base.Equals(o);
		}
		public override int GetHashCode() 
		{
			return ToString().GetHashCode();
		}

#region Open
		/// <summary>
		/// Opens or creates the stream in read-write mode, with no sharing.
		/// </summary>
		/// <returns>A <see cref="System.IO.FileStream"/> wrapper for the stream.</returns>
		public FileStream Open() 
		{
			return Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}
		/// <summary>
		/// Opens or creates the stream in read-write mode with no sharing.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> action for the stream.</param>
		/// <returns>A <see cref="System.IO.FileStream"/> wrapper for the stream.</returns>
		public FileStream Open(FileMode Mode) 
		{
			return Open(Mode, FileAccess.ReadWrite, FileShare.None);
		}
		/// <summary>
		/// Opens or creates the stream with no sharing.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> action for the stream.</param>
		/// <param name="Access">The <see cref="System.IO.FileAccess"/> level for the stream.</param>
		/// <returns>A <see cref="System.IO.FileStream"/> wrapper for the stream.</returns>
		public FileStream Open(FileMode Mode, FileAccess Access) 
		{
			return Open(Mode, Access, FileShare.None);
		}
		/// <summary>
		/// Opens or creates the stream.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> action for the stream.</param>
		/// <param name="Access">The <see cref="System.IO.FileAccess"/> level for the stream.</param>
		/// <param name="Share">The <see cref="System.IO.FileShare"/> level for the stream.</param>
		/// <returns>A <see cref="System.IO.FileStream"/> wrapper for the stream.</returns>
		public FileStream Open(FileMode Mode, FileAccess Access, FileShare Share) 
		{
			try 
			{
				IntPtr hFile = Kernel32.CreateFile(ToString(), Kernel32.Access2API(Access), Share, 0, Mode, 0, 0);
				return new FileStream(hFile, Access, true);
			}
			catch 
			{
				return null;
			}
		}
#endregion

#region Delete
		/// <summary>
		/// Deletes the stream from the file.
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value: true if the stream was deleted, false if there was an error.</returns>
		public bool Delete() 
		{
			return Kernel32.DeleteFile(ToString());
		}
#endregion
	}


	/// <summary>
	/// Encapsulates the collection of alternative data streams for a file.
	/// A collection of <see cref="StreamInfo"/> objects.
	/// </summary>
	public class FileStreams : CollectionBase 
	{
		private FileInfo _file;

#region Constructors
		public FileStreams(string File) 
		{
			_file = new FileInfo(File);
			initStreams();
		}
		public FileStreams(FileInfo file) 
		{
			_file = file;
			initStreams();
		}

		/// <summary>
		/// Reads the streams from the file.
		/// </summary>
		private void initStreams() 
		{
			//Open the file with backup semantics
			IntPtr hFile = Kernel32.CreateFile(_file.FullName, Kernel32.FileAccessAPI.GENERIC_READ, FileShare.Read, 0, FileMode.Open, Kernel32.FileFlags.BackupSemantics, 0);
			if (hFile.ToInt32() == Kernel32.INVALID_HANDLE_VALUE) return;

			try 
			{
				Kernel32.WIN32_STREAM_ID sid = new Kernel32.WIN32_STREAM_ID();
				int dwStreamHeaderSize = Marshal.SizeOf(sid);
				int Context = 0;
				bool Continue = true;
				while (Continue) 
				{
					//Read the next stream header
					int lRead = 0;
					Continue = Kernel32.BackupRead(hFile, ref sid, dwStreamHeaderSize, ref lRead, false, false, ref Context);
					if (Continue && lRead == dwStreamHeaderSize) 
					{
						if (sid.dwStreamNameSize>0) 
						{
							//Read the stream name
							lRead = 0;
							IntPtr pName = Marshal.AllocHGlobal(sid.dwStreamNameSize);
							try 
							{
								Continue = Kernel32.BackupRead(hFile, pName, sid.dwStreamNameSize, ref lRead, false, false, ref Context);
								char[] bName = new char[sid.dwStreamNameSize];
								Marshal.Copy(pName,bName,0,sid.dwStreamNameSize);

								//Name is of the format ":NAME:$DATA\0"
								string sName = new string(bName);
								int i = sName.IndexOf(Kernel32.STREAM_SEP, 1);
								if (i>-1) sName = sName.Substring(1, i-1);
								else 
								{
									//This should never happen. 
									//Truncate the name at the first null char.
									i = sName.IndexOf('\0');
									if (i>-1) sName = sName.Substring(1, i-1);
								}

								//Add the stream to the collection
								base.List.Add(new StreamInfo(this,sName,sid.Size.ToInt64()));
							}
							finally 
							{
								Marshal.FreeHGlobal(pName);
							}
						}

						//Skip the stream contents
						int l = 0; int h = 0;
						Continue = Kernel32.BackupSeek(hFile, sid.Size.Low, sid.Size.High, ref l, ref h, ref Context);
					}
					else break;
				}
			}
			finally 
			{
				Kernel32.CloseHandle(hFile);
			}
		}
#endregion

#region File
		/// <summary>
		/// Returns the <see cref="System.IO.FileInfo"/> object for the wrapped file. 
		/// </summary>
		public FileInfo FileInfo 
		{
			get { return _file; }
		}
		/// <summary>
		/// Returns the full path to the wrapped file.
		/// </summary>
		public string FileName 
		{
			get { return _file.FullName; }
		}

		/// <summary>
		/// Returns the size of the main data stream, in bytes.
		/// </summary>
		public long FileSize {
			get {return _file.Length;}
		}

		/// <summary>
		/// Returns the size of all streams for the file, in bytes.
		/// </summary>
		public long Size 
		{
			get 
			{
				long size = this.FileSize;
				foreach (StreamInfo s in this) size += s.Size;
				return size;
			}
		}
#endregion

#region Open
		/// <summary>
		/// Opens or creates the default file stream.
		/// </summary>
		/// <returns><see cref="System.IO.FileStream"/></returns>
		public FileStream Open() 
		{
			return new FileStream(_file.FullName, FileMode.OpenOrCreate);
		}

		/// <summary>
		/// Opens or creates the default file stream.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> for the stream.</param>
		/// <returns><see cref="System.IO.FileStream"/></returns>
		public FileStream Open(FileMode Mode) 
		{
			return new FileStream(_file.FullName, Mode);
		}

		/// <summary>
		/// Opens or creates the default file stream.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> for the stream.</param>
		/// <param name="Access">The <see cref="System.IO.FileAccess"/> for the stream.</param>
		/// <returns><see cref="System.IO.FileStream"/></returns>
		public FileStream Open(FileMode Mode, FileAccess Access) 
		{
			return new FileStream(_file.FullName, Mode, Access);
		}

		/// <summary>
		/// Opens or creates the default file stream.
		/// </summary>
		/// <param name="Mode">The <see cref="System.IO.FileMode"/> for the stream.</param>
		/// <param name="Access">The <see cref="System.IO.FileAccess"/> for the stream.</param>
		/// <param name="Share">The <see cref="System.IO.FileShare"/> for the stream.</param>
		/// <returns><see cref="System.IO.FileStream"/></returns>
		public FileStream Open(FileMode Mode, FileAccess Access, FileShare Share) 
		{
			return new FileStream(_file.FullName, Mode, Access, Share);
		}
#endregion

#region Delete
		/// <summary>
		/// Deletes the file, and all alternative streams.
		/// </summary>
		public void Delete() 
		{
			for (int i=base.List.Count;i>0;i--) 
			{
				base.List.RemoveAt(i);
			}
			_file.Delete();
		}
#endregion

#region Collection operations
		/// <summary>
		/// Add an alternative data stream to this file.
		/// </summary>
		/// <param name="Name">The name for the stream.</param>
		/// <returns>The index of the new item.</returns>
		public int Add(string Name) 
		{
			StreamInfo FSI = new StreamInfo(this, Name, 0);
			int i = base.List.IndexOf(FSI);
			if (i==-1) i = base.List.Add(FSI);
			return i;
		}
		/// <summary>
		/// Removes the alternative data stream with the specified name.
		/// </summary>
		/// <param name="Name">The name of the string to remove.</param>
		public void Remove(string Name) 
		{
			StreamInfo FSI = new StreamInfo(this, Name, 0);
			int i = base.List.IndexOf(FSI);
			if (i>-1) base.List.RemoveAt(i);
		}

		/// <summary>
		/// Returns the index of the specified <see cref="StreamInfo"/> object in the collection.
		/// </summary>
		/// <param name="FSI">The object to find.</param>
		/// <returns>The index of the object, or -1.</returns>
		public int IndexOf(StreamInfo FSI) 
		{
			return base.List.IndexOf(FSI);
		}
		/// <summary>
		/// Returns the index of the <see cref="StreamInfo"/> object with the specified name in the collection.
		/// </summary>
		/// <param name="Name">The name of the stream to find.</param>
		/// <returns>The index of the stream, or -1.</returns>
		public int IndexOf(string Name) 
		{
			return base.List.IndexOf(new StreamInfo(this, Name, 0));
		}

		public StreamInfo this[int Index] 
		{
			get { return (StreamInfo)base.List[Index]; }
		}
		public StreamInfo this[string Name] 
		{
			get 
			{ 
				int i = IndexOf(Name);
				if (i==-1) return null;
				else return (StreamInfo)base.List[i];
			}
		}
#endregion

#region Overrides
		/// <summary>
		/// Throws an exception if you try to add anything other than a StreamInfo object to the collection.
		/// </summary>
		protected override void OnInsert(int index, object value) 
		{
			if (!(value is StreamInfo)) throw new InvalidCastException();
		}
		/// <summary>
		/// Throws an exception if you try to add anything other than a StreamInfo object to the collection.
		/// </summary>
		protected override void OnSet(int index, object oldValue, object newValue) 
		{
			if (!(newValue is StreamInfo)) throw new InvalidCastException();
		}

		/// <summary>
		/// Deletes the stream from the file when you remove it from the list.
		/// </summary>
		protected override void OnRemoveComplete(int index, object value) 
		{
			try 
			{
				StreamInfo FSI = (StreamInfo)value;
				if (FSI != null) FSI.Delete();
			}
			catch {}
		}

		public new StreamEnumerator GetEnumerator() 
		{
			return new StreamEnumerator(this);
		}
#endregion

#region StreamEnumerator
		public class StreamEnumerator : object, IEnumerator 
		{
			private IEnumerator baseEnumerator;
            
			public StreamEnumerator(FileStreams mappings) 
			{
				this.baseEnumerator = ((IEnumerable)(mappings)).GetEnumerator();
			}
            
			public StreamInfo Current 
			{
				get 
				{
					return ((StreamInfo)(baseEnumerator.Current));
				}
			}
            
			object IEnumerator.Current 
			{
				get 
				{
					return baseEnumerator.Current;
				}
			}
            
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
            
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
#endregion
	}

}