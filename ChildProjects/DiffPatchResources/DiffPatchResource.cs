using System;
using System.IO;
using System.Xml;

namespace DiffPatchResources
{
	/// <summary>
	/// Summary description for DiffPatchResource.
	/// </summary>
	class DiffPatchResource
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args == null || args.Length == 0) {
				Console.WriteLine(@"missing parameter: path to main resource file (e.g. 'C:\Projects\My\myproject.resx')");
				return ;
			}
			
			bool convert = false;
			CompatibleCLRVersion version = CompatibleCLRVersion.Version_1_0;
			bool testOnly = false;
			foreach (string path in args) {
				
				if (path == "-t") {
					testOnly = true;
					continue;
				}

				if (path =="-c") {
					convert = true;
					continue;
				}

				if (convert) {
					if (path == "1.0") {
						version = CompatibleCLRVersion.Version_1_0;
						continue;
					} else if (path == "1.1") {
						version = CompatibleCLRVersion.Version_1_1;
						continue;
					}
				}

				string fp = Path.GetFullPath(path);
				if (!File.Exists(path) ) {
					if (File.Exists( Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)))
						fp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
				}

				DiffPatchTasks tasks = new DiffPatchTasks(fp);
				Console.WriteLine("Diff/Patch process {0} ... ", fp);
				if (!tasks.Run(testOnly)) {
					Console.WriteLine("failed: {0}", tasks.ExecutionException.Message);
				} else {
					Console.WriteLine("succeeds.");
					
					if (convert) {
						ConvertResources cr = new ConvertResources(fp);
						Console.WriteLine("Conversion process to {0} of {1} ... ", version, fp);
						if (!cr.Run(testOnly, version)) {
							Console.WriteLine("failed: {0}", cr.ExecutionException.Message);
						} else {
							Console.WriteLine("succeeds.");
						}
					}
				}
			}
			Console.WriteLine("Press a key...");
			Console.ReadLine();
		}
	
		public static XmlNode CopyOfNode(XmlNode source, XmlDocument creator) {
			XmlNode newNode = creator.CreateElement(source.LocalName);
			foreach (XmlNode attr in source.Attributes) {
				XmlAttribute childAttr = creator.CreateAttribute(attr.LocalName);
				childAttr.InnerText  = attr.InnerText;
				newNode.Attributes.Append(childAttr);
			}
			newNode.InnerXml = source.InnerXml;
			return newNode;
		}

	}
}
