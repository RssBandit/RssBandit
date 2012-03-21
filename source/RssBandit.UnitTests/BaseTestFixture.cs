using System;
using System.IO;
using System.Reflection;

namespace RssBandit.UnitTests
{
	/// <summary>
	/// Base class for all the NUnit test fixtures in this 
	/// project.  Provides common functions, etc.
	/// </summary>
	public class BaseTestFixture
	{
        public static readonly string APP_NAME = "RssBanditUnitTests";
        public static readonly string UNPACK_DESTINATION_FORMAT = Path.Combine(Path.GetTempPath(), APP_NAME + ".Resources.{0}");

        /// <summary>For ease of clean-up. Embedded resources are written to this directory (or a sub dir).</summary>
        public string UNPACK_DESTINATION { get { return string.Format(UNPACK_DESTINATION_FORMAT, this.GetType().Name); } }

	    /// <summary>For ease of clean-up and set-up, the web server files are written here.</summary>
        public string WEBROOT_PATH { get { return UNPACK_DESTINATION + @"\WebRoot"; } }

	    const string RESOURCE_ROOT = "RssBandit.UnitTests.Resources.";

        public string FEEDS_DIR { get { return UNPACK_DESTINATION; } } 
		
		/// <summary>
		/// Returns a resource stream given a resource name.  Assumes the resource 
		/// is within the project folder "Resources" and the 
		/// </summary>
		/// <remarks>
		/// If your resource is within a subfolder of the "Resources" folder, delimit 
		/// folders with the "." character. For example, if your resource file is named 
		/// test.txt and is in the folder "WebRoot", the name should be "WebRoot.test.txt".
		/// </remarks>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		protected Stream GetResourceStream(string resourceName)
		{
			if(resourceName[0] == '.')
				resourceName = resourceName.Substring(1); //remove beginning .

			return Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_ROOT + resourceName);
		}

		/// <summary>
		/// Writes an embedded resource into a file.
		/// </summary>
		/// <param name="resourceName"></param>
		/// <param name="destination"></param>
		protected void UnpackResource(string resourceName, FileInfo destination)
		{
			using(StreamWriter writer = new StreamWriter(destination.FullName))
			{
				writer.Write(UnpackResource(resourceName));
				writer.Flush();
			}
			destination.Attributes = FileAttributes.Temporary;
		}

		/// <summary>
		/// Unpacks the resource to a string.
		/// </summary>
		/// <param name="resourceName">Name of the resource.</param>
		/// <returns></returns>
		protected string UnpackResource(string resourceName)
		{
			using(Stream resourceStream = GetResourceStream(resourceName))
			{
				using(StreamReader reader = new StreamReader(resourceStream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// Unpacks the resource directory.
		/// </summary>
		/// <param name="directory">Directory.</param>
		protected void UnpackResourceDirectory(string directory)
		{
			UnpackResourceDirectory(directory, new DirectoryInfo(UNPACK_DESTINATION));
		}

		/// <summary>
		/// Unpacks an embedded resource directory.  Does not (and cannot) support recursion.
		/// </summary>
		/// <remarks>
		/// <p>Given the resource path (use . instead of \) to an embedded resource 
		/// directory, unpacks every resource within the directory.  This uses a 
		/// simple process of looking for all resources with a prefix of the directory. 
		/// </p>
		/// <p>This will recreate the directory structure of the given directory, but 
		/// cannot handle recursion.
		/// </p>
		/// </remarks>
		/// <param name="directory"></param>
		/// <param name="destination"></param>
		protected void UnpackResourceDirectory(string directory, DirectoryInfo destination)
		{
			if(directory[0] == '.')
				directory = directory.Substring(1);
			
			string physicalDestination = Path.Combine(destination.FullName, directory.Replace(".", @"\"));
			if(!Directory.Exists(physicalDestination))
				Directory.CreateDirectory(physicalDestination);
			
			directory = RESOURCE_ROOT + directory;

			string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

			foreach(string resourceFullName in resourceNames)
			{
				if(resourceFullName.StartsWith(directory))
				{
					string resourceName = resourceFullName.Substring(RESOURCE_ROOT.Length);
					string resourceFileName = resourceFullName.Substring(directory.Length + 1);
					UnpackResource(resourceName, new FileInfo(Path.Combine(physicalDestination, resourceFileName)));
				}
			}
		}

		/// <summary>
		/// Deletes the directory.
		/// </summary>
		/// <param name="dir">Dir.</param>
		protected void DeleteDirectory(string dir)
		{
			if(Directory.Exists(dir))
				DeleteDirectory(new DirectoryInfo(dir));
		}

		/// <summary>
		/// Deletes the directory.
		/// </summary>
		/// <param name="dir">Dir.</param>
		protected void DeleteDirectory(DirectoryInfo dir)
		{
			foreach(FileInfo file in dir.GetFiles())
				file.Delete();
			foreach(DirectoryInfo subDir in dir.GetDirectories())
				DeleteDirectory(subDir);
		}
	}
}