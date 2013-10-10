using System;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;

namespace RssBandit.UnitTests.Utils
{
	/// <summary>
	/// Series of unit tests of the ShortcutHandler class.
	/// </summary>
	/// <remarks>
	/// Since the class lives in the RSSBandit.exe project, I've copied the class 
	/// into the unit test project purely for testing purposes.
	/// </remarks>
	[TestFixture]
	public class ShortcutManagerTests : BaseTestFixture
	{
		/// <summary>
		/// Purely a test for gathering info.
		/// </summary>
		[Test]
		public void TestInformational()
		{
			Console.WriteLine((int)Keys.KeyCode);
			Console.WriteLine((int)Keys.Alt);
			Console.WriteLine((int)Keys.Menu);
		}
		
		/// <summary>
		/// Loads the ValidSettings.xml file and tests its contents.
		/// </summary>
		[Test]
		public void TestLoadValidSettings()
		{
			ShortcutHandler manager = new ShortcutHandler();
			using(FileStream stream = File.OpenRead(UNPACK_DESTINATION + @"\Settings\ValidShortcutSettings.xml"))
			{
				manager.Load(stream);
			}
			
			Assert.AreEqual(System.Windows.Forms.Shortcut.F1, manager.GetShortcut("cmdTestOne"));
			Assert.AreEqual(System.Windows.Forms.Shortcut.Ctrl4, manager.GetShortcut("cmdTestTwo"));
			Assert.IsTrue(manager.IsShortcutDisplayed("cmdTestTwo"));
			Assert.AreEqual(System.Windows.Forms.Shortcut.None, manager.GetShortcut("cmdNotInSettings"));
		}

		/// <summary>
		/// Loads the DefaultSettingsWithNoWhitespace.xml file and tests its contents.
		/// </summary>
		[Test]
		public void TestLoadValidSettingsWithNoWhitespace()
		{
			ShortcutHandler manager = new ShortcutHandler();
			using(FileStream stream = File.OpenRead(UNPACK_DESTINATION + @"\Settings\DefaultSettingsWithNoWhitespace.xml"))
			{
				manager.Load(stream);
			}
			
			Assert.AreEqual(Shortcut.None, manager.GetShortcut("cmdNotInSettings"));
			Assert.AreEqual(Shortcut.CtrlE, manager.GetShortcut("cmdExportFeeds"));
			Assert.IsTrue(manager.IsCommandInvoked("BrowserCreateNewTab", Keys.N | Keys.Control));
		}

		/// <summary>
		/// Loads the ValidSettings.xml file and tests its contents.
		/// </summary>
		[Test]
		public void TestDefaultSettings()
		{
			ShortcutHandler manager = new ShortcutHandler();
			using(FileStream stream = File.OpenRead(UNPACK_DESTINATION + @"\Settings\DefaultSettings.xml"))
			{
				manager.Load(stream);
			}
			Assert.AreEqual(System.Windows.Forms.Shortcut.None, manager.GetShortcut("cmdNotInSettings"));
			Assert.IsTrue(manager.IsCommandInvoked("BrowserCreateNewTab", Keys.N | Keys.Control));
		}

		/// <summary>
		/// Sets up the settings files used for testing.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			base.UnpackResourceDirectory("Settings");
		}

		/// <summary>
		/// Deletes the test files.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			DeleteDirectory(UNPACK_DESTINATION);
		}
	}
}
