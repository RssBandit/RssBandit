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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

// Code from MVVM Foundation on CodePlex: http://mvvmfoundation.codeplex.com/
// License is MS-Pl

namespace RssBandit.AppServices.Util
{
  public static class PropertyObserver
  {
    public static PropertyObserver<T> Create<T>(T target) where T : INotifyPropertyChanged
    {
      if (target == null)
        throw new ArgumentNullException("target");

      return new PropertyObserver<T>(target);
    }
  }

  /// <summary>
  /// Monitors the PropertyChanged event of an object that implements INotifyPropertyChanged,
  /// and executes callback methods (i.e. handlers) registered for properties of that object.
  /// </summary>
  /// <typeparam name="TPropertySource">The type of object to monitor for property changes.</typeparam>
  public sealed class PropertyObserver<TPropertySource> : IWeakEventListener
      where TPropertySource : INotifyPropertyChanged
  {
    readonly Dictionary<string, Action<TPropertySource>> _propertyNameToHandlerMap;
    readonly WeakReference _propertySourceRef;

    /// <summary>
    /// Initializes a new instance of PropertyObserver, which
    /// observes the 'propertySource' object for property changes.
    /// </summary>
    /// <param name="propertySource">The object to monitor for property changes.</param>
    public PropertyObserver(TPropertySource propertySource)
    {
      if (propertySource == null)
        throw new ArgumentNullException("propertySource");

      _propertySourceRef = new WeakReference(propertySource);
      _propertyNameToHandlerMap = new Dictionary<string, Action<TPropertySource>>();
    }

    /// <summary>
    /// Registers a callback to be invoked when the PropertyChanged event has been raised for the specified property.
    /// </summary>
    /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
    /// <param name="handler">The callback to invoke when the property has changed.</param>
    /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
    public PropertyObserver<TPropertySource> RegisterHandler(
        Expression<Func<TPropertySource, object>> expression,
        Action<TPropertySource> handler)
    {
      string propertyName = GetPropertyName(expression);

      return RegisterHandler(propertyName, handler);
    }

    public PropertyObserver<TPropertySource> RegisterHandler(
        string propertyName,
        Action<TPropertySource> handler)
    {
      if (handler == null)
        throw new ArgumentNullException("handler");

      propertyName = propertyName ?? string.Empty;

      TPropertySource propertySource = GetPropertySource();
      if (propertySource != null)
      {
        Debug.Assert(!_propertyNameToHandlerMap.ContainsKey(propertyName), "Why is the '" + propertyName + "' property being registered again?");

        _propertyNameToHandlerMap[propertyName] = handler;
        PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
      }

      return this;
    }

    /// <summary>
    /// Removes the callback associated with the specified property.
    /// </summary>
    /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
    /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
    public PropertyObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
    {
      string propertyName = GetPropertyName(expression);

      return UnregisterHandler(propertyName);
    }

    public PropertyObserver<TPropertySource> UnregisterHandler(string propertyName)
    {
      propertyName = propertyName ?? string.Empty;
      TPropertySource propertySource = GetPropertySource();
      if (propertySource != null)
      {
        if (_propertyNameToHandlerMap.ContainsKey(propertyName))
        {
          _propertyNameToHandlerMap.Remove(propertyName);
          PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);
        }
      }

      return this;
    }

    public void UnregisterAllHandlers()
    {
      TPropertySource propertySource = GetPropertySource();
      if (propertySource != null)
      {
        var properties = _propertyNameToHandlerMap.Keys.ToArray();

        properties.Run(s =>
                         {
                           _propertyNameToHandlerMap.Remove(s);
                           PropertyChangedEventManager.RemoveListener(propertySource,this,s);
                         }
          
          );
      }
    }

    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
      bool handled = false;

      if (managerType == typeof(PropertyChangedEventManager))
      {
        handled = true;

        var args = e as PropertyChangedEventArgs;
        if (args != null && sender is TPropertySource)
        {
          string propertyName = args.PropertyName;
          var propertySource = (TPropertySource)sender;

          if (String.IsNullOrEmpty(propertyName))
          {
            // When the property name is empty, all properties are considered to be invalidated.
            // Iterate over a copy of the list of handlers, in case a handler is registered by a callback.
            foreach (Action<TPropertySource> handler in _propertyNameToHandlerMap.Values.ToArray())
              handler(propertySource);
            
          }
          else
          {
            Action<TPropertySource> handler;
            if (_propertyNameToHandlerMap.TryGetValue(propertyName, out handler))
            {
              handler(propertySource);
            }
          }
        }
      }

      return handled;
    }

    internal static string GetPropertyName(Expression<Func<TPropertySource, object>> expression)
    {
      if (expression == null)
        return string.Empty;

      var lambda = expression as LambdaExpression;
      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = lambda.Body as UnaryExpression;
        memberExpression = unaryExpression.Operand as MemberExpression;
      }
      else
      {
        memberExpression = lambda.Body as MemberExpression;
      }

      Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

      if (memberExpression != null)
      {
        var propertyInfo = memberExpression.Member as PropertyInfo;

        return propertyInfo.Name;
      }

      return null;
    }

    TPropertySource GetPropertySource()
    {
      try
      {
        return (TPropertySource)_propertySourceRef.Target;
      }
      catch
      {
        return default(TPropertySource);
      }
    }
  }

}
