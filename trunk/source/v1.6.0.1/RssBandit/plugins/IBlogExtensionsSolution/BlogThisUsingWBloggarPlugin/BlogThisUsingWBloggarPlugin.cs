using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.IO; 
using System.Xml.XPath; 
using System.Xml.Xsl;
using Syndication.Extensibility;
using System.Diagnostics;
using Microsoft.Win32;

namespace Haack.Rss.BlogExtensions
{
	/// <summary>
	/// An updated version of the standard Blog This With w.bloggar 
	/// extension. This one allows configuration.
	/// </summary>
	public sealed class BlogThisUsingWbloggarPlugin : IBlogExtension
	{
		RegistryKey _registryKey = null;
		BlogThisType _blogType = BlogThisType.None;

		/// <summary>
		/// True in this case, this plug-in has configuration settings.
		/// </summary>
		public bool HasConfiguration { get {return true; } }
		
		/// <summary>
		/// Return true if an editing GUI will be shown when BlogItem is called. 
		/// In this case, it is true.
		/// </summary>
		public bool HasEditingGUI{ get {return true; } }
		
		/// <summary>
		/// Displays configuration dialog to user.  This allows the user 
		/// to select whether the entire item is pasted into w.bloggar or 
		/// just the link.
		/// </summary>
		/// <param name="parent"></param>
		public void Configure(IWin32Window parent)
		{
			using(ConfigurationForm form = new ConfigurationForm())
			{
				form.BlogType = this.BlogType;
			
				DialogResult result = form.ShowDialog(parent);
				if(result == DialogResult.OK)
				{
					this.BlogType = form.BlogType; //Writes it to the configuration.
				}
			}
		}

		/// <summary>
		/// "Blog This Using w.bloggar...".  The name of the plug-in that is displayed 
		/// in the context menu when right clicking on a feed item.
		/// </summary>
		public string DisplayName { get { return "Blog This Using w.bloggar..."; } }

 
		/// <summary>
		/// Blogs the item.
		/// </summary>
		/// <param name="rssFragment">RSS fragment.</param>
		/// <param name="edited">Edited.</param>
		public void BlogItem(System.Xml.XPath.IXPathNavigable rssFragment, bool edited) 
		{
			if(!IsBloggarInstalled)
			{
				throw new ApplicationException("No registry setting for w.bloggar located. Please install w.bloggar from http://www.wbloggar.com to get this feature to work"); 
			}
	   
			string wbloggarPath = ((string)BloggarRegistryKey.GetValue("InstallPath")); 
	   
			XslTransform transform = new XslTransform();
			transform.Load(new XmlTextReader(XsltStream), null, null);

			string tempfile = Path.GetTempFileName(); 
			transform.Transform(rssFragment, null, new StreamWriter(tempfile), null);
	   	   
			Process.Start(wbloggarPath + @"\wbloggar.exe", tempfile); 
		}

		Stream XsltStream
		{
			get
			{
				string resourceName = "Haack.Rss.BlogExtensions.Resources." + this.BlogType.ToString() + ".xslt";
				return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			}
		}

		// Checks the registry to see if w.bloggar is installed.
		bool IsBloggarInstalled
		{
			get
			{
				return BloggarRegistryKey != null;
			}
		}

		RegistryKey BloggarRegistryKey
		{
			get
			{
				if(_registryKey == null)
					_registryKey = Registry.CurrentUser.OpenSubKey(@"Software\VB and VBA Program Settings\Bloggar");
				return _registryKey;
			}
		}

		/// <summary>
		/// Gets or sets the blog type.
		/// </summary>
		/// <remarks>
		/// The blog type determines how much of the post to blog.
		/// </remarks>
		/// <value></value>
		BlogThisType BlogType
		{
			get
			{
				if(_blogType != BlogThisType.None)
					return _blogType;
				
				try
				{
					XmlDocument doc = null;
		
					if(File.Exists(ConfigurationPath))
					{
						doc = new XmlDocument();
						doc.Load(ConfigurationPath);
						XPathNavigator navigator = doc.CreateNavigator();
						_blogType = (BlogThisType)Enum.Parse(typeof(BlogThisType), ((string)navigator.Evaluate("string(//blogType/text())")).Trim());
						return _blogType;
					}
				}
				catch(Exception)
				{
					//Ignore.
				}
				return BlogThisType.LinkOnly; //Default.
			}
			set
			{
				try
				{
					// Create directory within Application Settings folder.
					if(!Directory.Exists(Path.GetDirectoryName(ConfigurationPath)))
						Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationPath));

					bool append = true;
					using(StreamWriter writer = new StreamWriter(ConfigurationPath, !append))
					{
						writer.Write("<?xml version=\"1.0\" standalone=\"yes\" ?><blogThisUsingWBloggarPluginSettings><blogType>" + value.ToString() + "</blogType></blogThisUsingWBloggarPluginSettings>");
					}
				}
				finally
				{
					_blogType = value;
				}
			}
		}
		
		string ConfigurationPath
		{
			get
			{
				string configPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
				return Path.Combine(configPath, @"BlogThisUsingWBloggarPlugin\wBloggarSettings.xml");
			}
		}
	
		internal enum BlogThisType
		{
			None,
			LinkOnly,
			LinkWithAuthor,
			Full
		}
	}
}
