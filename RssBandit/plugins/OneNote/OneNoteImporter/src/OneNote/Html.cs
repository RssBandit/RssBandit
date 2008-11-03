using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Encapsulates a chunk of HTML and provides common workarounds for OneNote html parsing. 
	/// HTMLContent is used in OutlineObjects. An OutlineObject can have 
	/// one or more HTMLContent objects. 
	/// </summary>
	[Serializable]
	public class HtmlContent : OutlineContent
	{
		/// <summary>
		/// Constructs a new <see cref="HtmlContent"/> object containing the HTML 
		/// in the specified file.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> containing our html data.</param>
		public HtmlContent(FileInfo file)
		{
			HtmlData = new FileData(file);
		}

		/// <summary>
		/// Constructs a new <see cref="HtmlContent"/> object containing the HTML
		/// in the given string.
		/// </summary>
		/// <param name="html">The <see cref="string"/> containing our html data.</param>
		public HtmlContent(String html)
		{
			HtmlData = new StringData(html);
		}

		/// <summary>
		/// Constructs a new <see cref="HtmlContent"/> object containing the HTML copied 
		/// from the specified HtmlContent.  
		/// </summary>
		/// <param name="clone">The <see cref="HtmlContent"/> whose data is to be copied to 
		/// the new HtmlContent.</param>
		public HtmlContent(HtmlContent clone)
		{
			HtmlData = clone.HtmlData;
		}

		/// <summary>
		/// Clones the HtmlContent, returning a new deep copy.
		/// </summary>
		/// <returns>A new HtmlContent with the same value as the original.</returns>
		public override object Clone()
		{
			return new HtmlContent(this);
		}

		/// <summary>
		/// Serializes the <see cref="HtmlContent"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement htmlElement = xmlDocument.CreateElement("Html");
			parentNode.AppendChild(htmlElement);

			// Clean our HTML:
			if (HtmlData is StringData)
			{
				Data tidyHtmlData = (Data) HtmlData.Clone();
				tidyHtmlData.data = CleanHtml(HtmlData.data);

				tidyHtmlData.SerializeToXml(htmlElement);
			}
			else
			{
				// TODO: Read the file data into memory and clean it as well:
				HtmlData.SerializeToXml(htmlElement);
			}
		}

		/// <summary>
		/// Given 
		/// </summary>
		/// <param name="inputText"></param>
		/// <returns></returns>
		protected internal static string CleanHtml(string inputText)
		{
			// Wrap the text with the appropriate html tags if necessary:
			if (inputText.IndexOf("<html") == -1)
			{
				inputText = "<html><body>" + inputText + "</body></html>";
			}

			// Replace <p xmlns=\"http://www.w3.org/1999/xhtml\"> with <p>
			inputText = inputText.Replace("<p xmlns=\"http://www.w3.org/1999/xhtml\">", "<p>");

			// Need to convert &nbsp; to 0xA0 or else OneNote will think each one is a new OutlineElement
			char space = '\xA0';
			inputText = inputText.Replace("&nbsp;", space.ToString());

			// Replace <p>\s</p> with <p>&nbsp;</p>
			inputText = HtmlContent.replacePWhiteSpace.Replace(inputText, "${open}&nbsp;${close}");

			// Replace </p>\s<p> with </p><p>&nbsp;</p><p>
			inputText = HtmlContent.replacePWhiteSpaceBetween.Replace(inputText, "${open}<p>&nbsp;</p>${close}");

			// Replace \r\n inside <pre>\w</pre> with <br> tags
			foreach (Match match in HtmlContent.replacePreWithBr.Matches(inputText))
			{
				string innerText = match.Groups["inner"].Value;
				string innerTextReplaced = innerText.Replace("\r\n", "<br>");
				innerTextReplaced = innerTextReplaced.Replace("\t", "&nbsp;");
				inputText = inputText.Replace(innerText, innerTextReplaced);
			}

			// Replace <pre> and </pre with <div style="font-family:Courier New"> and </div>
			inputText = Regex.Replace(inputText, @"<pre[^>]*>", "<div style=\"font-family:Courier New>\"");
			inputText = inputText.Replace("</pre>", "</div>");

			// Replace <br>\s<br> with <p>&nbsp;</p>
			inputText = HtmlContent.replaceBr.Replace(inputText, "<p>&nbsp;</p>");

			return inputText;
		}

		/// <summary>
		/// Gets or sets the HTML data.
		/// </summary>
		public Data HtmlData
		{
			get
			{
				return (Data) GetChild("HtmlData");
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("HtmlData");

				if (!(value is FileData || value is StringData))
					throw new ArgumentException("Incorrect data type.");

				Data htmlData = HtmlData;
				if (htmlData != null)
					RemoveChild(htmlData);

				AddChild(value, "HtmlData");
			}
		}

		private static readonly Regex replacePWhiteSpaceBetween = new Regex(@"(?<open></p[^>]*>)(?<inner>[^<\w]*)(?<close><p>)",
		                                                                    RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex replacePWhiteSpace = new Regex(@"(?<open>\<p[^>]*>)(?<inner>[^<\w]*)(?<close>\</p>)",
		                                                             RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex replacePreWithBr = new Regex(@"(?<open><pre[^>]*>)(?<inner>(?:\s*([^<]+)\s*)+)(?<close></pre>)",
		                                                           RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex replaceBr = new Regex(@"(?<open><br[^>]*>)(?<inner>[^<\w]*)(?<close><br[^>]*>)",
		                                                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}