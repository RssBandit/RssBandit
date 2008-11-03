using System;
using System.Xml;

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
		/// <summary>
		/// Inits the implementor.
		/// </summary>
		/// <param name="fromSource">From source.</param>
		/// <param name="verbose">if set to <c>true</c> [verbose].</param>
		void Init(XmlDocument fromSource, bool verbose);
		/// <summary>
		/// Gets the nodes appended.
		/// </summary>
		/// <value>The nodes appended.</value>
		int NodesAppended { get; }
		/// <summary>
		/// Gets the nodes changed.
		/// </summary>
		/// <value>The nodes changed.</value>
		int NodesChanged { get; }
		/// <summary>
		/// Gets the nodes removed.
		/// </summary>
		/// <value>The nodes removed.</value>
		int NodesRemoved { get; }
		/// <summary>
		/// Patches the target.
		/// </summary>
		/// <param name="docToPatch">The doc to patch.</param>
		/// <returns></returns>
		Exception PatchTarget(XmlDocument docToPatch);
	}

	public enum CompatibleCLRVersion {
		Version_1_0,
		Version_1_1,
		Version_2_0,
	}

	public interface IConversionTask {
		/// <summary>
		/// Inits the specified verbose.
		/// </summary>
		/// <param name="verbose">if set to <c>true</c> [verbose].</param>
		void Init(bool verbose);
		/// <summary>
		/// Gets the nodes appended.
		/// </summary>
		/// <value>The nodes appended.</value>
		int NodesAppended { get; }
		/// <summary>
		/// Gets the nodes changed.
		/// </summary>
		/// <value>The nodes changed.</value>
		int NodesChanged { get; }
		/// <summary>
		/// Gets the nodes removed.
		/// </summary>
		/// <value>The nodes removed.</value>
		int NodesRemoved { get; }
		Exception ConvertTarget(XmlDocument docToConvert, CompatibleCLRVersion toVersion);
	}
}
