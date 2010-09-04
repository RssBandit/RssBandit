using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MorganStanley.EquityPPP.PharosGUI.Presentation.Util
{
  // The code in this file is from Dustin Campbell's blog at
  // http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="E"></typeparam>
  /// <param name="eventHandler"></param>
  public delegate void UnregisterEventHandler<E>(EventHandler<E> eventHandler) where E : EventArgs;

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="E"></typeparam>
  internal interface IWeakEventHandler<E> where E : EventArgs
  {
    EventHandler<E> Handler { get; }
    WeakReference Target { get; }
  }

  /// <summary>
  /// Weak Event Handler, taken from: 
  /// http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="E"></typeparam>
  internal class WeakEventHandler<T, E> : IWeakEventHandler<E>
    where T : class
    where E : EventArgs
  {


    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <param name="unregister"></param>
    public WeakEventHandler(EventHandler<E> eventHandler, UnregisterEventHandler<E> unregister)
    {
      m_TargetRef = new WeakReference(eventHandler.Target);
      m_OpenHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler),
        null, eventHandler.Method);
      m_Handler = Invoke;
      m_Unregister = unregister;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Invoke(object sender, E e)
    {
      T target = (T)m_TargetRef.Target;

      if (!ReferenceEquals(target, null))
        m_OpenHandler.Invoke(target, sender, e);
      else if (m_Unregister != null)
      {
        m_Unregister(m_Handler);
        m_Unregister = null;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public EventHandler<E> Handler
    {
      get { return m_Handler; }
    }

    /// <summary>
    /// 
    /// </summary>
    public WeakReference Target
    {
      get { return m_TargetRef; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weh"></param>
    /// <returns></returns>
    public static implicit operator EventHandler<E>(WeakEventHandler<T, E> weh)
    {
      return weh.m_Handler;
    }

    private delegate void OpenEventHandler(T @this, object sender, E e);
    private WeakReference m_TargetRef;
    private OpenEventHandler m_OpenHandler;
    private EventHandler<E> m_Handler;
    private UnregisterEventHandler<E> m_Unregister;

  }



  /// <summary>
  /// 
  /// </summary>
  public static class EventHandlerUtils
  {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="eventHandler"></param>
    /// <param name="unregister"></param>
    /// <returns></returns>
    public static EventHandler<E> MakeWeak<E>(this EventHandler<E> eventHandler, UnregisterEventHandler<E> unregister) where E : EventArgs
    {
      if (eventHandler == null)
        throw new ArgumentNullException("eventHandler");
      if (eventHandler.Method.IsStatic || eventHandler.Target == null)
        throw new ArgumentException("Only instance methods are supported.", "eventHandler");

      // check to see if we're already weak
      if (eventHandler.Method.DeclaringType.IsGenericType && eventHandler.Method.DeclaringType.GetGenericTypeDefinition() == typeof(WeakEventHandler<,>))
      {
        return eventHandler;
      }

      Type wehType = typeof(WeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(E));

      ConstructorInfo wehConstructor = wehType.GetConstructor(new Type[] { typeof(EventHandler<E>), 
                typeof(UnregisterEventHandler<E>) });

      IWeakEventHandler<E> weh = (IWeakEventHandler<E>)wehConstructor.Invoke(
        new object[] { eventHandler, unregister });

      return weh.Handler;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="sourceHandler"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EventHandler<E> Unregister<E>(this EventHandler<E> sourceHandler, EventHandler<E> value) where E : EventArgs
    {
      if (value == null)
        throw new ArgumentNullException("value");
      if (value.Method.IsStatic || value.Target == null)
        throw new ArgumentException("Only instance methods are supported.", "value");

      if (sourceHandler != null)
      {
        // look for the weak event handler in the invocation list
        foreach (EventHandler<E> evt in sourceHandler.GetInvocationList())
        {
          IWeakEventHandler<E> weh = evt.Target as IWeakEventHandler<E>;
          if (weh != null)
          {
            object target = weh.Target.Target;
            if (!ReferenceEquals(target, null) && ReferenceEquals(target, value.Target))
            {
              return weh.Handler;
            }
          }
        }
      }

      // return the input as the default if we don't find a wrapped event handler
      return value;
    }
  }
}
