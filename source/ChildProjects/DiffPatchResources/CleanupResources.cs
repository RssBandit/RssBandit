using System;
using System.IO;
using System.Xml;

namespace DiffPatchResources
{
	/// <summary>
	/// CleanupResources: removes unwanted resource entries.
	/// </summary>
	public class CleanupResources
	{
		private string resource;
		public Exception ExecutionException = null;
		private static char[] pointsep = new char[]{'.'};
		
		public CleanupResources(string resourceFileName) {
			this.resource = resourceFileName;
		}

		public bool Run(bool testOnly) {
			if (!File.Exists(resource)) 
				return ExitWith(new ArgumentException(String.Format("File does not exist: '{0}'", resource)));
				
			string baseName = Path.GetFileNameWithoutExtension(resource);
			
			string[] languageResources = Directory.GetFiles(Path.GetDirectoryName(resource), baseName + ".*.resx");
			if (languageResources == null || languageResources.Length == 0)
				return ExitWith(new InvalidOperationException(String.Format("No dependent language resource files (e.g. '....de.resx') found related to: '{0}'", resource)));

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

			if (! isWindowsFormsResource) {
				Console.WriteLine("\tnothing to cleanup.");
			}


			foreach (string langResFile in languageResources) {

				XmlDocument docToCleanup = new XmlDocument();
				docToCleanup.PreserveWhitespace = true;
				docToCleanup.XmlResolver = null;
				
				try {
					Console.WriteLine(" + {0}  ", Path.GetFileName(langResFile));
					docToCleanup.Load(langResFile);
					
					int deleted = CleanupTarget(docSource, docToCleanup, testOnly);

					if (!testOnly && (deleted != 0)) {
						File.Copy(langResFile, langResFile + ".bak", true);
						docToCleanup.Save(langResFile);
					}

					Console.WriteLine("\tremoved data nodes:{0}", deleted);
				} catch (Exception ex) {
					return ExitWith(ex);
				}

			}

			return true;	// success
		}

		private int CleanupTarget(XmlDocument source, XmlDocument target, bool verbose) {
			
			int nodesRemoved = 0;
			string searchPath = "/root/data[@name='{0}']";

			// remove resources from lang. resx, that are no longer in the source
			foreach (XmlNode tn in target.SelectNodes("/root/data")) {
				XmlNode name = tn.Attributes.GetNamedItem("name");
				string idName = name.InnerText;
				
				XmlNode sourceNode = source.SelectSingleNode(String.Format(searchPath, idName));
				if (sourceNode == null) {
					// remove resources from lang. resx, that are no longer in the source
					tn.ParentNode.RemoveChild(tn);
					
					if (verbose)
						Console.WriteLine("REMOVED (source not found) " + idName);
					else
						Console.Write(".");
					nodesRemoved++;
				} else {
					// remove unwanted data nodes from lang. resx
					if (false == KeepNode(idName)) {
						tn.ParentNode.RemoveChild(tn);
					
						if (verbose)
							Console.WriteLine("REMOVED (unwanted) " + idName);
						else
							Console.Write(".");
						nodesRemoved++;
					}
				}
			}
			return nodesRemoved;
		}

		private bool KeepNode(string nodeName) {
			
			string[] nameparts = nodeName.Split(pointsep);
			if (nameparts.Length < 2)
				return true;
			string lookupPart = nameparts[nameparts.Length-1];

			if (lookupPart == "Size")
				return false;
			if (lookupPart == "Location")
				return false;
			if (nameparts[0] == "$this" && lookupPart == "Icon")
				return false;

			return true;
		}

		private bool ExitWith(Exception ex) {
			this.ExecutionException = ex;
			return false;
		}

	}
}
