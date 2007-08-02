using System;
using System.Xml;
using System.Xml.XPath;

namespace DiffPatchResources
{
	/// <summary>
	/// IDiffPatchTask to be implemented by classes that compare the main language 
	/// resource file with the various other language resources and apply some patching
	/// tasks to keep them synchronized:
	///  - all relevant resources are there
	///  - write over CHANGED resources (meaning changed)
	/// </summary>
	public interface IDiffPatchTask
	{
		void Init(XmlDocument fromSource, bool verbose);
		int NodesAppended { get; }
		int NodesChanged { get; }
		Exception PatchTarget(XmlDocument docToPatch);
	}

	public enum CompatibleCLRVersion {
		Version_1_0,
		Version_1_1,
	}

	public interface IConversionTask {
		void Init(bool verbose);
		int NodesAppended { get; }
		int NodesChanged { get; }
		Exception ConvertTarget(XmlDocument docToConvert, CompatibleCLRVersion toVersion);
	}
}
