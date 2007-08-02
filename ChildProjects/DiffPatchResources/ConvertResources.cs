using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace DiffPatchResources
{
	/// <summary>
	/// Summary description for ConvertResources.
	/// </summary>
	public class ConvertResources
	{
		private string resource;
		public Exception ExecutionException = null;

		public static Regex findVersion = new Regex(@"Version=(?<vn>\d+\.\d+\.\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public ConvertResources(string resourceFileName) {
			this.resource = resourceFileName;
		}

		public bool Run(bool testOnly, CompatibleCLRVersion version) {
			if (!File.Exists(resource)) 
				return ExitWith(new ArgumentException(String.Format("File does not exist: '{0}'", resource)));
				
			ArrayList resources = new ArrayList();
			resources.Add(resource);

			string baseName = Path.GetFileNameWithoutExtension(resource);
			
			string[] languageResources = Directory.GetFiles(Path.GetDirectoryName(resource), baseName + ".*.resx");
			if (languageResources != null && languageResources.Length > 0)
				resources.AddRange(languageResources);

			bool isWindowsFormsResource = false;
			XmlDocument docSource = new XmlDocument();
			docSource.PreserveWhitespace = true;
			docSource.XmlResolver = null;
			try {
				docSource.Load(resource);
				isWindowsFormsResource = (null != docSource.SelectSingleNode("/root/data[@name='>>$this.Type']"));
			} catch (Exception ex) {
				return ExitWith(ex);
			}


			foreach (string langResFile in resources) {
				XmlDocument docToConvert = new XmlDocument();
				docToConvert.PreserveWhitespace = true;
				docToConvert.XmlResolver = null;
				try {
					Console.WriteLine(" + {0}  ", Path.GetFileName(langResFile));
					docToConvert.Load(langResFile);
					
					int changes = 0;
					
					if (isWindowsFormsResource) {
						try {
							changes = ConvertResTypes(docToConvert, version, testOnly);
						} catch (Exception taskEx) {
							return ExitWith(taskEx);
						}
					} else {
						try {
							changes = ConvertResHeader(docToConvert, version, testOnly);
						} catch (Exception taskEx) {
							return ExitWith(taskEx);
						}
					}

					if (!testOnly && (changes != 0)) {
						File.Copy(langResFile, langResFile + ".bak", true);
						docToConvert.Save(langResFile);
					}

					Console.WriteLine("\tconversions:{0}", changes);
				} catch (Exception ex) {
					return ExitWith(ex);
				}

			}

			return true;	// success
		}

		private bool ExitWith(Exception ex) {
			this.ExecutionException = ex;
			return false;
		}

		public static int ConvertResHeader(XmlDocument doc, CompatibleCLRVersion version, bool verbose) {
			int changes = 0;
			foreach (XmlNode sn in doc.SelectNodes("/root/resheader")) {
				XmlNode name = sn.Attributes.GetNamedItem("name");
				string idName = name.InnerText;
				XmlNode srcValue = sn.SelectSingleNode("value");
				
				if (idName == "Version"){
					if (version == CompatibleCLRVersion.Version_1_0) {
						if ("1.0.0.0" != srcValue.InnerText) {
							srcValue.InnerText = "1.0.0.0";
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if ("1.0.0.0" != srcValue.InnerText) {
							srcValue.InnerText = "1.0.0.0";
							changes++;
						}
					}
				}

				if (idName == "Reader"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3102.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3102.0");
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}
				}

				if (idName == "Writer"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3102.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3102.0");
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}
				}

			}
			return changes;
		}

		public static int ConvertResTypes(XmlDocument doc, CompatibleCLRVersion version, bool verbose) {
			int changes = 0;

			foreach (XmlNode sn in doc.SelectNodes("/root/resheader")) {
				XmlNode name = sn.Attributes.GetNamedItem("name");
				string idName = name.InnerText;
				XmlNode srcValue = sn.SelectSingleNode("value");
				
				if (idName == "version"){
					if (version == CompatibleCLRVersion.Version_1_0) {
						if ("1.3" != srcValue.InnerText) {
							srcValue.InnerText = "1.3";
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if ("1.3" != srcValue.InnerText) {
							srcValue.InnerText = "1.3";
							changes++;
						}
					}
				}

				if (idName == "reader"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3300.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3300.0");
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}
				}

				if (idName == "writer"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3300.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3300.0");
							changes++;
						}
					}
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}
				}

			}

			foreach (XmlNode sn in doc.SelectNodes("/root/data")) {
				XmlNode type = sn.Attributes.GetNamedItem("type");
				
				if (type != null){
					string v = GetVersion(type.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3300.0") {
							type.InnerText = type.InnerText.Replace(v, "1.0.3300.0");
							changes++;
						}
					}

					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							type.InnerText = type.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}
				}
			}
			return changes;
		}

		private static string GetVersion(string source) {
			if (source == null)
				return null;
			Match m = findVersion.Match(source);
			if (m != null && m.Groups.Count > 0) {
				return m.Groups["vn"].Value;
			}
			return null;
		}

	}
}
