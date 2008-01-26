using System;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// FileDocument is a concrete, lightweight implementation of IDocument
	/// for searching plain-text file-based resources.
	/// </summary>
	class FileDocument : IDocument
	{
		string m_data = null;
		string m_name;

		public FileDocument(string name)
		{
			m_name = name;
		}

		private bool LoadDocument()
		{
			try
			{
				m_data = new System.IO.StreamReader(m_name).ReadToEnd();
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
		
			if (m_data.IndexOf(str) != -1)
				return true;
		
			return false;
		}

		public string Name()
		{
			return m_name;
		}
	}
}
