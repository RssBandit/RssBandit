#region CVS Version Header
/*
 * $Id: VersionInfo.cs,v 1.95 2005/05/10 18:57:15 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/10 18:57:15 $
 * $Revision: 1.95 $
 */
#endregion

using System.Reflection;
using System.Resources;

//
// Version information for an assembly consists of the following four values:
//
//      Major Version 
//      Minor Version 
//      Build Number (raise on each modification/build)
//      Revision (like SP's for a build)
//
// Please raise Build Number on each modification !!!
[assembly: AssemblyVersion("1.3.0.33")]

// This attribute attaches additional version information to 
// an assembly for documentation purposes only.
[assembly: AssemblyInformationalVersion("1.3.0")]

// Allows you to update a main assembly without having to update your satellite assembly, 
// or vice versa. When the main assembly is updated, its assembly version number is changed. 
// If you want to continue using the existing satellite assemblies, change the main assembly's 
// version number but leave the satellite contract version number the same. For example, 
// in your first release your main assembly version may be 1.0.0.0. The satellite contract 
// version and the assembly version of the satellite assembly will also be 1.0.0.0. 
// If you need to update your main assembly for a service pack, you can change the 
// assembly version to 1.0.0.1, while keeping the satellite contract version and the 
// satellite's assembly version as 1.0.0.0
[assembly: SatelliteContractVersion("1.3.0.26")]
