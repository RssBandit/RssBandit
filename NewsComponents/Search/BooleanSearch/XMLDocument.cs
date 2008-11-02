using System;
using System.Xml;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// XMLDocument is a concrete implementation of IDocument
	/// for searching XML-based resources.
	/// </summary>
	public class XMLDocument : IDocument
	{
		XmlDocument m_data = null;
		string m_name;

		public XMLDocument(string name)
		{
			m_name = name;
		}

		private bool LoadDocument()
		{
			try
			{
				m_data = new XmlDocument();
				m_data.Load(m_name);
			}
			catch(Exception)
			{
				return false;
			}

			return true;
		}

		public bool Find(string str)
		{
			if (m_data == null)
			{
				if (LoadDocument() == false)
					return false;
			}
		
			// Parse the document
			System.Xml.XmlNodeList nodes = m_data.DocumentElement.SelectNodes("//*");
			
			foreach(XmlNode node in nodes)
			{
				if (node.InnerText.IndexOf(str) != -1)
					return true;
			}

			return false;
		}

		public string Name()
		{
			return m_name;
		}
	}
}
