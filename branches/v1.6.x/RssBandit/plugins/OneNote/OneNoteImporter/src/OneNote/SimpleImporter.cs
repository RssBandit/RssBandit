using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Wraps the COM ISimpleImporter interface and maps application
	/// errors to exceptions. You would not normally need to use this directly unless you want
	/// to directly call the underlying COM methods. The Page object wraps these functions for you.
	/// </summary>
	public class SimpleImporter
	{
		/// <summary>
		/// Creates a new Importer object.  A new Importer should be created
		/// before performing any import or navigation operations as construction
		/// will guarantee that OneNote has been launched.  If OneNote is not
		/// installed, or cannot be found, an <see cref="ImportException"/> will
		/// be thrown.
		/// </summary>
		public SimpleImporter()
		{
			try
			{
				// CoCreate the importer.  This will implicitly start the EXE 
				// server if necessary.
				CSimpleImporter simpleImporter = new CSimpleImporter();

				// QI it for ISimpleImporter:
				importer = (ISimpleImporter) simpleImporter;
			}
			catch (COMException e)
			{
				if (0x80040154 == (uint) e.ErrorCode)
				{
					throw new ImportException("OneNote is not installed on this machine!");
				}

				throw;
			}
			catch (FileNotFoundException) 
			{
				throw new ImportException("OneNote.exe cannot be found, please reinstall!");
			}
		}

		/// <summary>
		/// Imports the specified content into OneNote.  The specified XML 
		/// can contain any number of EnsurePage and PlaceObject verbs.  For
		/// more information regarding the format of the XML, please refer 
		/// to the schema file: SimpleImport.xsd.  In the event of an error,
		/// an <see cref="ImportException"/> will be thrown.
		/// </summary>
		/// <param name="xml">The XML to be imported.</param>
		public void Import(string xml)
		{
			try
			{
				importer.Import(xml);
			}
			catch (COMException e)
			{
				throw new ImportException(e.ErrorCode);
			}
		}

		/// <summary>
		/// Navigates to a page in OneNote using a specific section path 
		/// and page guid.  In the event of an error, an <see 
		/// cref="ImportException"/> will be thrown.
		/// </summary>
		/// <param name="sectionPath">The file path of the OneNote section.</param>
		/// <param name="pageGuid">The guid of the OneNote page.</param>
		public void NavigateToPage(string sectionPath, string pageGuid)
		{
			try
			{
				importer.NavigateToPage(sectionPath, pageGuid);
			}
			catch (COMException e)
			{
				throw new ImportException(e.ErrorCode);
			}
		}

		private ISimpleImporter importer;
	}	

	/*------------------------------------------------------------------------*/

	/// <summary>
	/// The ISimpleImporter is a COM interface which can be used to create
	/// OneNote hierarchy such as folders, sections and pages, and import
	/// content into this hierarchy (marshalled in an XML format).  
	/// </summary>
	[Guid("F56A67BB-243F-4927-B987-F57B3D9DBEFE"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	interface ISimpleImporter
	{
		void Import([In, MarshalAs(UnmanagedType.BStr)] string xml);

		void NavigateToPage(
			[In, MarshalAs(UnmanagedType.BStr)] string path, 
			[In, MarshalAs(UnmanagedType.BStr)] string guid);
	}

	/*------------------------------------------------------------------------*/

	/// <summary>
	/// A CSimpleImporter is a COM component that can be instantiated and
	/// subsequently cast to an ISimpleImporter.  
	/// </summary>
	[ComImport, Guid("22148139-F1FC-4EB0-B237-DFCD8A38EFFC")]
	class CSimpleImporter
	{
	}
}