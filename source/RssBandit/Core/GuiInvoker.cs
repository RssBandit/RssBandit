using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using NewsComponents.Utils;

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
    internal static class GuiInvoker
    {
        #region Public Methods

        /// <summary>
        /// This method MUST be called from your main UI thread prior to the invoke methods being called.
        /// </summary>
        public static void Initialize()
        {
            if (_context == null)
                _context = AsyncOperationManager.SynchronizationContext;
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

        private static void DoInvoke(Control control, Action action, SynchronizationContext context, bool synchronous)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (context == null)
                throw new ArgumentNullException("context");

            try
            {
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
                        context.Send(SafeInvoke, invokeControl);
                    else
                        context.Post(SafeInvoke, invokeControl);
                }
                else
                {
                    if (synchronous)
                        context.Send(ActionInvoke, action);
                    else
                        context.Post(ActionInvoke, action);
                }
            }
            catch (InvalidAsynchronousStateException)
            {
                // This can happen on shutdown
            }
            catch (NullReferenceException)
            {
                // can happen on shutdown
            }
            catch(InvalidOperationException)
            {
                // can happen on shutdown
            }
            catch (TargetInvocationException e)
            {

                if (e.InnerException != null)
                {
                    ExceptionHelper.PreserveExceptionStackTrace(e.InnerException);

                    // Throw the new exception
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
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

        private static SynchronizationContext _context;

        #endregion Private Fields
    }

}
