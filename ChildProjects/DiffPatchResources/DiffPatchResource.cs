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
			bool cleanup = false;
			bool makeStrings = false;
			
			foreach (string path in args) {
				
				if (path == "-?" || path == "/?") {
					WriteUsage();
					break;
				}

				if (path == "-t" || path == "/t") {
					testOnly = true;
					continue;
				}

				if (path =="-c" || path == "/c") {
					convert = true;
					continue;
				}

				if (path =="-d" || path == "/d") {
					cleanup = true;
					continue;
				}

				if (path =="-s" || path == "/s") {
					makeStrings = true;
					continue;
				}

				if (convert) {
					if (path == "1.0") {
						version = CompatibleCLRVersion.Version_1_0;
						continue;
					} else if (path == "1.1") {
						version = CompatibleCLRVersion.Version_1_1;
						continue;
					} else if (path == "2.0") {
						version = CompatibleCLRVersion.Version_2_0;
						continue;
					}
				}

				string fp = Path.GetFullPath(path);
				if (!File.Exists(path) ) {
					if (File.Exists( Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)))
						fp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
				}

				if (makeStrings) {
					StringsFromResources sfr = new StringsFromResources(fp);
					Console.WriteLine("Create .strings from {0} ... ", fp);
					if (!sfr.Run(testOnly)) {
						Console.WriteLine("failed: {0}", sfr.ExecutionException.Message);
					} else {
						Console.WriteLine("succeeds.");
					}
					break;
				}

				DiffPatchTasks tasks = new DiffPatchTasks(fp);
				Console.WriteLine("Diff/Patch process {0} ... ", fp);
				if (!tasks.Run(testOnly)) {
					Console.WriteLine("failed: {0}", tasks.ExecutionException.Message);
				} else {
					Console.WriteLine("succeeds.");
				}

				if (convert) {
					ConvertResources cr = new ConvertResources(fp);
					Console.WriteLine("Conversion process to {0} of {1} ... ", version, fp);
					if (!cr.Run(testOnly, version)) {
						Console.WriteLine("failed: {0}", cr.ExecutionException.Message);
					} else {
						Console.WriteLine("succeeds.");
					}
				}

				if (cleanup) {
					CleanupResources cr = new CleanupResources(fp);
					Console.WriteLine("Cleanup processing of {0}... ", fp);
					if (!cr.Run(testOnly)) {
						Console.WriteLine("failed: {0}", cr.ExecutionException.Message);
					} else {
						Console.WriteLine("succeeds.");
					}
				}

			}
			Console.WriteLine("Press a key...");
			Console.ReadLine();
		}
	
		private static void WriteUsage() {
			Console.WriteLine("Usage: DiffPatchResource [options] inputfile");
			Console.WriteLine();
			Console.WriteLine("  1. Can: read the main .resx inputfile and synchronize the data entries of any");
			Console.WriteLine("          language .resx with the same basename as the specified inputfile.");
			Console.WriteLine("  2. Can: read the main .resx inputfile and convert it to another CLR version.");
			Console.WriteLine("  3. Can: read the main .resx inputfile and create a .strings file.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine();
			Console.WriteLine("\t-?\t Display this text");
			Console.WriteLine("\t-t\t test only. Does not modify any .resx file(s)");
			Console.WriteLine("\t-c <version> convert to another CLR resx file version.");
			Console.WriteLine("\t\t Allowed <version>: '1.0', '1.1', '2.0'");
			Console.WriteLine("\t-d\t cleanup. Removes data entries from language .resx file(s),");
			Console.WriteLine("\t\t that are not anymore available in the main .resx inputfile");
			Console.WriteLine("\t-s\t create .strings file from the main .resx inputfile");
			Console.WriteLine();
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
