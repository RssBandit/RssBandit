#region CVS Version Header
/*
 * $Id: MultiIcon.cs,v 1.1 2007/03/19 10:43:06 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/03/19 10:43:06 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Drawing;
using NewsComponents.Utils;

namespace RssBandit.WinGui.Utility 
{
	/// <summary>
	/// Major part of that code is from Matthew Hazlett. Thanks!
	/// See also http://www.codeproject.com/dotnet/MultiIcon.asp
	/// </summary>
	/// <remarks>
	/// Changes:
	///  * impl. IDisposable
	///  * fileLoad now open filestream with using
	///  * findIcon search for high color versions (but &lt; 32 bits per pixel)
	/// </remarks>
	public class MultiIcon: IDisposable
	{
		private bool debug = false;
		private MemoryStream icoStream;
		private iconHeader icoHeader;
		private ArrayList icoEntrys;
		public enum DisplayType { Largest = 0, Smallest = 1 }
	    
		//--------------------------------------------------------------------------
		//   Class: iconHeader
		// Purpose: Stores the headers of the ICO
		//
		private class iconHeader {
			public Int16 Reserved;
			public Int16 Type;
			public Int16 Count;

			public iconHeader(MemoryStream icoStream) {
				BinaryReader icoFile = new BinaryReader(icoStream);

				Reserved = icoFile.ReadInt16();
				Type = icoFile.ReadInt16();
				Count = icoFile.ReadInt16();
			}
		}

		//--------------------------------------------------------------------------
		//   Class: iconEntry
		// Purpose: Each icon if the file has its own header.  This is where I read
		//          the info for each icon.
		//
		public class iconEntry {
			public byte Width;
			public byte Height;
			public byte ColorCount;
			public byte Reserved;
			public Int16 Planes;
			public Int16 BitCount;
			public Int32 BytesInRes;
			public Int32 ImageOffset;
	
			public iconEntry(MemoryStream icoStream) {
				BinaryReader icoFile = new BinaryReader(icoStream);

				Width = icoFile.ReadByte();
				Height = icoFile.ReadByte();
				ColorCount = icoFile.ReadByte();
				Reserved = icoFile.ReadByte();
				Planes = icoFile.ReadInt16();
				BitCount = icoFile.ReadInt16();
				BytesInRes = icoFile.ReadInt32();
				ImageOffset = icoFile.ReadInt32();
			}
		}

		//--------------------------------------------------------------------------
		// Main class
		//--------------------------------------------------------------------------

		//--------------------------------------------------------------------------
		// Function: loadFile
		//  Purpose: Loads the icon file into the memory stream
		//
		private bool loadFile(string filename) {
			try {
				using (FileStream icoFile = FileHelper.OpenForRead(filename)) {
					byte[] icoArray = new byte[icoFile.Length];
					icoFile.Read(icoArray, 0, (int)icoFile.Length);
					icoStream = new MemoryStream(icoArray);
				}
			}
			catch   { return false; }
			finally {  }
		
			return true;
		}

		//--------------------------------------------------------------------------
		// Function: buildIcon
		//  Purpose: Loads the icon file into the memory stream
		//
		public Icon buildIcon(int index) {
			iconEntry thisIcon = (iconEntry)icoEntrys[index];
			MemoryStream newIcon = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(newIcon);

			// New Values
			Int16 newNumber = 1;
			Int32 newOffset = 22;

			// Write it
			writer.Write(icoHeader.Reserved);
			writer.Write(icoHeader.Type);
			writer.Write(newNumber);
			writer.Write(thisIcon.Width);
			writer.Write(thisIcon.Height);
			writer.Write(thisIcon.ColorCount);
			writer.Write(thisIcon.Reserved);
			writer.Write(thisIcon.Planes);
			writer.Write(thisIcon.BitCount);
			writer.Write(thisIcon.BytesInRes);
			writer.Write(newOffset);

			// Grab the icon
			byte[] tmpBuffer = new byte[thisIcon.BytesInRes];
			icoStream.Position = thisIcon.ImageOffset;
			icoStream.Read(tmpBuffer, 0, thisIcon.BytesInRes);
			writer.Write(tmpBuffer);
			
			// Finish up
			writer.Flush();
			newIcon.Position = 0;
			return new Icon(newIcon, thisIcon.Width, thisIcon.Height);
		}

		private Icon searchIcons(DisplayType searchKey) {
			int foundIndex = 0;
			int counter = 0;
			int currentBitCnt = 0;
			// this bits per color looks bad in the tree, so ignore:
			int maxSupportedBitCnt = 32;
			
			foreach (iconEntry thisIcon in icoEntrys) {
				iconEntry current = (iconEntry)icoEntrys[foundIndex];

				if (searchKey == DisplayType.Largest) {
					if (thisIcon.Width > current.Width && thisIcon.Height > current.Height ) 
					{ 
						foundIndex = counter; 
						currentBitCnt = thisIcon.BitCount;
					} else if (thisIcon.Width == current.Width && thisIcon.Height == current.Height) {
						if (thisIcon.BitCount < maxSupportedBitCnt) {
							if (thisIcon.BitCount > currentBitCnt) {
								foundIndex = counter; 
								currentBitCnt = thisIcon.BitCount;
							} else {
								foundIndex = counter; 
								currentBitCnt = thisIcon.BitCount;
							}
						}
					}
				} else {
					if ( thisIcon.Width < current.Width && thisIcon.Height < current.Height ) 
					{ 
						foundIndex = counter; 
						currentBitCnt = thisIcon.BitCount;
					} else if ( thisIcon.Width == current.Width && thisIcon.Height == current.Height ) {
						if (thisIcon.BitCount < maxSupportedBitCnt) {
							if (thisIcon.BitCount > currentBitCnt) {
								foundIndex = counter; 
								currentBitCnt = thisIcon.BitCount;
							} else {
								foundIndex = counter; 
								currentBitCnt = thisIcon.BitCount;
							}
						}
					}

				}

				counter++;
			}

			return buildIcon(foundIndex);
		}

		public ArrayList IconsInfo { get{ return icoEntrys; } }
		public Icon FindIcon(DisplayType searchKey)	{ return searchIcons(searchKey); }
		public int Count { get { return icoEntrys.Count; } }

		//--------------------------------------------------------------------------
		// Function: Constructor
		//  Purpose: Loads the icon file into the memory stream
		//
		public MultiIcon(string filename) {
			icoEntrys = new ArrayList();

			// Load the icon Header
			if (loadFile(filename)) {
				icoHeader = new iconHeader(icoStream);
				if (debug) { Debug.WriteLine(String.Format("There are {0} images in this icon file", icoHeader.Count)); }

				// Read the icons
				for (int counter=0; counter < icoHeader.Count; counter++) {
					iconEntry entry = new iconEntry(icoStream);
					icoEntrys.Add(entry);
					if (debug) { Debug.WriteLine(String.Format("This entry has a width of {0} and a height of {1}", entry.Width, entry.Height)); }
				}
			}
		}
		
		#region IDisposable Members

		public void Dispose() {
			if (icoStream != null)			
				icoStream.Close();
		}

		#endregion
	}
}

#region CVS Version Log
/*
 * $Log: MultiIcon.cs,v $
 * Revision 1.1  2007/03/19 10:43:06  t_rendelmann
 * changed: better handling of favicon's (driven by extension now); we are now looking for the smallest and smoothest icon image to use (if ICO)
 *
 */
#endregion
