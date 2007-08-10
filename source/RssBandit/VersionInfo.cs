#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Reflection;
using System.Resources;

//
// Version information for an assembly consists of the following four values:
//
//      Application Version: v.x.y.z
//		v = Major version number
//		x = minor version number
//		y = revision number
//		z = build number 
//
//	Major number is increased when there are significant jumps in functionality, 
//	the minor number is incremented when only minor features or significant fixes 
//	have been added, and the revision number is incremented when minor bugs are fixed. 
//	The build number should be incremented after each successful build/checkin. 
//
// Please raise Build Number on each modification !!!
[assembly: AssemblyVersion("1.5.0.16")]
[assembly: AssemblyFileVersion("1.5.0.16")]

// This attribute attaches additional version information to 
// an assembly for documentation purposes only.
[assembly: AssemblyInformationalVersion("1.5.16")]

// Allows you to update a main assembly without having to update your satellite assembly, 
// or vice versa. When the main assembly is updated, its assembly version number is changed. 
// If you want to continue using the existing satellite assemblies, change the main assembly's 
// version number but leave the satellite contract version number the same. For example, 
// in your first release your main assembly version may be 1.0.0.0. The satellite contract 
// version and the assembly version of the satellite assembly will also be 1.0.0.0. 
// If you need to update your main assembly for a service pack, you can change the 
// assembly version to 1.0.0.1, while keeping the satellite contract version and the 
// satellite's assembly version as 1.0.0.0
[assembly: SatelliteContractVersion("1.5.0.10")]
