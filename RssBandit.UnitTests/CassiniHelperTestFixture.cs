using System;
using System.IO;
using Cassini;

namespace RssBandit.UnitTests
{
	/// <summary>
	/// Base Test Fixture for unit tests involving Cassini.dll without 
	/// using the GAC.
	/// </summary>
	/// <remarks>
	/// This is a slight modification of a technique described by Scott Hanselman in his blog 
	/// http://www.hanselman.com/blog/PermaLink.aspx?guid=944a5284-6b8d-4366-81e8-2e241401e1b3
	/// </remarks>
	public class CassiniHelperTestFixture : BaseTestFixture
	{
		private Server _webServer;
		private readonly int _webServerPort = 8081;
		private readonly string _webServerVDir = "/";
		private string _webrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEBROOT_PATH);
		private string _webBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEBROOT_PATH + @"\bin");
		private string _webServerUrl; //built in Setup

		/// <summary>
		/// Setups the test fixture.  Call this from within a method 
		/// marked with the [TestFixtureSetUp] attribute.
		/// </summary>
		protected virtual void SetUp()
		{
			//NOTE: Cassini is going to load itself AGAIN into another AppDomain,
			// and will be getting it's Assembliesfrom the BIN, including another copy of itself!
			// Therefore we need to do this step FIRST because I've removed Cassini from the GAC
 
			//Copy our assemblies down into the web server's BIN folder
			Directory.CreateDirectory(_webBinPath);
			foreach(string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
			{
				string newFile = Path.Combine(_webBinPath,Path.GetFileName(file));
				if(File.Exists(newFile))
				{
					File.Delete(newFile);
				}
				File.Copy(file,newFile);
			}
 
			//Start the internal Web Server
			_webServer = new Server(_webServerPort, _webServerVDir, _webrootPath);
			_webServerUrl = String.Format("http://localhost:{0}{1}",_webServerPort,_webServerVDir);
			_webServer.Start();
 
			//Let everyone know
			Console.WriteLine(String.Format("Web Server started on port {0} with VDir {1} in physical directory {2}",_webServerPort,_webServerVDir,_webrootPath));
		}

		/// <summary>
		/// Call this method from a method labelled with the 
		/// [TestFixtureTearDown] attribute.
		/// </summary>
		protected virtual void TearDown()
		{
			try
			{
				if (_webServer != null)
				{
					_webServer.Stop();
					_webServer = null;
				}
				Directory.Delete(_webBinPath,true);
			}
			catch{}
		}

		/// <summary>
		/// Starts the web server.
		/// </summary>
		protected void StartWebServer()
		{
			if (_webServer != null)
			{
				_webServer.Start();
			}
		}

		/// <summary>
		/// Stops the web server.
		/// </summary>
		protected void StopWebServer()
		{
			if (_webServer != null)
			{
				_webServer.Stop();
			}
		}
	}
}
