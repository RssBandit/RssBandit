using System;
using System.Xml;

namespace DiffPatchResources
{
	/// <summary>
	/// Summary description for TaskFactory.
	/// </summary>
	public sealed class TaskFactory
	{
		
		public static IDiffPatchTask GetDiffPatchTask(XmlDocument source, bool isWindowsFormsResource, bool verbose) {
			
			if (isWindowsFormsResource) {
				formTask.Init(source, verbose);
				return formTask;
			} else {
				textTask.Init(source, verbose);
				return textTask;
			}

		}

		private TaskFactory(){}

		private static IDiffPatchTask textTask = new DiffPatchTaskTextResource();
		private static IDiffPatchTask formTask = new DiffPatchTaskFormResource();

	}
}
