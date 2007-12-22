using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace DiffPatchResources
{
	/// <summary>
	/// ConvertResources between the various CLR versions.
	/// </summary>
	/// <remarks>
	/// Conversion to v2.0 is not yet fully implemented, while
	/// conversion down to 1.0 or 1.1 should work also for 2.0 files.
	/// </remarks>
	public class ConvertResources
	{
		private string resource;
		public Exception ExecutionException = null;

		public static Regex findVersion = new Regex(@"Version=(?<vn>\d+\.\d+\.\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		// e.g. PublicKeyToken=b77a5c561934e089
		public static Regex findPubKeyToken = new Regex(@"PublicKeyToken=(?<pkt>[a-fA-F0-9]{16})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		
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
					int changes = ConvertResSchema(docToConvert, version, testOnly);
					
					if (isWindowsFormsResource) {
						try {
							changes += ConvertResHeader(docToConvert, version, testOnly);
							changes += ConvertResTypes(docToConvert, version, testOnly);
						} catch (Exception taskEx) {
							return ExitWith(taskEx);
						}
					} else {
						try {
							changes += ConvertResHeader(docToConvert, version, testOnly);
							changes += ConvertResTypes(docToConvert, version, testOnly);
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
				
				if (idName == "version"){
					if (version == CompatibleCLRVersion.Version_1_0) {
						if ("1.0.0.0" != srcValue.InnerText) {
							srcValue.InnerText = "1.0.0.0";
							changes++;
						}
					} else
					if (version == CompatibleCLRVersion.Version_1_1) {
						if ("1.0.0.0" != srcValue.InnerText) {
							srcValue.InnerText = "1.0.0.0";
							changes++;
						}
					} else
					if (version == CompatibleCLRVersion.Version_2_0) {
						if ("2.0" != srcValue.InnerText) {
							srcValue.InnerText = "2.0";
							changes++;
						}
					}
				}

				if (idName == "reader"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3102.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3102.0");
							changes++;
						}
					} else
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}  else
					if (version == CompatibleCLRVersion.Version_2_0) {
						if (v != null && v != "2.0.0.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "2.0.0.0");
							changes++;
						}
					}
				}

				if (idName == "writer"){
					string v = GetVersion(srcValue.InnerText);
					if (version == CompatibleCLRVersion.Version_1_0) {
						if (v != null && v != "1.0.3102.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3102.0");
							changes++;
						}
					} else
					if (version == CompatibleCLRVersion.Version_1_1) {
						if (v != null && v != "1.0.5000.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
							changes++;
						}
					}  else
					if (version == CompatibleCLRVersion.Version_2_0) {
						if (v != null && v != "2.0.0.0") {
							srcValue.InnerText = srcValue.InnerText.Replace(v, "2.0.0.0");
							changes++;
						}
					}
				}

			}
			return changes;
		}

		public static int ConvertResTypes(XmlDocument doc, CompatibleCLRVersion version, bool verbose) {
			int changes = 0;

			foreach (XmlNode sn in doc.SelectNodes("/root/data", GetNamespaceManager(doc))) {
				XmlAttribute name = sn.Attributes["name"];
				XmlAttribute type = sn.Attributes["type"];
				XmlAttribute space = sn.Attributes["xml:space"];
				XmlNode srcValue = sn.SelectSingleNode("value");

				if (name != null && name.InnerText.StartsWith(">>") && name.InnerText.EndsWith(".Type")) {
					
					string v = GetVersion(srcValue.InnerText);
					string t = GetPublicKeyToken(srcValue.InnerText);
					
					// convert only CLR framework types (Microsoft):
					if (t == "b03f5f7f11d50a3a" || t == "b77a5c561934e089") {
						
						if (RepairTypePublicKeyToken(srcValue, t))
							changes++;

						if (version == CompatibleCLRVersion.Version_1_0) {
							
							if (v != null && v != "1.0.3300.0" ) {
								srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.3300.0");
								changes++;
							}

						} else
					
							if (version == CompatibleCLRVersion.Version_1_1) {
							if (v != null && v != "1.0.5000.0") {
								srcValue.InnerText = srcValue.InnerText.Replace(v, "1.0.5000.0");
								changes++;
							}

						} else
					
							if (version == CompatibleCLRVersion.Version_2_0) {
							//TODO: remove the ", Version=..., Culture=... " part
							//TODO: add assembly aliases
						}
					}
				}
				
				if (type != null){
					string v = GetVersion(type.InnerText);
					string t = GetPublicKeyToken(type.InnerText);
					
					// convert only CLR framework types (Microsoft):
					if (t == "b03f5f7f11d50a3a" || t == "b77a5c561934e089") {
						
						if (RepairTypePublicKeyToken(type, t))
							changes++;

						if (version == CompatibleCLRVersion.Version_1_0) {
							
							if (v != null && v != "1.0.3300.0") {
								type.InnerText = type.InnerText.Replace(v, "1.0.3300.0");
								changes++;
							} 

						} else
					
							if (version == CompatibleCLRVersion.Version_1_1) {
							if (v != null && v != "1.0.5000.0" ) {
								type.InnerText = type.InnerText.Replace(v, "1.0.5000.0");
								changes++;
							} 

						} else
					
							if (version == CompatibleCLRVersion.Version_2_0) {
							//TODO: remove the ", Version=..., Culture=... " part
							//TODO: add assembly aliases
						}
					}
				}

				if (space != null) {
					if (version == CompatibleCLRVersion.Version_1_0 ||
						version == CompatibleCLRVersion.Version_1_1) {
						sn.Attributes.Remove(space);
						changes++;
					}
				}

				if (srcValue != null && srcValue.Attributes["xml:space"] != null) {
					if (version == CompatibleCLRVersion.Version_1_0 ||
						version == CompatibleCLRVersion.Version_1_1) {
						XmlAttribute srcSp = srcValue.Attributes["xml:space"];
						srcValue.Attributes.Remove(srcSp);
						changes++;
					}
				}
			}

			if (version == CompatibleCLRVersion.Version_1_0 ||
				version == CompatibleCLRVersion.Version_1_1) {
				
				foreach (XmlNode sn in doc.SelectNodes("/root/assembly")) {
					doc.DocumentElement.RemoveChild(sn);
					changes++;
				}
			}

			return changes;
		}

		private static bool RepairTypePublicKeyToken(XmlNode n, string containedToken) {
			// convert only CLR framework types (Microsoft):
			if (containedToken == "b03f5f7f11d50a3a" || containedToken == "b77a5c561934e089") {
				
				if (n.InnerText.IndexOf("System.Drawing") != -1) {
					if (containedToken != "b03f5f7f11d50a3a") {
						// have to have token "b03f5f7f11d50a3a"
						n.InnerText = n.InnerText.Replace(containedToken, "b03f5f7f11d50a3a");
						return true;
					}
				} else {
					if (containedToken != "b77a5c561934e089") {
						// have to have token "b77a5c561934e089"
						n.InnerText = n.InnerText.Replace(containedToken, "b77a5c561934e089");
						return true;
					}
				}
			}
			return false;
		}

		public static int ConvertResSchema(XmlDocument doc, CompatibleCLRVersion version, bool verbose) {
			int changes = 0;

			foreach (XmlNode sn in doc.SelectNodes("/root/xsd:schema", GetNamespaceManager(doc))) {
				if (version == CompatibleCLRVersion.Version_1_0 ||
					version == CompatibleCLRVersion.Version_1_1) {
					sn.InnerXml = res_schema_1_x;
					changes++;
				} else if (version == CompatibleCLRVersion.Version_2_0) {
					XmlNode newNode = doc.CreateElement("xsd", "schema", "http://www.w3.org/2001/XMLSchema");
					XmlAttribute a = doc.CreateAttribute("id", null);
					a.Value = "root";
					newNode.Attributes.Append(a);
					sn.ParentNode.InsertBefore(newNode, sn);
					newNode.InnerXml = res_schema_2_0;	// fails with unknown namespace?
					doc.RemoveChild(sn);
					//sn.InnerXml = res_schema_2_0;
					changes++;
				}
			}

			return changes;
		}

		private static string GetVersion(string source) {
			if (source == null)
				return null;
			Match m = findVersion.Match(source);
			if (m != null && m.Groups.Count > 0) {
				string f = m.Groups["vn"].Value;
				if (f == null) return null;
				if (f.Length == 0) return null;
				return f;
			}
			return null;
		}

		private static string GetPublicKeyToken(string source) {
			if (source == null)
				return null;
			Match m = findPubKeyToken.Match(source);
			if (m != null && m.Groups.Count > 0) {
				string f = m.Groups["pkt"].Value;
				if (f == null) return null;
				if (f.Length == 0) return null;
				return f;
			}
			return null;
		}

		private static XmlNamespaceManager GetNamespaceManager(XmlDocument doc) {
			XmlNamespaceManager m = new XmlNamespaceManager(doc.NameTable);
			m.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
			m.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
			return m;
		}

		// <xsd:schema id='root' xmlns='' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
		private const string res_schema_2_0 = @"
		<xsd:import namespace='http://www.w3.org/XML/1998/namespace' />
		<xsd:element name='root' msdata:IsDataSet='true'>
		<xsd:complexType>
		<xsd:choice maxOccurs='unbounded'>
				 <xsd:element name='metadata'>
						  <xsd:complexType>
		<xsd:sequence>
		<xsd:element name='value' type='xsd:string' minOccurs='0' />
		</xsd:sequence>
		<xsd:attribute name='name' use='required' type='xsd:string' />
		<xsd:attribute name='type' type='xsd:string' />
		<xsd:attribute name='mimetype' type='xsd:string' />
		<xsd:attribute ref='xml:space' />
		</xsd:complexType>
		</xsd:element>
		<xsd:element name='assembly'>
				 <xsd:complexType>
		<xsd:attribute name='alias' type='xsd:string' />
		<xsd:attribute name='name' type='xsd:string' />
		</xsd:complexType>
		</xsd:element>
		<xsd:element name='data'>
				 <xsd:complexType>
		<xsd:sequence>
		<xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
		<xsd:element name='comment' type='xsd:string' minOccurs='0' msdata:Ordinal='2' />
		</xsd:sequence>
		<xsd:attribute name='name' type='xsd:string' use='required' msdata:Ordinal='1' />
		<xsd:attribute name='type' type='xsd:string' msdata:Ordinal='3' />
		<xsd:attribute name='mimetype' type='xsd:string' msdata:Ordinal='4' />
		<xsd:attribute ref='xml:space' />
		</xsd:complexType>
		</xsd:element>
		<xsd:element name='resheader'>
				 <xsd:complexType>
		<xsd:sequence>
		<xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
		</xsd:sequence>
		<xsd:attribute name='name' type='xsd:string' use='required' />
		</xsd:complexType>
		</xsd:element>
		</xsd:choice>
		</xsd:complexType>
		</xsd:element>";
		//</xsd:schema>";

		// <xsd:schema id='root' xmlns='' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
		private const string res_schema_1_x =@"
		<xsd:element name='root' msdata:IsDataSet='true'>
		<xsd:complexType>
		<xsd:choice maxOccurs='unbounded'>
				 <xsd:element name='data'>
						  <xsd:complexType>
		<xsd:sequence>
		<xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
		<xsd:element name='comment' type='xsd:string' minOccurs='0' msdata:Ordinal='2' />
		</xsd:sequence>
		<xsd:attribute name='name' type='xsd:string' msdata:Ordinal='1' />
		<xsd:attribute name='type' type='xsd:string' msdata:Ordinal='3' />
		<xsd:attribute name='mimetype' type='xsd:string' msdata:Ordinal='4' />
		</xsd:complexType>
		</xsd:element>
		<xsd:element name='resheader'>
				 <xsd:complexType>
		<xsd:sequence>
		<xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
		</xsd:sequence>
		<xsd:attribute name='name' type='xsd:string' use='required' />
		</xsd:complexType>
		</xsd:element>
		</xsd:choice>
		</xsd:complexType>
		</xsd:element>";
		//</xsd:schema>";
	}
}
