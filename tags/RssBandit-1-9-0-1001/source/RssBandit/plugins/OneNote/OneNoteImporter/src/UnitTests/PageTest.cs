using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using NUnit.Framework;

namespace Microsoft.Office.OneNote.UnitTests
{
	/// <summary>
	/// Tests the Page class.
	/// </summary>
	[TestFixture]
	public class PageTest
	{
		[Test]
		public void EmptyPage()
		{
			Page page = new Page(sectionPath);
			Assert.AreEqual(page.SectionPath, sectionPath);

			// Serialize the page:
			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());

			XmlElement import = serializedPage.DocumentElement;
			Assert.IsTrue(import.ChildNodes.Count == 1);

			XmlElement ensurePageVerb = (XmlElement) import.ChildNodes.Item(0);
			Assert.AreEqual(sectionPath, ensurePageVerb.GetAttribute("path"));
		}

		[Test]
		public void Title()
		{
			Page page = new Page(sectionPath);
			Assert.IsNull(page.Title);

			page.Title = pageTitle;
			Assert.AreEqual(pageTitle, page.Title);

			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());

			XmlElement import = serializedPage.DocumentElement;
			Assert.IsTrue(import.ChildNodes.Count == 1);

			XmlElement ensurePageVerb = (XmlElement) import.ChildNodes.Item(0);
			Assert.AreEqual(pageTitle, ensurePageVerb.GetAttribute("title"));
		}

		[Test]
		public void Date()
		{
			Page page = new Page(sectionPath);
			DateTime pageDate = new DateTime(2004, 4, 1); // Happy April Fool's Day!
			page.Date = pageDate;

			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());

			XmlElement import = serializedPage.DocumentElement;
			Assert.IsTrue(import.ChildNodes.Count == 1);

			XmlElement ensurePageVerb = (XmlElement) import.ChildNodes.Item(0);
			Assert.IsTrue(ensurePageVerb.HasAttribute("date"));

			DateTime serializeDate = XmlConvert.ToDateTime(ensurePageVerb.GetAttribute("date"));
			Assert.AreEqual(pageDate, serializeDate);
		}

		[Test]
		public void Html()
		{
			Page page = new Page(sectionPath);

			string htmlString = "<html><body><p>hello world</p></body></html>";
			HtmlContent htmlContent = new HtmlContent(htmlString);
			Assert.IsNotNull(htmlContent.HtmlData);

			OutlineObject outlineObject = new OutlineObject();
			outlineObject.AddContent(htmlContent);

			int cContent = 0;
			foreach (OutlineContent content in outlineObject)
			{
				Assert.AreSame(htmlContent, content);
				cContent++;
			}
			Assert.IsTrue(cContent == 1);

			page.AddObject(outlineObject);

			int cObjects = 0;
			foreach (PageObject pageObject in page)
			{
				Assert.AreSame(outlineObject, pageObject);
				cObjects++;
			}
			Assert.IsTrue(cObjects == 1);

			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());

			XmlElement import = serializedPage.DocumentElement;
			Assert.IsTrue(import.ChildNodes.Count == 2);

			XmlElement placeObjectsVerb = (XmlElement) import.ChildNodes.Item(1);
			Assert.IsTrue(placeObjectsVerb.ChildNodes.Count == 1);

			XmlElement objectElement = (XmlElement) placeObjectsVerb.ChildNodes.Item(0);
			Assert.IsTrue(objectElement.ChildNodes.Count == 2);

			XmlElement positionElement = (XmlElement) objectElement.ChildNodes.Item(0);
			Assert.AreEqual(positionElement.GetAttribute("x"), outlineObject.Position.X.ToString());
			Assert.AreEqual(positionElement.GetAttribute("y"), outlineObject.Position.Y.ToString());

			XmlElement outlineElement = (XmlElement) objectElement.ChildNodes.Item(1);
			Assert.IsTrue(outlineElement.ChildNodes.Count == 1);

			XmlElement htmlElement = (XmlElement) outlineElement.ChildNodes.Item(0);
			XmlElement dataElement = (XmlElement) htmlElement.ChildNodes.Item(0);

			Assert.AreEqual(htmlString, dataElement.InnerText);
		}

		[Test]
		public void Text()
		{
			Page page = new Page(sectionPath);

			string htmlString = "hello world";
			HtmlContent htmlContent = new HtmlContent(htmlString);
			Assert.IsNotNull(htmlContent.HtmlData);
			OutlineObject outlineObject = new OutlineObject();
			outlineObject.AddContent(htmlContent);

			page.AddObject(outlineObject);

			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());

			XmlElement import = serializedPage.DocumentElement;

			string validHtml = "<html><body>" + htmlString + "</body></html>";
			Assert.AreEqual(validHtml, import.InnerText);
		}

		[Test]
		public void ConstructWithHtml()
		{
			Page page = new Page(sectionPath, pageTitle, "hello world");

			OutlineObject outline = null;
			int cPageObjects = 0;

			foreach (PageObject pageObject in page)
			{
				outline = (OutlineObject) pageObject;
				cPageObjects++;
			}

			Assert.AreEqual(1, cPageObjects);

			HtmlContent content = null;
			int cContent = 0;

			foreach (OutlineContent outlineContent in outline)
			{
				content = (HtmlContent) outlineContent;
				cContent++;
			}

			Assert.AreEqual(1, cPageObjects);

			Assert.AreEqual(content.HtmlData, "hello world");
		} 

		[Test]
		public void Serialization()
		{
			Page page = new Page(sectionPath);

			string htmlString = "<html><body><p>hello world</p></body></html>";
			HtmlContent htmlContent = new HtmlContent(htmlString);

			OutlineObject outlineObject = new OutlineObject();
			outlineObject.AddContent(htmlContent);

			page.AddObject(outlineObject);

			IFormatter formatter = new BinaryFormatter();
			Stream serialized = new MemoryStream();
			formatter.Serialize(serialized, page);

			serialized.Seek(0, SeekOrigin.Begin);
			Page deserialized = (Page) formatter.Deserialize(serialized);
			serialized.Close();

			Assert.AreEqual(page, deserialized);
			Assert.AreEqual(page.ToString(), deserialized.ToString());
		}

		[Test]
		public void Image()
		{
			Page page = new Page(sectionPath, pageTitle);

			OutlineObject outline = new OutlineObject();
			page.AddObject(outline);

			Bitmap bitmap = new Bitmap(200, 200);
			for (int y = 0; y < bitmap.Height; y++)
				for (int x = 0; x < bitmap.Width; x++)
					bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 128));

			MemoryStream stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Bmp);

			outline.AddContent(new ImageContent(stream.ToArray())); 

			XmlDocument serializedPage = new XmlDocument();
			serializedPage.LoadXml(page.ToString());
		}

		private const string sectionPath = "Test.one";
		private const string pageTitle = "My funky title";
	}
}