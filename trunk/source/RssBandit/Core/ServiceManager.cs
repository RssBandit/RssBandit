#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using log4net;
using NewsComponents.Utils;
using RssBandit;
using RssBandit.Common.Logging;
using RssBandit.UIServices;
using Syndication.Extensibility;
using Logger = RssBandit.Common.Logging;

namespace AppInteropServices
{
    /// <summary>
    /// ServiceManager implements a similar algorithm described in
    /// http://msdn.microsoft.com/asp.net/using/building/components/default.aspx?pull=/library/en-us/dnaspp/html/searchforplugins.asp
    /// to find classes in assemblies that implements Syndication.Extensibility.IBlogExtension
    /// and RssBandit.UIServices.IAddInPackage.
    /// </summary>
    [ReflectionPermission(SecurityAction.Demand, MemberAccess=true, Unrestricted=true)]
    [Serializable]
    public class ServiceManager : IAddInManager
    {
        #region ivars

        private static readonly ILog _log = DefaultLog.GetLogger(typeof (ServiceManager));
        
        private static AppDomain CreateLoaderDomain(string relativeSearchPath)
        {
			var setup = new AppDomainSetup();
			AppDomainSetup currentSetup = AppDomain.CurrentDomain.SetupInformation;
			setup.LoaderOptimization = LoaderOptimization.SingleDomain;

			// We have to copy this, otherwise we have System.Security.Permissions.FileIOPermission while debugging!
			setup.ActivationArguments = currentSetup.ActivationArguments;
        	setup.ApplicationBase = currentSetup.ApplicationBase;
			setup.PrivateBinPath = relativeSearchPath;
			
			// the loader appDomain is for search/load/test for plugin files only. Not all dll/exe implement our type,
			// so we are able to unload the needless assemblies.
			AppDomain loaderDomain = AppDomain.CreateDomain("loaderDomain", null, setup);
        	return loaderDomain;      
        }

        private IList<IAddIn> addInList;

        #endregion

        #region IBlogExtension type loading support

        /// <summary>
        /// Walk all the assemblies in the specified directory looking 
        /// for classes that implements Syndication.Extensibility.IBlogExtension.
        /// </summary>
        /// <param name="path">path to search for service assemblies</param>
        /// <returns>ArrayList containing suitable processors found</returns>
        /// <permission cref="ReflectionPermission">Used to find extensions</permission>
        public static IList<IBlogExtension> SearchForIBlogExtensions(string path)
        {
        	AppDomain loaderDomain = null;
			try
			{
				Type managerType = typeof(ServiceManager);
			
				string fullPath = RssBanditApplication.GetPlugInPath();
				loaderDomain = CreateLoaderDomain(RssBanditApplication.GetPlugInRelativePath());
				ServiceManager srvFinder = (ServiceManager)loaderDomain.CreateInstanceAndUnwrap(
						Assembly.GetAssembly(managerType).FullName,
						managerType.FullName);

				IEnumerable<Type> extensions = srvFinder.SearchForIBlogExtensionTypes(fullPath);

				List<IBlogExtension> extensionInstances = new List<IBlogExtension>();
				foreach (Type foundType in extensions)
				{
					if (foundType.Equals(typeof(IBlogExtension)))
						continue;
					try
					{
						IBlogExtension extension = (IBlogExtension)Activator.CreateInstance(foundType);
						extensionInstances.Add(extension);
					}
					catch (Exception ex)
					{
						_log.Error("Plugin of type '" + foundType.FullName + "' could not be activated.", ex);
					}
				}

				return extensionInstances;
			} 
			finally
			{
				if (loaderDomain != null)
					AppDomain.Unload(loaderDomain);
			}
        }

        /// <summary>
        /// Called by static method SearchForIBlogExtensions(). Instance method is used to test/load available plugin's.
        /// The above static method ensure that all tested dll's are loaded into a separate AppDomain, that is 
        /// unloaded after detecting all valid IBlogExtension's to save memory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>ArrayList of types, that impl. the IBlogExtension interface</returns>
        /// <remarks>If you call this method directly, all tested dll/exe are loaded into the default AppDomain!</remarks>
        /// <permission cref="ReflectionPermission">Used to find extensions</permission>
        public IEnumerable<Type> SearchForIBlogExtensionTypes(string path)
        {
            Type blogExtensionType = typeof (IBlogExtension);
            List<Type> foundTypes = new List<Type>();

            // check for permissions
            IPermission rp = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
            try
            {
				rp.Demand();
            }
            catch (SecurityException se)
            {
                _log.Debug("ServiceManager.SearchForIBlogExtensionTypes()", se);
                return foundTypes;
            }

            if (path == null || ! Directory.Exists(path))
            {
                return foundTypes;
            }

            string[] fs1 = Directory.GetFiles(path, "*.exe");
            string[] fs2 = Directory.GetFiles(path, "*.dll");

            // merge
            string[] files = new string[fs1.Length + fs2.Length];
            fs1.CopyTo(files, 0);
            fs2.CopyTo(files, fs1.Length);

            foreach (string f in files)
            {
                if (string.IsNullOrEmpty(f)) 
					continue;

                try
                {
                    // try and load the assembly
                    Assembly a = AssemblyHelper.GetAssemblyFromFile(f);

					if (a == null) 
						continue;

                    // walk through all the types to see if anything implements IBlogExtension
                    foreach (Type t in a.GetTypes())
                    {
						if (blogExtensionType.IsAssignableFrom(t) && !t.IsInterface)
                        {
                            // found one, add
                            foundTypes.Add(t);
                        }
                    } //foreach type
                }
                catch (Exception e)
                {
                    _log.Debug("ServiceManager.SearchForIBlogExtensionTypes()", e);
                }
            }

            return foundTypes;
        }

        #endregion

        #region AddInPackage type loading support

        
        /// <summary>
        /// Walk all the AddIns assembly locations 
        /// for classes that implements RssBandit.UIServices.IAddInPackage.
        /// Then it builds the full AddInList by initializing the AddIn name to the
        /// base assembly file name and creates the type instances.
        /// </summary>
        /// <param name="assemblies">AddInList with AddIn assembly locations</param>
        /// <returns>AddInList containing suitable types implementing IAddInPackages found</returns>
        /// <permission cref="ReflectionPermission">Used to find IAddInPackages implementors</permission>
        public static IList<IAddIn> PopulateAndInitAddInPackages(IList<IAddIn> assemblies)
        {
			List<IAddIn> newList = new List<IAddIn>();
			if (assemblies == null || assemblies.Count == 0)
				return newList;

			AppDomain loaderDomain = null;
			try
			{
				loaderDomain = CreateLoaderDomain(RssBanditApplication.GetAddInInRelativePath());

				ServiceManager srvFinder =
					(ServiceManager)
					loaderDomain.CreateInstanceAndUnwrap(
						Assembly.GetAssembly(typeof(ServiceManager)).FullName,
						"AppInteropServices.ServiceManager");
				IDictionary<string, IEnumerable<Type>> addInTypes =
					srvFinder.SearchForIAddInPackagesTypes(assemblies);

				foreach (IAddIn a in assemblies)
				{
					if (a.Location == null || !addInTypes.ContainsKey(a.Location))
						continue;

					List<IAddInPackage> instances = new List<IAddInPackage>();
					IEnumerable<Type> foundTypes = addInTypes[a.Location];
					foreach (Type foundType in foundTypes)
					{
						try
						{
							IAddInPackage addInPackage = (IAddInPackage)Activator.CreateInstance(foundType);
							if (addInPackage != null)
								instances.Add(addInPackage);
						}
						catch (Exception ex)
						{
							_log.Error("AddIn of type '" + foundType.FullName + "' could not be activated.", ex);
						}
					}

					if (instances.Count > 0)
					{
						newList.Add(new AddIn(a.Location, Path.GetFileNameWithoutExtension(a.Location), instances));
					}
				}
				return newList;
			}
			finally
			{
				if (loaderDomain != null)
					AppDomain.Unload(loaderDomain);
			}
        }

        /// <summary>
        /// Called by static method SearchForIAddInPackages(). Instance method is used to test/load available 
        /// AddInPackages.
        /// The above static method ensure that all tested dll's are loaded into a separate AppDomain, that is 
        /// unloaded after detecting all valid IAddInPackages to save memory.
        /// </summary>
        /// <param name="addIns">addIns list with assembly locations set</param>
        /// <returns>Hashtable of types, that impl. the RssBandit.UIServices.IAddInPackage interface. 
        /// Key is the assembly location, item is a Type[] array</returns>
        /// <remarks>If you call this method directly, all tested dll/exe are 
        /// loaded into the default AppDomain!</remarks>
        /// <permission cref="ReflectionPermission">Used to find plugins</permission>
        public IDictionary<string, IEnumerable<Type>> SearchForIAddInPackagesTypes(IEnumerable<IAddIn> addIns)
        {
            Type packageType = typeof (IAddInPackage);
            Dictionary<string, IEnumerable<Type>> foundTypes = new Dictionary<string, IEnumerable<Type>>();

            // check for permissions
            IPermission rp = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
            try
            {
                rp.Demand();
            }
            catch (SecurityException se)
            {
                _log.Debug("ServiceManager.SearchForIAddInPackagesTypes()", se);
                return foundTypes;
            }

            if (addIns == null)
            {
                return foundTypes;
            }

            foreach (IAddIn f in addIns)
            {
                if (string.IsNullOrEmpty(f.Location)) continue;
                if (! File.Exists(f.Location)) continue;

                try
                {
                    // try and load the assembly
                    Assembly a = AssemblyHelper.GetAssemblyFromFile(f.Location);
					if (a == null)
						continue; 
					
					List<Type> typeArray = new List<Type>();
                    // walk through all the types to see if any implements IAddInPackage
                    foreach (Type t in a.GetTypes())
                    {
                        if (packageType.IsAssignableFrom(t))
                        {
                            // found one, add
                            typeArray.Add(t);
                        }
                    } //foreach type

                    if (typeArray.Count > 0)
                        foundTypes.Add(f.Location, typeArray);
                }
                catch (Exception e)
                {
                    _log.Debug("ServiceManager.SearchForIAddInPackagesTypes()", e);
                }
            }

            return foundTypes;
        }

        #endregion

        #region IAddInManager Members

        public void Unload(IAddIn addIn)
        {
            if (addIn == null) return;
            addInList.Remove(addIn);
            SaveAddInsToConfiguration(addInList, AddInConfigurationFile);
        }

        public IAddIn Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            if (File.Exists(fileName))
            {
                foreach (IAddIn a in AddIns)
                {
                    if (a.Location == fileName) // yet loaded
                        return null;
                }

                List<IAddIn> list = new List<IAddIn>();
                list.Add(new AddIn(fileName));

                // check if we can load and create a instance of the type:
                IList<IAddIn> ret = PopulateAndInitAddInPackages(list);
                if (ret != null && ret.Count > 0)
                {
                    IAddIn addin = ret[0];
                    addInList.Add(addin);

                    SaveAddInsToConfiguration(addInList, AddInConfigurationFile);
                    return addin;
                }
            }

            return null;
        }

        public IEnumerable<IAddIn> AddIns
        {
            get
            {
                if (addInList == null)
                {
                    addInList =  PopulateAndInitAddInPackages(LoadAddInFromConfiguration());
                }
                return addInList;
            }
        }

        #endregion

        #region Save/Restore AddIn to/from config file

        private static string AddInConfigurationFile
        {
            get
            {
                return Path.Combine(RssBanditApplication.GetUserPath(), "addins.cfg");
            }
        }

        private static IList<IAddIn> LoadAddInFromConfiguration()
        {
            return LoadAddInPathsFromConfiguration(AddInConfigurationFile);
        }

        private static IList<IAddIn> LoadAddInPathsFromConfiguration(string cfgFile)
        {
            List<IAddIn> list = new List<IAddIn>();
            if (File.Exists(cfgFile))
            {
                using (Stream s = FileHelper.OpenForRead(cfgFile))
                {
                    using (StreamReader r = new StreamReader(s, Encoding.UTF8))
                    {
                        string addInPath = r.ReadLine();
                        if (File.Exists(addInPath))
                        {
                            list.Add(new AddIn(addInPath));
                        }
                        else
                        {
                            _log.Error("Unable to locate configured AddIn at '" + addInPath + "'");
                        }
                    }
                }
            }
            return list;
        }

        private static void SaveAddInsToConfiguration(ICollection<IAddIn> addIns, string cfgFile)
        {
            if (addIns == null || addIns.Count == 0)
            {
                if (File.Exists(cfgFile))
                    FileHelper.Delete(cfgFile);
                return;
            }
            using (Stream s = FileHelper.OpenForWrite(cfgFile))
            {
                using (StreamWriter w = new StreamWriter(s, Encoding.UTF8))
                {
                    foreach (IAddIn addIn in addIns)
                    {
                        w.WriteLine(addIn.Location);
                    }
                }
            }
        }

        #endregion

        #region AddIn support classes

        [Serializable]
        private class AddIn : IAddIn
        {
            private readonly string _location;
            private readonly string _name;
            private readonly IList<IAddInPackage> _packages;

            public AddIn(string location, string name, IList<IAddInPackage> packages)
            {
                _location = location;
                _name = name;
                _packages = packages;
            }

            public AddIn(string location) : this(location, null, null)
            {
            }

            #region IAddIn Members

            public IList<IAddInPackage> AddInPackages
            {
                get
                {
                    return _packages;
                }
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public string Location
            {
                get
                {
                    return _location;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                // TODO:  Add AddIn.Dispose implementation
            }

            #endregion
        }


        #endregion
    }

	internal static class AssemblyHelper
	{
		private static readonly ILog _log = DefaultLog.GetLogger(typeof(AssemblyHelper));
        
		public static Assembly GetAssemblyFromFile(string fileName)
		{
            if (string.IsNullOrEmpty(fileName))
                return null;

            Assembly assemblyInstance = null;

			try
			{
                assemblyInstance = Assembly.LoadFile(fileName);    
			}
            catch (FileLoadException ex)
            {
                // Ignore it 
                _log.Error("Failed to load assembly " + fileName, ex);
            }
            catch (FileNotFoundException ex)
            {
                // Ignore it 
                _log.Error("Failed to load assembly " + fileName, ex);
            }

            return assemblyInstance;

		}
	}
}