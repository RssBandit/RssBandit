using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace DiffPatchResources {
	
	/// <summary>
	/// DiffPatchTaskTextResource patches Windows Text Resources.
	/// </summary>
	public class DiffPatchTaskTextResource: IDiffPatchTask {
		
		//TODO: extend the reg.expression to recognize all possible known String.Format's:
		private Regex findSimpleParamPlaceHolders = new Regex(@"\{[0-9]+\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		private bool verbose = false;
		private int nodesAppended = 0;
		private int nodesChanged = 0;
		private XmlDocument s = null;

		public DiffPatchTaskTextResource() {
			nodesAppended = 0;
			nodesChanged = 0;
			s = null;
		}

		#region IDiffPatchTask impl.
		void IDiffPatchTask.Init(XmlDocument source, bool verbose) {
			this.verbose = verbose;
			nodesAppended = 0;
			nodesChanged = 0;
			if ((this.s == null || this.s != source) && source != null) {
				this.s = source;
			}
		}

		Exception IDiffPatchTask.PatchTarget(XmlDocument target) {
			
			string searchPath = "/root/data[@name='{0}']";
			XmlNode rootNode = target.DocumentElement;

			foreach (XmlNode sn in s.SelectNodes("/root/data")) {
				XmlNode name = sn.Attributes.GetNamedItem("name");
				string idName = name.InnerText;
				XmlNode srcValue = sn.SelectSingleNode("value");
				int srcParamCount = 0;
				if (findSimpleParamPlaceHolders.IsMatch(srcValue.InnerText)){
					srcParamCount = findSimpleParamPlaceHolders.Matches(srcValue.InnerText).Count;
				}

				XmlNode targetNode = target.SelectSingleNode(String.Format(searchPath, name.InnerText));
				if (targetNode == null) {
					rootNode.AppendChild(DiffPatchResource.CopyOfNode(sn, target));
					rootNode.AppendChild(target.CreateWhitespace("\n\t"));
					if (verbose)
						Console.WriteLine("NEW " + idName);
					else
						Console.Write(".");
					nodesAppended++;
				} else {
					XmlNode comment = sn.SelectSingleNode("comment");
					if (comment != null && comment.InnerText.IndexOf("CHANGED") == 0) {
						rootNode.ReplaceChild(DiffPatchResource.CopyOfNode(sn, target), targetNode);
						if (verbose)
							Console.WriteLine("CHANGED " + idName);
						else
							Console.Write(".");
						nodesChanged++;
					} else {

						XmlNode targetValue = targetNode.SelectSingleNode("value");
						if (findSimpleParamPlaceHolders.IsMatch(targetValue.InnerText)){
							if (srcParamCount != findSimpleParamPlaceHolders.Matches(targetValue.InnerText).Count) {
								rootNode.ReplaceChild(DiffPatchResource.CopyOfNode(sn, target), targetNode);
								if (verbose)
									Console.WriteLine("PARAM mismatch " + idName);
								else
									Console.Write(".");
								nodesChanged++;
							}						
						}
					}
				}
			}
			return null;
		}

		int IDiffPatchTask.NodesChanged {
			get { return this.nodesChanged; }
		}
		int IDiffPatchTask.NodesAppended {
			get { return this.nodesAppended; }
		}

		#endregion

	}

	/// <summary>
	/// DiffPatchTaskFormResource patches Windows Forms Resources.
	/// </summary>
	public class DiffPatchTaskFormResource: IDiffPatchTask {
		
		private static char[] pointsep = new char[]{'.'};

		private bool verbose = false;
		private int nodesAppended = 0;
		private int nodesChanged = 0;
		private XmlDocument s;
		private Hashtable controlTypes;

		public DiffPatchTaskFormResource() {
			nodesAppended = 0;
			nodesChanged = 0;
			s = null;
		}

		#region IDiffPatchTask impl.
		void IDiffPatchTask.Init(XmlDocument source, bool verbose) {
			this.verbose = verbose;
			nodesAppended = 0;
			nodesChanged = 0;
			if ((this.s == null || this.s != source) && source != null) {
				this.s = source;
				this.controlTypes = this.BuildControlNamesLookup(s);
			}
		}

		Exception IDiffPatchTask.PatchTarget(XmlDocument target) {
			

			string searchPath = "/root/data[@name='{0}']";
			XmlNode rootNode = target.DocumentElement;

			foreach (XmlNode sn in s.SelectNodes("/root/data")) {
				if (!this.NodeRequired(sn))
					continue;
				
				XmlNode val = sn.SelectSingleNode("value");
				if (val == null || val.InnerText == null || val.InnerText.Length == 0)
					continue;

				XmlNode name = sn.Attributes.GetNamedItem("name");
				string idName = name.InnerText;
				XmlNode targetNode = target.SelectSingleNode(String.Format(searchPath, name.InnerText));
				if (targetNode == null) {
					rootNode.AppendChild(DiffPatchResource.CopyOfNode(sn, target));
					rootNode.AppendChild(target.CreateWhitespace("\n\t"));
					if (verbose)
						Console.WriteLine("NEW " + idName);
					else
						Console.Write(".");
					nodesAppended++;
				} else {
					XmlNode comment = sn.SelectSingleNode("comment");
					if (comment != null && comment.InnerText.IndexOf("CHANGED") == 0) {
						rootNode.ReplaceChild(DiffPatchResource.CopyOfNode(sn, target), targetNode);
						if (verbose)
							Console.WriteLine("CHANGED " + idName);
						else
							Console.Write(".");
						nodesChanged++;
					}
				}
			}

			return null;
		}

		int IDiffPatchTask.NodesChanged {
			get { return this.nodesChanged; }
		}
		int IDiffPatchTask.NodesAppended {
			get { return this.nodesAppended; }
		}

		#endregion

		private bool NodeRequired(XmlNode node) {
			
			XmlNode name = node.Attributes.GetNamedItem("name");

			string[] nameparts = name.InnerText.Split(pointsep);
			if (nameparts.Length < 2)
				return false;

			if (!this.controlTypes.ContainsKey(nameparts[0]))
				return false;

			if (nameparts[1] == "Text")
				return true;

			if (nameparts[1] == "ToolTip")
				return true;

			string typeinfo = (string)this.controlTypes[nameparts[0]];
			if (nameparts[1].StartsWith("Items")) {
				if (typeinfo.IndexOf("System.Windows.Forms.ListBox") >= 0)
					return true;
				if (typeinfo.IndexOf("System.Windows.Forms.ComboBox") >= 0)
					return true;
				if (typeinfo.IndexOf("System.Windows.Forms.ListView") >= 0)
					return true;
			}

			return false;
		}

		private Hashtable BuildControlNamesLookup(XmlDocument s) {
			Hashtable t = new Hashtable();
			foreach (XmlNode sn in s.SelectNodes("/root/data[starts-with(@name, '>>') and contains(@name,'.Type')]")) {
				XmlNode name = sn.Attributes.GetNamedItem("name");
				string key = name.InnerText.Substring(2).Replace(".Type", String.Empty);
				string type = sn.SelectSingleNode("value").InnerText;
				if (key != null)
					t.Add(key, type);
			}
			return t;
		}

	}

}
