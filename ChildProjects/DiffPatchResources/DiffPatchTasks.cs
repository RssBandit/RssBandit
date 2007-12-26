using System;
using System.IO;
using System.Xml;

namespace DiffPatchResources
{
	/// <summary>
	/// Summary description for DiffPatchTasks.
	/// </summary>
	public class DiffPatchTasks
	{
		private string resource;
		public Exception ExecutionException = null;

		public DiffPatchTasks(string resourceFileName)
		{
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

			foreach (string langResFile in languageResources) {
				XmlDocument docToPatch = new XmlDocument();
				docToPatch.PreserveWhitespace = true;
				docToPatch.XmlResolver = null;
				try {
					Console.WriteLine(" + {0}  ", Path.GetFileName(langResFile));
					docToPatch.Load(langResFile);
					IDiffPatchTask task = TaskFactory.GetDiffPatchTask(docSource, isWindowsFormsResource, testOnly);
					Exception taskEx = task.PatchTarget(docToPatch);
					if (taskEx != null) 
						return ExitWith(taskEx);
					if (!testOnly && (task.NodesAppended != 0 || task.NodesChanged != 0)) {
						File.Copy(langResFile, langResFile + ".bak", true);
						docToPatch.Save(langResFile);
					}

					Console.WriteLine("\tnew:{0}, changes:{1}", task.NodesAppended, task.NodesChanged);
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
	}
}
