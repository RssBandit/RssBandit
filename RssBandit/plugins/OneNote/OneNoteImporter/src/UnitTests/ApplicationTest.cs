using System;
using System.IO;
using Microsoft.Win32;
using NUnit.Framework;

namespace Microsoft.Office.OneNote.UnitTests
{
	/// <summary>
	/// Summary description for ApplicationTest.
	/// </summary>
	[TestFixture]
	public class ApplicationTest
	{
		[Test]
		public void Activate()
		{
			Application.Activate();
		}

		[Test]
		public void GetNotebookPath()
		{
			string notebookPath = "My Notebook";
			string saveKey = "Software\\Microsoft\\Office\\11.0\\OneNote\\Options\\Save";
			RegistryKey saveOptions = Registry.CurrentUser.OpenSubKey(saveKey);
			if (saveOptions != null)
			{
				notebookPath = saveOptions.GetValue("My Notebook path", notebookPath).ToString();
				saveOptions.Close();
			}

			string registryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
			                                   notebookPath);
			string applicationPath = Application.GetNotebookPath();
			Assert.AreEqual(registryPath, applicationPath);
		}

		[Test]
		public void GetExecutablePath()
		{
			Application.GetExecutablePath();
		}
	}
}