using System;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using RssBandit.Common.Logging;
using RssBandit.Resources;

namespace RssBandit
{
    internal class ApplicationExceptionHandler
    {
        private static readonly ILog _log = Log.GetLogger(typeof(ApplicationExceptionHandler));

   

        public void OnAppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is ThreadAbortException)
            {
                return; // ignore. We catch them already in the apropriate places
            }

            // this seems to be the only place to "handle" the 
            //		System.NullReferenceException: Object reference not set to an instance of an object.
            //			at System.Net.OSSOCK.WSAGetOverlappedResult(IntPtr socketHandle, IntPtr overlapped, UInt32& bytesTransferred, Boolean wait, IntPtr ignored)
            //			at System.Net.Sockets.OverlappedAsyncResult.CompletionPortCallback(UInt32 errorCode, UInt32 numBytes,NativeOverlapped* nativeOverlapped)
            // that occurs on some systems running behind a NAT/Router/Dialer network connection.
            // See also the discussions here: 
            // http://groups.google.com/groups?hl=de&ie=UTF-8&oe=UTF-8&q=WSAGetOverlappedResult+%22Object+reference+not+set%22&sa=N&tab=wg&lr=
            // http://groups.google.com/groups?hl=de&lr=&ie=UTF-8&oe=UTF-8&threadm=7P-cnbOVWf_pEtKiXTWc-g%40speakeasy.net&rnum=4&prev=/groups%3Fhl%3Dde%26ie%3DUTF-8%26oe%3DUTF-8%26q%3DWSAGetOverlappedResult%2B%2522Object%2Breference%2Bnot%2Bset%2522%26sa%3DN%26tab%3Dwg%26lr%3D
            // http://groups.google.com/groups?hl=de&lr=&ie=UTF-8&oe=UTF-8&threadm=3fd6eba3.432257543%40news.microsoft.com&rnum=3&prev=/groups%3Fhl%3Dde%26ie%3DUTF-8%26oe%3DUTF-8%26q%3DWSAGetOverlappedResult%2B%2522Object%2Breference%2Bnot%2Bset%2522%26sa%3DN%26tab%3Dwg%26lr%3D

            if (e.ExceptionObject is AccessViolationException)
            {
                string message = e.ExceptionObject.ToString();
                if (message.IndexOf("WSAGetOverlappedResult") >= 0 && message.IndexOf("CompletionPortCallback") >= 0)
                    _log.Debug("Unhandled exception ignored: ", (Exception) e.ExceptionObject);
                return; // ignore. See comment above :-(
            }

            DialogResult result = DialogResult.Cancel;

            // The log is an extra backup in case the stack trace doesn't
            // get fully included in the exception.
            //string logName = RssBanditApplication.GetLogFileName();
            try
            {
                Exception ex = (Exception) e.ExceptionObject;
                ExceptionDispatchInfo.Capture(ex);
                result = ShowExceptionDialog(ex);
            }
            catch (Exception fatal)
            {
                try
                {
                    Log.Fatal("Exception on publish AppDomainException.", fatal);
                    MessageBox.Show("Fatal Error: " + fatal.Message, "Fatal Error", MessageBoxButtons.OK,
                                    MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            // Exits the program when the user clicks Abort.
            if (result == DialogResult.Abort)
                Application.Exit();
        }

        // Creates the error message and displays it 
        public static DialogResult ShowExceptionDialog(Exception e)
        {
            return ShowExceptionDialog(e, false);
        }

        public static DialogResult ShowExceptionDialog(Exception e, bool resumable)
        {
            ExceptionManager.Publish(e);
            try
            {
                StringBuilder errorMsg =
                    new StringBuilder(String.Format(SR.ExceptionGeneralCritical, RssBanditApplication.GetLogFileName()));
                errorMsg.Append("\n" + e.Message);
                if (Application.MessageLoop && e.Source != null)
                    errorMsg.Append("\n@:" + e.Source);
                return
                    MessageBox.Show(errorMsg.ToString(),
                                    SR.GUIErrorMessageBoxCaption,
                                    (resumable ? MessageBoxButtons.AbortRetryIgnore : MessageBoxButtons.OK),
                                    MessageBoxIcon.Stop);
            }
            catch (Exception ex)
            {
                _log.Error("Critical exception in ShowExceptionDialog() ", ex);
                /* */
            }
            return DialogResult.Abort;
        }
    }
}
