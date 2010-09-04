using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows;

namespace RssBandit.Util
{
  public static class NotifyCollectionChangedExtensions
  {
    public static CollectionChangedObserver ListenToCollectionChanged(this INotifyCollectionChanged collection, Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> handler)
    {
      Contract.Requires(collection != null);
      Contract.Requires(handler != null);

      return new CollectionChangedObserver(collection, handler);
    }

    public static CollectionChangedObserver SynchronizeCollection<TSource, TTarget>(this IEnumerable<TSource> sourceCollection, ObservableCollection<TTarget> targetCollection, Func<TSource, TTarget> creator, Action continueWith = null)
    {
      Contract.Requires(sourceCollection != null);
      Contract.Requires(targetCollection != null);
      Contract.Requires(creator != null);
      Contract.Requires(sourceCollection is INotifyCollectionChanged);

      var ret = ((INotifyCollectionChanged)sourceCollection).ListenToCollectionChanged(
        (s, e) =>
        {
          switch (e.Action)
          {
            case NotifyCollectionChangedAction.Add:
                targetCollection.Add(creator((TSource)e.NewItems[0]));
              break;
            case NotifyCollectionChangedAction.Remove:
                targetCollection.RemoveAt(e.OldStartingIndex); 
              break;
            case NotifyCollectionChangedAction.Replace:
              targetCollection[e.OldStartingIndex] = creator((TSource)e.NewItems[0]);
              break;
            case NotifyCollectionChangedAction.Move:
                targetCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
              break;
            case NotifyCollectionChangedAction.Reset:
              targetCollection.Clear();
              PopulateTarget(sourceCollection, targetCollection, creator);
              break;
            default:
              throw new ArgumentOutOfRangeException("e");
          }

          if (continueWith != null)
            continueWith();
        });

      PopulateTarget(sourceCollection, targetCollection, creator);

      if (continueWith != null)
        continueWith();

      return ret;
    }

    private static void PopulateTarget<TSource, TTarget>(IEnumerable<TSource> source, ICollection<TTarget> target, Func<TSource, TTarget> creator)
    {
      foreach (var sa in source)
        target.Add(creator(sa));
    }

  }

  public sealed class CollectionChangedObserver : IWeakEventListener, IDisposable
  {
    private Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> _handler;

    public CollectionChangedObserver(INotifyCollectionChanged collection, Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> handler)
    {
      Contract.Requires(collection != null);
      Contract.Requires(handler != null);
      
      _handler = handler;

      CollectionChangedEventManager.AddListener(collection, this);
    }

    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
      bool handled = false;

      if (managerType == typeof (CollectionChangedEventManager))
      {
        handled = true;

        var args = e as NotifyCollectionChangedEventArgs;

        if (_handler != null && args != null && sender is INotifyCollectionChanged)
        {
          _handler((INotifyCollectionChanged)sender, args);
        }

      }

      return handled;
    }


    public void Dispose()
    {
      _handler = null;
    }
  }
}
