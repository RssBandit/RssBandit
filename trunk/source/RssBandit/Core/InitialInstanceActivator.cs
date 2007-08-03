#region CVS Version Header
/*
 * $Id: InitialInstanceActivator.cs,v 1.4 2006/10/31 13:36:35 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/10/31 13:36:35 $
 * $Revision: 1.4 $
 */
#endregion

// InitialInstanceActivator.cs: Contributed by Chris Sells [csells@sellsbrothers.com]
// Inspired by Mike Woodring
// Single instance detection and activation
#region Copyright © 2002-2003 The Genghis Group
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from the
 * use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not claim
 * that you wrote the original software. If you use this software in a product,
 * an acknowledgment in the product documentation is IsRequired, as shown here:
 * 
 * Portions Copyright © 2002-2003 The Genghis Group (http://www.genghisgroup.com/).
 * 
 * 2. No substantial portion of the source code of this library may be redistributed
 * without the express written permission of the copyright holders, where
 * "substantial" is defined as enough code to be recognizably from this library. 
*/
#endregion
#region Notes
// -Uses Application.UserAppDataPath to pick a unique string composed
//  of the app name, the app version and the user name. This
//  gets us a unique mutex name, channel name and port number for each
//  user running each app of a specific version.
#endregion
#region Usage
/*
TODO: Reference the System.Runtime.Remoting assembly
using SellsBrothers;
...
static void Main(string[] args) {
  // Check for initial instance, registering callback to consume args from other instances
  // Main form will be activated automatically
  OtherInstanceCallback callback = new OtherInstanceCallback(OnOtherInstance);
  if( InitialInstanceActivator.Activate(mainForm, callback, args) ) return;
  
  // Check for initial instance w/o registering a callback
  // Main form will still be activated automatically
  if( InitialInstanceActivator.Activate(mainForm) ) return;

  // Check for initial instance, registering callback to consume args from other instances
  // Main form from ApplicationContext will be activated automatically
  OtherInstanceCallback callback = new OtherInstanceCallback(OnOtherInstance);
  if( InitialInstanceActivator.Activate(context, callback, args) ) return;

  TODO: Run application
}

// Called from other instances
static void OnOtherInstance(string[] args) {
  TODO: Handle args from other instance
}
*/
#endregion

using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace RssBandit
{
  // Signature of method to call when another instance is detected
  public delegate void OtherInstanceCallback(string[] args);

  public class InitialInstanceActivator 
  {
	  private static int usedPort = IPEndPoint.MinPort;

	  public static int GetPort() {
		  if (usedPort == IPEndPoint.MinPort) {
			  int configPort = Win32.Registry.InstanceActivatorPort;
			  if (configPort != 0) {
				  usedPort = configPort;
			  } else {
				  Random rnd = new Random();
				  usedPort = rnd.Next(49153, 65535);	// The Dynamic and/or Private Ports are those from 49152 through 65535.
				  Win32.Registry.InstanceActivatorPort = usedPort;
			  }
		  }
		  return usedPort;
	  }

	  public static int Port {
		  get { return GetPort();  }
	  }

	  public static string GetChannelName() {
		  return GetChannelName(RssBanditApplication.GetUserPath());
	  }

	  public static string GetChannelName(string userApp) {
		  return userApp.ToLower().Replace(@"\", "_");
	  }

	  public static string ChannelName {
		  get { return GetChannelName(); }
	  }

	  public static string MutexName {
		  get { return ChannelName; }
	  }

	  public static bool Activate(Form mainForm) {
		  return Activate(new ApplicationContext(mainForm), null, null);
	  }

	  public static bool Activate(Form mainForm, OtherInstanceCallback callback, string[] args) {
		  return Activate(new ApplicationContext(mainForm), callback, args);
	  }

	  //TODO: when the mutex.ReleaseMutex() is called on the very first instance?
	  private static Mutex mutex;

	  public static bool Activate(ApplicationContext context, OtherInstanceCallback callback, string[] args) {
		  // Check for existing instance
		  bool createdNew = false;
		  mutex = new Mutex(true, MutexName, out createdNew);

		  if( !createdNew ) {
			  // Second instance
			  // Open remoting channel exposed from initial instance
			  string url = string.Format("tcp://localhost:{0}/{1}", Port, ChannelName);
			  MainFormActivator activator = (MainFormActivator)RemotingServices.Connect(typeof(MainFormActivator), url);

			  // Send arguments to initial instance and exit this one
			  activator.OnOtherInstance(args);
			  return true;
		  }

		  // initial instance code...
		  bool success = false; int maxRetry = 25;
		  while (!success && maxRetry > 0) {
			  try {
				  // Expose remoting channel to accept arguments from other instances
				  ChannelServices.RegisterChannel(new TcpChannel(Port));
				  success = true;
			  } catch (System.Net.Sockets.SocketException sx) {
				  maxRetry--;
				  if (maxRetry > 0 && sx.ErrorCode == 10048)	{ // WSAEADDRINUSE (10048) Address already in use.
					  usedPort++;
					  Win32.Registry.InstanceActivatorPort = usedPort;
				  } else {
					throw;
				  }
			  }
		  }
		  
		  /* for .NET 1.1 we may have to apply this fix
		   * to workaround the exception:
		   * "An unhandled exception of type 'System.Runtime.Serialization.SerializationException' 
		   * occurred in mscorlib.dll.
		   * Additional information: Because of security restrictions, the type System.Runtime.Remoting.ObjRef 
		   * cannot be accessed."

				  IDictionary prop = new Hashtable();
				  prop["bindTo"] = "127.0.0.1";
				  prop["port"] = 0; // pick a random open port
				  // need to add this due to issues with security and .net 1.1
				  prop["typeFilterLevel"] = "Full";
		    
				  // need to add this due to issues with security and .net 1.1
				  BinaryServerFormatterSinkProvider provider = new 
				   BinaryServerFormatterSinkProvider();
				  provider.TypeFilterLevel = Formatters.TypeFilterLevel.Full;
				  TcpChannel tcp = new TcpChannel(prop, null, provider);
				  ChannelServices.RegisterChannel(tcp);

		  */

		  RemotingServices.Marshal(new MainFormActivator(context, callback), ChannelName);
		  return false;
	  }

	  public class MainFormActivator : MarshalByRefObject {

		  public MainFormActivator(ApplicationContext context, OtherInstanceCallback callback) {
			  this.context = context;
			  this.callback = callback;
		  }

		  public override object InitializeLifetimeService() {
      
			  // We want an infinite lifetime as far as the
			  // remoting infrastructure is concerned
			  // (Thanks for Mike Woodring for pointing this out)
			  ILease lease = (ILease)base.InitializeLifetimeService();
			  lease.InitialLeaseTime = TimeSpan.Zero;
			  return(lease);
		  }

		  public void OnOtherInstance(string[] args) {
      
			  // Transition to the UI thread
			  if( this.context.MainForm.InvokeRequired ) {
				  OtherInstanceCallback cb = new OtherInstanceCallback(OnOtherInstance);
				  this.context.MainForm.Invoke(cb, new object[] { args });
				  return;
			  }

			  // Let the UI thread know about the other instance
			  if( this.callback != null ) this.callback(args);

			  // Activate the main form
			  context.MainForm.Activate();
		  }

		  ApplicationContext context;
		  OtherInstanceCallback callback;
	  }
  }
}
