#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Reflection; 
using System.IO;

namespace NewsComponents
{
	/// <summary>
	/// Helper class used to manage assembly Resources
	/// </summary>
	internal sealed class Resource 
	{
		#region Static part
		
		static readonly Resource InternalResource = new Resource();
		/// <summary>
		/// Gets a resource manager for the assembly resource file
		/// </summary>
		public static Resource Manager {
			get	{	return InternalResource; }
		}
		#endregion
		
		#region Instance part 
		
		/// <summary>
		/// Gets a resource stream with the messages used by the Bandit classes
		/// </summary>
		/// <param name="name">resource key</param>
		/// <returns>a resource stream</returns>
		public Stream GetStream( string name ){
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType().Namespace + "." + name); 
		}

		#endregion
	}
}
