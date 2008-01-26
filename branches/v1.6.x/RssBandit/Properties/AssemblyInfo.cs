#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("RSS Bandit")]
[assembly: AssemblyDescription("Your desktop news aggregator")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RssBandit")]
[assembly: AssemblyCopyright("(C) 2003-2007 by www.rssbandit.org")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum)]

// MSDN Magazine July 2002, p. 94: marks our bundled resources as
// culture specific to save assembly resource resolve steps:
[assembly: NeutralResourcesLanguageAttribute("en-US")]
