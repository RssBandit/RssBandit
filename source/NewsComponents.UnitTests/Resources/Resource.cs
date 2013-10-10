
using System.IO;
using System.Reflection;

namespace NewsComponents.UnitTests.Resources
{
	static internal class Resource
	{
		public static Stream GetResourceAsStream(string resourceId)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Resource).Namespace + "." + resourceId);
		}
	}
}
