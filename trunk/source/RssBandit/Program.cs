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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using RssBandit.Resources;
using RssBandit.WinGui.Forms;

namespace RssBandit
{
    // Entrypoint
    static class Program
    {
        //private static readonly ILog _log = Log.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            bool running = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			// child threads should impersonate the current windows user
			AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
			
            /* setup handler for unhandled exceptions in non-debug modes */
            // Allow exceptions to be unhandled so they break in the debugger
#if !DEBUG

            ApplicationExceptionHandler eh = new ApplicationExceptionHandler();

            AppDomain.CurrentDomain.UnhandledException += eh.OnAppDomainException;
#endif

#if DEBUG && TEST_I18N_THISCULTURE			
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(new I18NTestCulture().Culture);
			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
#endif

            FormWindowState initialStartupState = Win32.GetStartupWindowState();
            // if you want to debug the minimzed startup (cannot be configured in VS.IDE),
            // comment out the line above and uncomment the next one:
            //FormWindowState initialStartupState =  FormWindowState.Minimized;

            RssBanditApplication appInstance = new RssBanditApplication();
            OtherInstanceCallback callback = appInstance.OnOtherInstance;
            try
            {
                running = InitialInstanceActivator.Activate(appInstance, callback, args);
            }
            catch (Exception /* ex */)
            {
                //_log.Error(ex); /* other instance is probably still running */
            }
            //_log.Info("Application v" + RssBanditApplication.VersionLong + " started, running instance is " + running);

            RssBanditApplication.StaticInit();
            if (!running)
            {
                // init to system default:
                RssBanditApplication.SharedCulture = CultureInfo.CurrentCulture;
                RssBanditApplication.SharedUICulture = CultureInfo.CurrentUICulture;

                if (appInstance.HandleCommandLineArgs(args))
                {
                    if (!string.IsNullOrEmpty(appInstance.CommandLineArgs.LocalCulture))
                    {
                        try
                        {
                            RssBanditApplication.SharedUICulture =
                                CultureInfo.CreateSpecificCulture(appInstance.CommandLineArgs.LocalCulture);
                            RssBanditApplication.SharedCulture = RssBanditApplication.SharedUICulture;
                        }
                        catch (Exception ex)
                        {
                            appInstance.MessageError(String.Format(
                                SR.ExceptionProcessCommandlineCulture, 
								appInstance.CommandLineArgs.LocalCulture,
                                ex.Message));
                        }
                    }

                    // take over customized cultures to current main thread:
                    Thread.CurrentThread.CurrentCulture = RssBanditApplication.SharedCulture;
                    Thread.CurrentThread.CurrentUICulture = RssBanditApplication.SharedUICulture;

                    if (!appInstance.CommandLineArgs.StartInTaskbarNotificationAreaOnly &&
                        initialStartupState != FormWindowState.Minimized)
                    {
                        // no splash, if start option is tray only or minimized
                        Splash.Show(SR.AppLoadStateLoading,
							String.Format(String.IsNullOrEmpty(RssBanditApplication.versionPostfix) ? "v{0}": "v{0} {1}",
								RssBanditApplication.Version, RssBanditApplication.versionPostfix )
							);
                    }

					if (appInstance.Init())
					{
						// does also run the windows event loop:
						appInstance.StartMainGui(initialStartupState);
						Splash.Close();
					}
					else
						return 3;	// init error

                	return 0; // OK
                }
            	return 2; // CommandLine error
            }
        	return 1; // other running instance
        }

    }
}
