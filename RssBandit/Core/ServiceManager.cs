#region CVS Version Header
/*
 * $Id: ServiceManager.cs,v 1.7 2005/03/04 16:47:53 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/04 16:47:53 $
 * $Revision: 1.7 $
 */
#endregion

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using Logger = RssBandit.Common.Logging;

namespace AppInteropServices
{
	/// <summary>
	/// ServiceManager implements a similar algorithm described in
	/// http://msdn.microsoft.com/asp.net/using/building/components/default.aspx?pull=/library/en-us/dnaspp/html/searchforplugins.asp
	/// to find classes in assemblies that implements Syndication.Extensibility.IBlogExtension
	/// and NewsComponents.IChannelProcessor's
	/// </summary>
	[ReflectionPermission(SecurityAction.Demand, MemberAccess=true, Unrestricted=true)]
	[Serializable]
	public class ServiceManager
	{
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(ServiceManager));

		
		public ServiceManager() {}

		/// <summary>
		/// Walk all the assemblies in the specified directory looking 
		/// for classes that implements Syndication.Extensibility.IBlogExtension.
		/// </summary>
		/// <param name="path">path to search for service assemblies</param>
		/// <returns>ArrayList containing suitable processors found</returns>
		/// <permission cref="ReflectionPermission">Used to find extensions</permission>
		public static ArrayList SearchForIBlogExtensions(string path)
		{

			AppDomain loaderDomain = AppDomain.CreateDomain("loaderDomain");
			AppInteropServices.ServiceManager srvFinder = (AppInteropServices.ServiceManager)loaderDomain.CreateInstanceAndUnwrap(Assembly.GetAssembly(typeof(AppInteropServices.ServiceManager)).FullName, "AppInteropServices.ServiceManager");
			ArrayList extensions = srvFinder.SearchForIBlogExtensionTypes(path);
			AppDomain.Unload(loaderDomain);	//dismiss loaded DLL/EXE that do not provide the interface

			ArrayList extensionInstances = new ArrayList();
			foreach(Type foundType in extensions)	{
				try 
				{
					Syndication.Extensibility.IBlogExtension extension = (Syndication.Extensibility.IBlogExtension)Activator.CreateInstance(foundType);
					extensionInstances.Add(extension);
				} 
				catch (Exception ex) 
				{
					_log.Error("Plugin of type '" + foundType.FullName + "' could not be activated.", ex);
				}
			}

			return extensionInstances;

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
		public ArrayList SearchForIBlogExtensionTypes(string path) {
			Type blogExtensionType = typeof(Syndication.Extensibility.IBlogExtension) ;
			ArrayList foundTypes = new ArrayList();
			
			// check for permissions
			IPermission rp = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
			try {
				rp.Demand();
			} catch (SecurityException se) {
				_log.Debug("ServiceManager.SearchForIBlogExtensionTypes()", se);
				return foundTypes;
			}

			if (path == null || ! Directory.Exists(path) ) {
				return foundTypes ;
			}
			
			string[] fs1 = Directory.GetFiles(path,"*.exe");
			string[] fs2 = Directory.GetFiles(path,"*.dll");

			// merge
			string[] files = new string[fs1.Length+fs2.Length];
			fs1.CopyTo(files, 0);
			fs2.CopyTo(files, fs1.Length);

			foreach(string f in files)	{

				if (f == null || f.Length == 0) continue;

				try	{
					
					// try and load the assembly
					Assembly a = Assembly.LoadFrom(f) ;					

					// walk through all the types to see if anything implements IBlogExtension
					foreach(Type t in a.GetTypes())	{
						if(blogExtensionType.IsAssignableFrom(t))	{	// found one, add
							foundTypes.Add(t);
						}
					}//foreach type
			
				}	catch(Exception e){
					_log.Debug("ServiceManager.SearchForIBlogExtensionTypes()", e);
				}

			}

			return foundTypes;
		}

		
#if NIGHTCRAWLER

		/// <summary>
		/// Walk all the assemblies in the specified directory looking 
		/// for classes that implements NewsComponents.IChannelProcessor.
		/// </summary>
		/// <param name="path">path to search for service assemblies</param>
		/// <returns>ArrayList containing suitable processors found</returns>
		/// <permission cref="ReflectionPermission">Used to find extensions</permission>
		public static ArrayList SearchForIChannelProcessors(string path) {

			AppDomain loaderDomain = AppDomain.CreateDomain("loaderDomain");
			AppInteropServices.ServiceManager srvFinder = (AppInteropServices.ServiceManager)loaderDomain.CreateInstanceAndUnwrap(Assembly.GetAssembly(typeof(AppInteropServices.ServiceManager)).FullName, "AppInteropServices.ServiceManager");
			ArrayList plugins = srvFinder.SearchForIChannelProcessorTypes(path);
			AppDomain.Unload(loaderDomain);	//dismiss loaded DLL/EXE that do not provide the interface

			ArrayList instances = new ArrayList();
			foreach(Type foundType in plugins)	{
				try {
					NewsComponents.IChannelProcessor plugin = (NewsComponents.IChannelProcessor)Activator.CreateInstance(foundType);
					if (plugin != null)
						instances.Add(plugin);
				} 
				catch (Exception ex) {
					_log.Error("Plugin of type '" + foundType.FullName + "' could not be activated.", ex);
				}
			}

			return instances;

		}
		
		/// <summary>
		/// Called by static method SearchForIChannelProcessors(). Instance method is used to test/load available plugin's.
		/// The above static method ensure that all tested dll's are loaded into a separate AppDomain, that is 
		/// unloaded after detecting all valid IChannelProcessor's to save memory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>ArrayList of types, that impl. the IChannelProcessor interface</returns>
		/// <remarks>If you call this method directly, all tested dll/exe are loaded into the default AppDomain!</remarks>
		/// <permission cref="ReflectionPermission">Used to find plugins</permission>
		public ArrayList SearchForIChannelProcessorTypes(string path) {
			Type channelProcessorType = typeof(NewsComponents.IChannelProcessor) ;
			ArrayList foundTypes = new ArrayList();
			
			// check for permissions
			IPermission rp = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
			try {
				rp.Demand();
			} catch (SecurityException se) {
				_log.Debug("ServiceManager.SearchForIChannelProcessorTypes()", se);
				return foundTypes;
			}

			if (path == null || ! Directory.Exists(path) ) {
				return foundTypes ;
			}
			
			string[] fs1 = Directory.GetFiles(path,"*.exe");
			string[] fs2 = Directory.GetFiles(path,"*.dll");

			// merge
			string[] files = new string[fs1.Length+fs2.Length];
			fs1.CopyTo(files, 0);
			fs2.CopyTo(files, fs1.Length);

			foreach(string f in files)	{

				if (f == null || f.Length == 0) continue;

				try	{
					
					// try and load the assembly
					Assembly a = Assembly.LoadFrom(f) ;					

					// walk through all the types to see if anything implements IChannelProcessor
					foreach(Type t in a.GetTypes())	{
						if(channelProcessorType.IsAssignableFrom(t))	{	// found one, add
							foundTypes.Add(t);
						}
					}//foreach type
			
				}	catch(Exception e){
					_log.Debug("ServiceManager.SearchForIChannelProcessorTypes()", e);
				}

			}

			return foundTypes;
		}

#endif
	
	}

}
