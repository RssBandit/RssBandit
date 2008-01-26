using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	public class ShellGUIDs
	{
		public static Guid IID_IMalloc = 
			new Guid("{00000002-0000-0000-C000-000000000046}");
		public static Guid IID_IShellFolder = 
			new Guid("{000214E6-0000-0000-C000-000000000046}");
		public static Guid IID_IFolderFilterSite = 
			new Guid("{C0A651F5-B48B-11d2-B5ED-006097C686F6}");
		public static Guid IID_IFolderFilter = 
			new Guid("{9CC22886-DC8E-11d2-B1D0-00C04F8EEB3E}");

		// AutoComplete Guids
		public static Guid IID_IAutoCompList = 
			new Guid("{00BB2760-6A77-11D0-A535-00C04FD7D062}");
		
		public static Guid IID_IObjMgr =
			new Guid("{00BB2761-6A77-11D0-A535-00C04FD7D062}");

		public static Guid IID_IACList =
			new Guid("{77A130B0-94FD-11D0-A544-00C04FD7D062}");

		public static Guid IID_IACList2 =
			new Guid("{470141A0-5186-11D2-BBB6-0060977B464C}");

		public static Guid IID_ICurrentWorkingDirectory =
			new Guid("{91956D21-9276-11D1-921A-006097DF5BD4}"); 
		
		public static Guid CLSID_AutoComplete =
			new Guid("{00BB2763-6A77-11D0-A535-00C04FD7D062}");

		public static Guid CLSID_ACLHistory =
			new Guid("{00BB2764-6A77-11D0-A535-00C04FD7D062}");

		public static Guid CLSID_ACListISF =
			new Guid("{03C036F1-A186-11D0-824A-00AA005B4383}");

		public static Guid CLSID_ACLMRU =
			new Guid("{6756A641-dE71-11D0-831B-00AA005B4383}"); 

		public static Guid CLSID_ACLMulti =
			new Guid("{00BB2765-6A77-11D0-A535-00C04FD7D062}");
		
		public static Guid CLSID_ACLCustomMRU =
			new Guid("{6935DB93-21E8-4CCC-BEB9-9fE3C77A297A}");

	}
}