using System;
using System.Collections.Generic;
using System.Text;
using Sgml;
using System.Xml;

namespace FastXPathReader
{
	public class FastSgmlXPathReader : SgmlReader
	{
		// Not a stack b/c we need to view the entire list to create the string representation
		private List<string> PositionTracker = new List<string>();

		// Used to build the string representation
		private StringBuilder XPathBuilder = new StringBuilder();

		// Override the Read() function to track changes to the XPath
		public override bool Read()
		{
			bool Value = base.Read();
			if (Value && base.NodeType == XmlNodeType.Element)
			{
				while (PositionTracker.Count > this.Depth)
				{
					// Remove any elements beyond this depth
					PositionTracker.RemoveAt(PositionTracker.Count - 1);
				}
				if (this.Depth != PositionTracker.Count)
				{
					// Add a new element at this depth
					PositionTracker.Add(this.Name);
				}
				else
				{
					// Change the element at this depth
					PositionTracker[PositionTracker.Count - 1] = this.Name;
				}
			}
			return Value;
		}

		// Build an XPath expression from the current location.
		public string XPath
		{
			get
			{
				XPathBuilder.Length = 0;
				XPathBuilder.Append("/");
				for (int i = 0; i < PositionTracker.Count; i++)
				{
					XPathBuilder.Append("/" + PositionTracker[i]);
				}
				return XPathBuilder.ToString();
			}
		}

		// Call the base constructors
		public FastSgmlXPathReader() : base() { }
	}
}
