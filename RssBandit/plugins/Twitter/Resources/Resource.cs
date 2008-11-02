using System;
using System.Resources;
using System.Reflection; 
using System.IO;

using System.Diagnostics;

namespace BlogExtension.Twitter {
	/// <summary>
	/// Helper class used to manage assembly Resources
	/// </summary>
	internal sealed class Resource {
		#region Static part
		private const string ResourceFileName = ".Resources.PluginsText";

		static Resource InternalResource = new Resource();
		/// <summary>
		/// Gets a resource manager for the assembly resource file
		/// </summary>
		public static Resource Manager {
			get	{	return InternalResource; }
		}
		#endregion
		
		#region Instance part 
		ResourceManager rm = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public Resource() {
			rm = new ResourceManager(this.GetType().Namespace + ResourceFileName, Assembly.GetExecutingAssembly());
		}

		/// <summary>
		/// Gets the message with the specified key from the assembly resource file
		/// </summary>
		public string this [ string key ]	{
			get	{
				return rm.GetString( key, System.Globalization.CultureInfo.CurrentUICulture );
			}
		}

		/// <summary>
		/// Gets the formatted message with the specified key and format arguments
		/// from the assembly resource file
		/// </summary>
		public string this [ string key, params object[] formatArgs ]	{
			get	{
				return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
			}
		}
		/// <summary>
		/// Gets a resource stream with the messages used by the Bandit classes
		/// </summary>
		/// <param name="name">resource key</param>
		/// <returns>a resource stream</returns>
		public Stream GetStream( string name ){
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType().Namespace + "." + name); 
		}

		/// <summary>
		/// Formats a message stored in the Data assembly resource file.
		/// </summary>
		/// <param name="key">resource key</param>
		/// <param name="formatArgs">format arguments</param>
		/// <returns>a formated string</returns>
		public string FormatMessage( string key, params object[] formatArgs )	{
			return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
		}

		#endregion
	}
}
