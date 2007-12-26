using System;
using System.Xml.XPath;
using Syndication.Extensibility;
using System.Windows.Forms;
using System.Diagnostics;

namespace BlogExtension.MailThis {

	public class EmailThisPlugin:IBlogExtension {

		public bool HasConfiguration { get {return false; } }
		public bool HasEditingGUI{ get {return false; } }

		public void Configure(IWin32Window parent){
			/* yeah, right */
		}

		public string DisplayName { get { return Resource.Manager["RES_MenuEmailThisCaption"]; } }

		public void BlogItem(System.Xml.XPath.IXPathNavigable rssFragment, bool edited) {

			LaunchEmail(GetMailUrl(rssFragment.CreateNavigator()));
		}

		private String GetMailUrl(XPathNavigator nav) {
			const String TEMPLATE_STRING = @"mailto:receiver@example.com?subject={0}&body={1}";

			return String.Format(TEMPLATE_STRING,
				this.HandleCharacters(nav.Evaluate("string(//item/title/text())")),
				this.HandleCharacters(nav.Evaluate("string(//item/link/text())")));
		}

		private void LaunchEmail(String mailUrl) {
			Process.Start(mailUrl);
		}

		private static string quote = new String('"', 1);

		private string HandleCharacters(object input) {

			return input.ToString().Replace(System.Environment.NewLine, "%0A").Replace(quote, "%22").Replace("&", "%26");
		}

	}
}
