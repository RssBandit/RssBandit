using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiffPatchResources
{
	/// <summary>
	/// Summary description for StringsFromResources.
	/// </summary>
	public class StringsFromResources
	{
		private string resource;
		private Regex findParams = new Regex("{[0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
			public Exception ExecutionException = null;


		public StringsFromResources(string resourceFileName) {
			this.resource = resourceFileName;
		}

		public bool Run(bool testOnly) {
			if (!File.Exists(resource)) 
				return ExitWith(new ArgumentException(String.Format("File does not exist: '{0}'", resource)));
			
			try {
				string baseName = Path.GetFileNameWithoutExtension(resource);
			
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

				using (StreamWriter writer = new StreamWriter(baseName + ".strings")) {
				
					WriteHeader(writer);
				
					foreach (XmlNode sn in docSource.SelectNodes("/root/data")) {

						XmlNode val = sn.SelectSingleNode("value");
						if (val == null || val.InnerText == null || val.InnerText.Length == 0)
							continue;

						XmlNode name = sn.Attributes.GetNamedItem("name");
						string idName = name.InnerText;

						XmlNode cmt = sn.SelectSingleNode("comment");
						if (cmt != null && cmt.InnerText != null && cmt.InnerText.Length > 0) {
							writer.WriteLine("; {0}", StripCrLf(cmt.InnerText));
						}

						string text = StripCrLf(val.InnerText);
						int pCount = GetParameterCount(text);
						writer.Write(idName);
						if (pCount > 0) writer.Write("(");
						for (int i = 0; i < pCount; i++) {
							writer.Write("object param{0}", i);
							if ((i + 1) < pCount)
								writer.Write(", ");
						}
						if (pCount > 0) writer.Write(") = ");
						else writer.Write(" = ");
						writer.WriteLine(text);
					}
				
					writer.Flush();
				}
			
				Console.WriteLine("\tCreated .strings");

			} catch (Exception ex) {
				
				this.ExecutionException = ex;
				return false;

			}
			return true;	// success
		}

		private void WriteHeader(StreamWriter writer) {
			writer.WriteLine("# Options are specified as lines starting with \"#!\"");
			writer.WriteLine("# Comments are lines starting with \";\" or \"#\"");
			writer.WriteLine("# To define the SR class public instead of internal (default):");
			writer.WriteLine("##! accessor_class_accessibility = public");
			writer.WriteLine("##! culture_info = My.SharedUICulture");
			writer.WriteLine("[strings]");	
		}

		private string StripCrLf(string s) {
			if (s == null)	
				return String.Empty;
			return s.Replace(Environment.NewLine, " ").Replace("  ", " ");
		}

		private int GetParameterCount(string s) {
			return findParams.Matches(s).Count;
		}
		private bool ExitWith(Exception ex) {
			this.ExecutionException = ex;
			return false;
		}
	}
}
