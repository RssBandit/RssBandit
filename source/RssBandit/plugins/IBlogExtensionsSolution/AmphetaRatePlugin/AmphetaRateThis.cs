using System;
using System.IO;
using System.Net;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Syndication.Extensibility;

namespace AmphetaRatePlugin
{
	/// <summary>
	/// Plug-in that implements the AmphetaRate API.  
	/// </summary>
	/// <remarks>
	/// This allows the user to rate individual feed items and send them to the 
	/// AmphetaRate rating/recommendation service.  The user can then subscribe 
	/// to an AmphetaRate feed that is personalized based on the ratings.  The 
	/// user is identified by a unique id generated by the AmphetaRate site. No 
	/// personal information is sent apart from the ratings the user chooses to 
	/// send.
	/// </remarks>
	public sealed class AmphetaRateThis : IBlogExtension
	{
		string _userID = null; 

		#region IBlogExtension Members

		/// <summary>
		/// Gives the user the ability to rate an item and send the rating 
		/// to AmphetaRate.
		/// </summary>
		/// <param name="rssFragment"></param>
		/// <param name="edited"></param>
		public void BlogItem(System.Xml.XPath.IXPathNavigable rssFragment, bool edited)
		{
			if(this.UserID == null)
			{
				MessageBox.Show("You cannot submit a rating without a valid AmphetaRate user id. Please configure one.");
				return;
			}

			XPathNavigator navigator = rssFragment.CreateNavigator();

			string title = (string)navigator.Evaluate("string(//item/title/text())");
			string link = (string)navigator.Evaluate("string(//item/link/text())");	

			using(RatingForm form = new RatingForm())
			{
				if(form.ShowDialog() == DialogResult.OK)
				{
					string url = String.Format("http://amphetarate.sourceforge.net/dinka-add-rating.php?rating={0}&title={1}&xmlurl={2}&desc={3}&link={4}&guid={5}&uid={6}&encoding={7}", form.SelectedRating, Encode(title), Encode(link), "", "", "", Encode(this.UserID), "unicode-entities");
					//MessageBox.Show(url);
					HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
					request.Method = "GET";
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					if(response.StatusCode != HttpStatusCode.OK)
					{
						MessageBox.Show("An error occurred while submitting the rating. The server responded with " + response.StatusDescription + ".");
					}
				}
			}
		}

		string Encode(string s)
		{
			return HttpUtility.UrlEncodeUnicode(s).Replace("+", "&20");
		}

		/// <summary>
		/// Return true if an editing GUI will be shown when BlogItem is called. 
		/// In this case, it is false.
		/// </summary>
		public bool HasEditingGUI
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// "AmphetaRate This...".  The name of the plug-in that is displayed 
		/// in the context menu when right clicking on a feed item.
		/// </summary>
		public string DisplayName
		{
			get
			{
				return "AmphetaRate This...";
			}
		}

		/// <summary>
		/// Displays configuration dialog to user.  This allows the user 
		/// to generate or configure a unique AmphetaRate id.
		/// </summary>
		/// <param name="parent"></param>
		public void Configure(System.Windows.Forms.IWin32Window parent)
		{
			string amphetaID = this.UserID;

			using(ConfigurationForm form = new ConfigurationForm())
			{
				form.AmphetaRateID = (amphetaID == null ? "" : amphetaID);
			
				DialogResult result = form.ShowDialog(parent);
				if(result == DialogResult.OK)
				{
					this.UserID = form.AmphetaRateID; //Writes it to the configuration.
				}
			}
		}

		// Gets and sets the AmphetaRate user id.
		string UserID
		{
			get
			{
				if(_userID == null)
				{
					XmlDocument doc = null;
			
					if(File.Exists(ConfigurationPath))
					{
						doc = new XmlDocument();
						doc.Load(ConfigurationPath);
						XPathNavigator navigator = doc.CreateNavigator();
						_userID = ((string)navigator.Evaluate("string(//ID/text())")).Trim();
					}
				}
				return _userID;
			}
			set
			{
				if(value != null && value != _userID)
				{
					// Create directory AmphetaRate directory within Application Settings folder.
					if(!Directory.Exists(Path.GetDirectoryName(ConfigurationPath)))
						Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationPath));

					using(StreamWriter writer = new StreamWriter(ConfigurationPath, false))
					{
						writer.Write("<?xml version=\"1.0\" standalone=\"yes\" ?><AmphetaRateSettings><ID>" + value + "</ID></AmphetaRateSettings>");
					}
					_userID = value;
				}
			}
		
		}

		string ConfigurationPath
		{
			get
			{
				string configPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
				return Path.Combine(configPath, @"AmphetaRate\AmphetaSettings.xml");
			}
		}

		/// <summary>
		/// True in this case, this plug-in has configuration settings.
		/// </summary>
		public bool HasConfiguration
		{
			get
			{
				return true;
			}
		}

		#endregion
	}
}