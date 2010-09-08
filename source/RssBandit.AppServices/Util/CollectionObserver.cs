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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

namespace RssBandit.AppServices.Util
{
  public static class CollectionObserver
  {
    public static CollectionObserver<T> Create<T>(ICollection<T> collection) where T : INotifyPropertyChanged
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      return new CollectionObserver<T>(collection);
    }
  }
  //TODO: replace with CLINQ or RX
  public sealed class CollectionObserver<TPropertySource> : IWeakEventListener where TPropertySource : INotifyPropertyChanged
  {
    private readonly WeakReference _collectionRef;
    private readonly Dictionary<string,Tuple<Expression<Func<TPropertySource, object>>,Action<TPropertySource>>> _propertyNameToExpressHandlerMap;

    private readonly List<PropertyObserver<TPropertySource>> _collectionIndexToObserverMap;

    public CollectionObserver(ICollection<TPropertySource> collection) 
    {
      var notifyCollection = collection as INotifyCollectionChanged;
      if(notifyCollection == null)
        throw new InvalidOperationException("collection must implement INotifyCollectionChanged");

      _collectionRef = new WeakReference(collection);
      _collectionIndexToObserverMap = new List<PropertyObserver<TPropertySource>>();
      _propertyNameToExpressHandlerMap =new Dictionary<string, Tuple<Expression<Func<TPropertySource, object>>, Action<TPropertySource>>>();

                
    }

    public CollectionObserver<TPropertySource> RegisterHandler(
        Expression<Func<TPropertySource, object>> expression,
        Action<TPropertySource> handler)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      string propertyName = PropertyObserver<TPropertySource>.GetPropertyName(expression);
      if (String.IsNullOrEmpty(propertyName))
        throw new ArgumentException("'expression' did not provide a property name.");
      
      INotifyCollectionChanged collection = GetCollection();
      if (collection != null)
      {
        if (_propertyNameToExpressHandlerMap.Count == 0)
        {
          //first time
          CollectionChangedEventManager.AddListener(collection, this);
          AddAll();
        }

        _propertyNameToExpressHandlerMap.Add(propertyName, Tuple.Create(expression,handler));
        _collectionIndexToObserverMap.Run(observer => observer.RegisterHandler(expression, handler)); 
      }

      return this;
    }

    public CollectionObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      string propertyName = PropertyObserver<TPropertySource>.GetPropertyName(expression);
      if (String.IsNullOrEmpty(propertyName))
        throw new ArgumentException("'expression' did not provide a property name.");
      
      INotifyCollectionChanged collection = GetCollection();
      if (collection != null)
      {
        if(_propertyNameToExpressHandlerMap.ContainsKey(propertyName))
        {

          _collectionIndexToObserverMap.Run(observer => observer.UnregisterHandler(expression));
          _propertyNameToExpressHandlerMap.Remove(propertyName);
        }

        if (_propertyNameToExpressHandlerMap.Count == 0)
        {
          //last one
          CollectionChangedEventManager.RemoveListener(collection, this);
        }

      }

      return this;
    }

    public void UnregisterAllHandlers()
    {
      INotifyCollectionChanged collection = GetCollection();
      if (collection != null)
      {
        var expressions = _propertyNameToExpressHandlerMap.Values.Select(tuple => tuple.Item1).ToArray();

        expressions.Run(expression => UnregisterHandler(expression));
      }
    }


    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
      bool handled = false;

      if (managerType == typeof(CollectionChangedEventManager))
      {
        handled = true;

        var args = e as NotifyCollectionChangedEventArgs;
        
        if (args != null && sender is INotifyCollectionChanged)
        {
          switch (args.Action)
          {
            case NotifyCollectionChangedAction.Add:
              
              for (int i = 0; i < args.NewItems.Count; i++)
              {
                var propertyObserver = new PropertyObserver<TPropertySource>((TPropertySource) args.NewItems[i]);
                RegisterAllHandlers(propertyObserver);
                _collectionIndexToObserverMap.Insert(args.NewStartingIndex+i,propertyObserver);
              }
              break;
            case NotifyCollectionChangedAction.Remove:
              for (int i = 0; i < args.OldItems.Count; i++)
              {
                var propertyObserver = _collectionIndexToObserverMap[args.OldStartingIndex];

                UnregisterAllHandlers(propertyObserver);

                _collectionIndexToObserverMap.RemoveAt(args.OldStartingIndex);
              }
              break;
            case NotifyCollectionChangedAction.Replace:
              for (int i = 0; i < args.OldItems.Count; i++)
              {
                var oldPropertyObserver = _collectionIndexToObserverMap[args.OldStartingIndex + i];
                UnregisterAllHandlers(oldPropertyObserver);

                var propertyObserver = new PropertyObserver<TPropertySource>((TPropertySource)args.NewItems[i]);
                RegisterAllHandlers(propertyObserver);

                _collectionIndexToObserverMap[args.OldStartingIndex + i] = propertyObserver;
              }
              break;
            case NotifyCollectionChangedAction.Move:
              for (int i = 0; i < args.NewItems.Count; i++)
              {
                var propertyObserver = _collectionIndexToObserverMap[args.OldStartingIndex + i];
                _collectionIndexToObserverMap.RemoveAt(args.OldStartingIndex);
                _collectionIndexToObserverMap.Insert(args.NewStartingIndex+i,propertyObserver);
              }
              break;
            case NotifyCollectionChangedAction.Reset:
              _collectionIndexToObserverMap.Run(UnregisterAllHandlers);
              _collectionIndexToObserverMap.Clear();
              AddAll();
              _collectionIndexToObserverMap.Run(RegisterAllHandlers);
              break;
          }
        }
      }

      return handled;
    }

    private void UnregisterAllHandlers(PropertyObserver<TPropertySource> propertyObserver)
    {
      _propertyNameToExpressHandlerMap.Values.Run(tuple => propertyObserver.UnregisterHandler(tuple.Item1));
    }

    private void RegisterAllHandlers(PropertyObserver<TPropertySource> propertyObserver)
    {
      _propertyNameToExpressHandlerMap.Values.Run(tuple => propertyObserver.RegisterHandler(tuple.Item1,tuple.Item2));
    }

    private void AddAll()
    {
      var collection = GetCollection() as ICollection<TPropertySource>;

      if(collection!=null)
      {
        var observers = collection.Select(source =>
                                            {
                                              var propertyObserver = new PropertyObserver<TPropertySource>(source);
                                              RegisterAllHandlers(propertyObserver);
                                              return propertyObserver;
                                            });

        _collectionIndexToObserverMap.AddRange(observers);
      }
    }

    INotifyCollectionChanged GetCollection()
    {
      try
      {
        return (INotifyCollectionChanged)_collectionRef.Target;
      }
      catch
      {
        return null;
      }
    }
  }
}
