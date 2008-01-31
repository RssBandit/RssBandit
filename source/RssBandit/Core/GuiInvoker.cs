using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace System
{
    // TODO: Remove when switching to .NET 3.5
    public delegate void Action();
}

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
            InvokeControl d = (InvokeControl)state;

            // Check these from the UI thread to prevent a race condition
            if (d.Disposing || d.IsDisposed)
            {
                return;
            }

            d.Action();
        }

        private static void ActionInvoke(object state)
        {
            Action a = (Action)state;
            a();
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

            //try
            //{
            if (control != null)
            {
                InvokeControl invokeControl = new InvokeControl(control, action);

                if (synchronous)
                {
                    // if we're already on the right thread, just execute
                    if (control.IsHandleCreated && !control.InvokeRequired)
                    {
                        SafeInvoke(invokeControl);
                        return;
                    }
                }

                if (synchronous)
                    context.Invoke((WaitCallback)SafeInvoke, invokeControl);
                else
                    context.BeginInvoke((WaitCallback)SafeInvoke, invokeControl);
            }
            else
            {
                if (synchronous)
                    context.Invoke((WaitCallback)ActionInvoke, action);
                else
                    context.BeginInvoke((WaitCallback)ActionInvoke, action);
            }
            //}
            //catch (InvalidAsynchronousStateException)
            //{
            //    // This can happen on shutdown
            //}
            //catch(InvalidOperationException)
            //{
            //    // This can happen on shutdown
            //}
            //catch (NullReferenceException)
            //{
            //    // can happen on shutdown
            //}
            //catch (TargetInvocationException e)
            //{

            //    if (e.InnerException != null)
            //    {
            //        ExceptionHelper.PreserveExceptionStackTrace(e.InnerException);

            //        // Throw the new exception
            //        throw e.InnerException;
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
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
             * There are to side-effects of this behavior:
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

            Type context = Type.GetType("System.Windows.Forms.Application+ThreadContext, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            MethodInfo current = context.GetMethod("FromCurrent", BindingFlags.NonPublic | BindingFlags.Static);

            PropertyInfo prop = context.GetProperty("MarshalingControl", BindingFlags.Instance | BindingFlags.NonPublic);

            object thread = current.Invoke(null, null);

            Control control = (Control)prop.GetValue(thread, null);

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
            private readonly Action action;

            public InvokeControl(Control targetObject, Action action)
            {
                if (targetObject == null)
                    throw new ArgumentNullException("targetObject");
                if (action == null)
                    throw new ArgumentNullException("action");

                control = targetObject;
                this.action = action;
            }

            public bool Disposing
            {
                get
                {
                    return control.Disposing;
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return control.IsDisposed;
                }
            }

            public Action Action
            {
                get { return action; }
            }

        }


        #endregion Private Class

        #region Private Fields

        private static Control _context;

        #endregion Private Fields
    }

}
