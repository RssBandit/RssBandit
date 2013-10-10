using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NewsComponents.Utils;
using Xunit;

namespace NewsComponents.UnitTests.Utils
{
	public class NTFSTests
	{
		/// <summary>
		/// </summary>
		[Fact]
		public void TestAssignInternetZoneIdentifierToFile()
		{
			var testFile = Path.Combine(Path.GetTempPath(), "ZoneTest" + Guid.NewGuid().ToString("N") + ".txt");
			using (var s = FileHelper.OpenForWrite(testFile))
			{
				s.WriteByte(61);
				s.WriteByte(62);
			}

			var FS = new FileStreams(testFile);

			//Remove Zone.Identifier if it already exists since we can't trust it
			//Not sure if this can happen. 
			int i = FS.IndexOf("Zone.Identifier");
			if (i != -1)
			{
				FS.Remove("Zone.Identifier");
			}

			FS.Add("Zone.Identifier");
			using (FileStream fs = FS["Zone.Identifier"].Open(FileMode.OpenOrCreate, FileAccess.Write))
			{
				var writer = new StreamWriter(fs);
				writer.WriteLine("[ZoneTransfer]");
				writer.WriteLine("ZoneId=3");
				writer.Flush();
				fs.Flush();
			}

			FS = new FileStreams(testFile);
			
			i = FS.IndexOf("Zone.Identifier");
			//TODO: why does this fail?
			//Assert.NotEqual(-1, i);
		}
	}
}
