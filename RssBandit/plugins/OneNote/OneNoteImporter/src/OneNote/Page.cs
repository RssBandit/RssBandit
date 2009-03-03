using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// A Page contains any number of <see cref="OutlineObject"/>s, 
	/// <see cref="InkObject"/>s, and <see cref="ImageObject"/>s.  Updates
	/// to the page are transactional -- clients should call <see cref="Page.Commit"/> to 
	/// push all pending updates to the OneNote COM server.
	/// </summary>
	[Serializable]
	public class Page : ImportNode, IEnumerable
	{
		/// <summary>
		/// Creates a new, empty Page object.
		/// </summary>
		/// <param name="sectionPath">
		/// The path of the section that this page should be created in.
		/// </param>
		public Page(string sectionPath)
		{
			this.sectionPath = sectionPath;
			this.id = new ObjectId();
		}

		/// <summary>
		/// Creates a new, empty Page object.
		/// </summary>
		/// <param name="sectionPath">
		/// The path of the section that this page should be created in.
		/// </param>
		/// <param name="title">
		/// The title of the page.
		/// </param>
		public Page(string sectionPath, string title) : this(sectionPath)
		{
			this.title = title;
		}

		/// <summary>
		/// Creates a new Page with the specified html content.
		/// </summary>
		/// <param name="sectionPath">
		/// The path of the section that this page should be created in.
		/// </param>
		/// <param name="title">
		/// The title of the page.
		/// </param>
		/// <param name="html">The html content to be inserted.</param>
		public Page(string sectionPath, string title, string html) : this(sectionPath, title)
		{
			OutlineObject outline = new OutlineObject();
			AddObject(outline);

			outline.AddContent(new HtmlContent(html));
		}

		/// <summary>
		/// Clones the Page.
		/// </summary>
		/// <returns>A deep copy of this Page.</returns>
		public override object Clone()
		{
			Page clone = new Page(this.sectionPath);

			foreach (PageObject pageObject in this)
			{
				clone.AddObject(pageObject);
			}

			return clone;
		}

		/// <summary>
		/// Adds the specified object to the page.
		/// </summary>
		/// <param name="pageObject">
		/// The object to be added.
		/// </param>
		public void AddObject(PageObject pageObject)
		{
			if (pageObject.Parent != this)
			{
				AddChild(pageObject);
			}

			pageObject.DeletePending = false;
		}

		/// <summary>
		/// Deletes the specified object from the page.
		/// Be aware that if the user has modified the object within OneNote, then
		/// you'll cause those modifications to be lost!
		/// </summary>
		/// <param name="pageObject">The object to be deleted.</param>
		public void DeleteObject(PageObject pageObject)
		{
			if (pageObject.Parent != this)
			{
				throw new ArgumentException("Page does not contain object: " + pageObject);
			}

			pageObject.DeletePending = true;
		}

		/// <summary>
		/// Creates a new PageEnumerator that can be used to iterate over all
		/// of the PageObjects.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return new PageEnumerator(this);
		}

		/// <summary>
		/// Commits all pending changes to the Page to OneNote.
		/// </summary>
		public void Commit()
		{
			if (!CommitPending)
				return;

			SimpleImporter importer = new SimpleImporter();
			importer.Import(this.ToString());

			// Remove all deleted objects:
			foreach (PageObject pageObject in this)
			{
				if (pageObject.DeletePending)
				{
					RemoveChild(pageObject);
					pageObject.DeletePending = false;
				}
			}

			committed = true;
			CommitPending = false;
		}

		/// <summary>
		/// Navigates to this page in OneNote.
		/// </summary>
		public void NavigateTo()
		{
			SimpleImporter importer = new SimpleImporter();
			importer.NavigateToPage(sectionPath, id.ToString());
		}

		/// <summary>
		/// Serializes this page and all child objects pending for update into 
		/// an XML format suitable for import.  
		/// </summary>
		/// <returns>
		/// The XML representing the DataImport transaction as a string.
		/// </returns>
		public override string ToString()
		{
			XmlDocument xmlDocument = new XmlDocument();
			SerializeToXml(xmlDocument);

			// Save the document to a text stream:
			MemoryStream xmlStream = new MemoryStream();

			XmlWriter xmlWriter = new XmlTextWriter(xmlStream, Encoding.Unicode);
			xmlDocument.Save(xmlWriter);
			xmlWriter.Flush();

			// Validate the document:
			ValidateXml(xmlStream);

			// Convert the text stream to a string:
			xmlStream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(xmlStream).ReadToEnd();
		}

		/// <summary>
		/// Serializes the <see cref="Page"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode as XmlDocument;
			if (xmlDocument == null)
				xmlDocument = parentNode.OwnerDocument;

			XmlElement import = xmlDocument.CreateElement("Import");
			parentNode.AppendChild(import);

			// EnsurePage
			XmlElement ensurePageVerb = xmlDocument.CreateElement("EnsurePage");
			ensurePageVerb.SetAttribute("path", sectionPath);
			ensurePageVerb.SetAttribute("guid", id.ToString());
			ensurePageVerb.SetAttribute("date", XmlConvert.ToString(date));
			if (title != null)
				ensurePageVerb.SetAttribute("title", title);
			if (rtl)
				ensurePageVerb.SetAttribute("rtl", "true");
			if (previousPage != null)
				ensurePageVerb.SetAttribute("insertAfter", previousPage.Id.ToString());

			import.AppendChild(ensurePageVerb);

			// PlaceObjects
			XmlElement placeObjectsVerb = xmlDocument.CreateElement("PlaceObjects");
			placeObjectsVerb.SetAttribute("pagePath", sectionPath);
			placeObjectsVerb.SetAttribute("pageGuid", id.ToString());

			foreach (PageObject pageObject in this)
			{
				if (pageObject.CommitPending)
					pageObject.SerializeToXml(placeObjectsVerb);
			}

			if (placeObjectsVerb.ChildNodes.Count > 0)
				import.AppendChild(placeObjectsVerb);

			// We're very sneaky.
			string xmlns = XmlNamespace;
			import.SetAttribute("xmlns", xmlns);
		}

		/// <summary>
		/// Validates the XML agains the SimpleImport.xsd Schema
		/// </summary>
		/// <param name="xmlStream">The serialized document to validate</param>
		/// <exception cref="XmlException"></exception>
		/// <exception cref="XmlSchemaException"></exception>
		protected internal void ValidateXml(Stream xmlStream)
		{
			Assembly assembly = typeof (Page).Assembly;
			string schemaName = typeof (Page).Namespace + ".SimpleImport.xsd";

			Stream xsdStream = assembly.GetManifestResourceStream(schemaName);

			XmlSchema schema = XmlSchema.Read(xsdStream, null);

			xmlStream.Seek(0, SeekOrigin.Begin);

			XmlValidatingReader validator = new XmlValidatingReader(xmlStream, XmlNodeType.Document, null);
			validator.Schemas.Add(schema);

			try
			{
				while (validator.Read())
				{
					continue;
				}
			}
			catch (XmlException ex)
			{
				throw ex;
			}
			catch (XmlSchemaException ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Determines whether or not the specified object is equal to the 
		/// current Page.  
		/// </summary>
		/// <param name="obj">
		/// The object to compare with the current Page.
		/// </param>
		/// <returns>
		/// true if the specified object is equal to the Page; false otherwise
		/// </returns>
		public override bool Equals(object obj)
		{
			Page page = obj as Page;
			if (page == null)
				return false;

			return page.Id.Equals(Id);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="Page"/>, suitable for use 
		/// in hashing algorithms and data structures like a hash table.  
		/// </summary>
		/// <returns>A hash code for the current <see cref="Page"/>.</returns>
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		/// <summary>
		/// The page's permanent id, which is used by the OneNote application
		/// to refer to this page in all transactions by the simple importer.
		/// This id is persisted across sessions.  If you find yourself needing
		/// access to the id, you should subclass the page class; for most tasks
		/// however, you should be able to serialize the entire Page object between
		/// sessions and obtain the desired results.
		/// </summary>
		protected internal ObjectId Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		/// <summary>
		/// The path to the section that this page is contained by.
		/// </summary>
		/// <remarks>
		/// <para>Paths can be absolute, remote, or a relative path rooted at 
		/// the My Notebook directory.</para>
		/// 
		/// <para>If the section (or directory hierarchy preceding the section) does not
		/// currently exist, it will be created.</para>
		/// </remarks>
		public String SectionPath
		{
			get
			{
				return sectionPath;
			}
			set
			{
				sectionPath = value;

				CommitPending = true;
				committed = false;
			}
		}

		/// <summary>
		/// The page title.  Cannot be modified once the page has been 
		/// committed.
		/// </summary>
		public String Title
		{
			get
			{
				return title;
			}
			set
			{
				if (committed)
					throw new ReadOnlyException("Page.Title is read-only.");

				if (title == value || (title != null && title.Equals(value)))
					return;

				title = value;
				CommitPending = true;
			}
		}

		/// <summary>
		/// The page's date, as it will appear in OneNote.
		/// </summary>
		public DateTime Date
		{
			get
			{
				return date;
			}
			set
			{
				if (committed)
					throw new ReadOnlyException("Page.Date is read-only.");

				if (date == value || date.Equals(value))
					return;

				date = value;
				CommitPending = true;
			}
		}

		/// <summary>
		/// Should this page be layed out right to left?
		/// <remarks>
		/// <para>Note that layout in RTL is a bit tricky.  In particular, the
		/// origin of the page (0,0) will be in the upper right hand corner
		/// of the page, yet the coordinate space is still LTR... ie, you'll
		/// need to use negative coordinates for most positions.  In addition,
		/// objects are still left anchored to their position so you'll need to 
		/// account for the object width.</para>
		/// </remarks>		
		/// </summary>
		public bool RTL
		{
			get
			{
				return rtl;
			}
			set
			{
				if (committed)
					throw new ReadOnlyException("Page.RTL is read-only.");

				if (rtl == value)
					return;

				rtl = value;
				CommitPending = true;
			}
		}

		/// <summary>
		/// The page that this page will be inserted after.  If the previous page
		/// exists, this page will be created as a subpage of the previous page.
		/// </summary>
		public Page PreviousPage
		{
			get
			{
				return previousPage;
			}
			set
			{
				if (committed)
					throw new ReadOnlyException("Page.PreviousPage is read-only.");

				if (previousPage == value || (previousPage != null && previousPage.Equals(value)))
					return;

				previousPage = value;
				CommitPending = true;
			}
		}

		private ObjectId id;
		private String sectionPath;
		private String title;
		private DateTime date = DateTime.Now;
		private bool rtl;

		/// <summary>
		/// We need to ensure that our previousPage doesn't get serialized
		/// or else entire page series will be serialized!  Additionally 
		/// serializing the previousPage may lead to instances where two
		/// Page objects with the same ObjectId may exist.
		/// </summary>
		[NonSerialized]
		private Page previousPage;

		/// <summary>
		/// Set to true once this page has been initially committed.
		/// Title, Date, and other page attributes used to ensure the pages
		/// creation cannote be modified once the page has been commited.
		/// </summary>
		private bool committed;

		private const string XmlNamespace = "http://schemas.microsoft.com/office/onenote/2004/import";
		
		/* ------------------------------------------------------------------ */

		class PageEnumerator : IEnumerator
		{
			protected internal PageEnumerator(Page page)
			{
				this.page = page;
				Reset();
			}

			public void Reset()
			{
				index = -1;
			}

			public object Current
			{
				get
				{
					if (index < page.GetChildCount())
					{
						return page.GetChild(index);
					}

					return null;
				}
			}

			public bool MoveNext()
			{
				while (++index < page.GetChildCount() &&
					!(page.GetChild(index) is PageObject))
				{
					continue;
				}

				return (index < page.GetChildCount());
			}

			private Page page;
			private int index;
		}
	}
}