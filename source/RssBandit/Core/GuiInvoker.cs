using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NewsComponents.Utils;

//TODO: Remove this class once transition to WPF is complete. 

namespace RssBandit
{
    /// <summary>
    /// This class fixes an issue that existed in 1.1 and 2.0, where the control could be in the middle
    /// of disposing while you are trying to call invoke on it.  This would result in object disposed exceptions.
    /// IMPORTANT: Initialize() method must be called from your UI thread prior to the invoke methods being called.
    /// </summary>
    [DebuggerNonUserCode]
    public static class GuiInvoker
    {
        #region Public Methods

        /// <summary>
        /// This method MUST be called from your main UI thread prior to the invoke methods being called.
        /// </summary>
        public static void Initialize()
        {
            if (_context == null)
                _context = GetMarshalingControl();
        }

        /// <summary>
        /// Invoke Async with control, delegate and parameters
        /// </summary>
        /// <param name="control">Supply control for invoking on a control instance method</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void InvokeAsync(Control control, Action action)
        {
            EnsureInitialized();

            DoInvoke(control, action, _context, false);
        }

        /// <summary>
        /// Synchronous Invoke with control method
        /// </summary>
        /// <param name="control">Supply control for invoking on a control instance method</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void Invoke(Control control, Action action)
        {
            EnsureInitialized();

            DoInvoke(control, action, _context, true);
        }

        #endregion Public Methods

        #region Private Methods

        private static void SafeInvoke(object state)
        {
            var d = (InvokeControl) state;

            // Check these from the UI thread to prevent a race condition
            if (d.Disposing || d.IsDisposed)
            {
                return;
            }

            // for sync code, we catch the exception and rethrow ourselves,
            // after preserving the stack trace
            if (d.IsSync)
            {
                try
                {
                    d.Action();
                }
                catch (Exception e)
                {
                    d.Exception = e;
                }
            }
            else
                d.Action();
        }

        private static void EnsureInitialized()
        {
            if (_context == null)
                throw new InvalidOperationException("Initialize must be called first.");
        }

        private static void DoInvoke(Control control, Action action, Control context, bool synchronous)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (context == null)
                throw new ArgumentNullException("context");


            if (control == null)
                control = context;


            var invokeControl = new InvokeControl(control, action, synchronous);

            if (synchronous)
            {
                // if we're already on the right thread, just execute
                if (control.IsHandleCreated && !control.InvokeRequired)
                {
                    SafeInvoke(invokeControl);
                    if (invokeControl.Exception != null)
                    {
                        invokeControl.Exception.PreserveExceptionStackTrace();
                        throw invokeControl.Exception;
                    }
                    return;
                }
            }

            if (synchronous)
            {
                context.Invoke((WaitCallback) SafeInvoke, invokeControl);

                if (invokeControl.Exception != null)
                {
                    invokeControl.Exception.PreserveExceptionStackTrace();
                    throw invokeControl.Exception;
                }
            }
            else
                context.BeginInvoke((WaitCallback) SafeInvoke, invokeControl);
        }


        /// <summary>
        /// Get a reference to the control used for GUI marshalling
        /// </summary>
        /// <returns></returns>
        private static Control GetMarshalingControl()
        {
            /*
           * We use this code instead of the SynchronizationContext to workaround
           * a defect in the way WinForms interacts with the context.
           * 
           * The issue is that Windows Forms doesn't support the correct 
           * delegate type in its internal marshaling machinery.  The 
           * SynchronizationContext uses a SendOrPostCallback delegate and 
           * that gets sent over to the marshaling control as either an Invoke 
           * or BeginInvoke.  In the implementation of Control.Invoke/BeginInvoke, 
           * they check for a few well-known delegate types for early binding.  
           * They check for EventHandler, MethodInvoker and WaitCallback.  
           * If the supplied delegate is one of those types, they cast to it
           * and early-bind/invoke it.  Otherwise, they do a late-bound dynamic 
           * invoke on the delegate type.  
           * 
           * There are two side-effects of this behavior:
           * 1) Late-bind calls are an order of magnitude slower than early-bound 
           *    ones.  Now it's still very fast, but it adds up
           * 2) Any exception in a late-bound call is caught, wrapped in a 
           *    TargetInvocationException and rethrown. This makes it impossible 
           *    for VS to get to the point of the exception when it's unhandled.  
           *    (First-chance still works.)
           * 
           * The WPF synchronization context does not suffer from this issue as the
           * Dispatcher's marshalling mechanism does check for the SendOrPostCallback 
           * delegate type and early-binds.  This was an error of omission by the 
           * Windows Forms 2.0 team.  By using the same internal control and using 
           * one of the "supported" delegate types, we get the desired behavior.
           * 
          */

            Type context =
                Type.GetType(
                    "System.Windows.Forms.Application+ThreadContext, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            MethodInfo current = context.GetMethod("FromCurrent", BindingFlags.NonPublic | BindingFlags.Static);

            PropertyInfo prop = context.GetProperty("MarshalingControl", BindingFlags.Instance | BindingFlags.NonPublic);

            object thread = current.Invoke(null, null);

            var control = (Control) prop.GetValue(thread, null);

            return control;
        }

        #endregion

        #region Private Class

        /// <summary>
        /// Invoke data class used internally.
        /// </summary>
        private class InvokeControl
        {
            private readonly Control control;


            public InvokeControl(Control targetObject, Action action, bool isSync)
            {
                if (targetObject == null)
                    throw new ArgumentNullException("targetObject");
                if (action == null)
                    throw new ArgumentNullException("action");

                control = targetObject;
                Action = action;
                IsSync = isSync;
            }

            public bool IsSync { get; private set; }

            public bool Disposing
            {
                get { return control.Disposing; }
            }

            public bool IsDisposed
            {
                get { return control.IsDisposed; }
            }

            public Action Action { get; private set; }

            public Exception Exception { get; set; }
        }

        #endregion Private Class

        #region Private Fields

        private static Control _context;

        #endregion Private Fields
    }
}