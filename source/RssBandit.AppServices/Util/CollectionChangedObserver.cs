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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Windows;

namespace RssBandit.AppServices.Util
{
  public static class NotifyCollectionChangedExtensions
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="handler"></param>
      /// <param name="observer">Need to store this strong reference to control lifetime -- usually just store as field in owning class</param>
    public static void ListenToCollectionChanged(this INotifyCollectionChanged collection, Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> handler, out CollectionChangedObserver observer)
    {
      Contract.Requires(collection != null);
      Contract.Requires(handler != null);

      observer = new CollectionChangedObserver(collection, handler);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="sourceCollection"></param>
    /// <param name="targetCollection"></param>
    /// <param name="creator"></param>
    /// <param name="observer">Need to store this strong reference to control lifetime -- usually just store as field in owning class</param>
    /// <param name="continueWith"></param>
    public static void SynchronizeCollection<TSource, TTarget>(this IEnumerable<TSource> sourceCollection, ObservableCollection<TTarget> targetCollection, Func<TSource, TTarget> creator, out CollectionChangedObserver observer, Action continueWith = null)
    {
      Contract.Requires(sourceCollection != null);
      Contract.Requires(targetCollection != null);
      Contract.Requires(creator != null);
      Contract.Requires(sourceCollection is INotifyCollectionChanged);


      ((INotifyCollectionChanged)sourceCollection).ListenToCollectionChanged(
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
        }, out observer);

      PopulateTarget(sourceCollection, targetCollection, creator);

      if (continueWith != null)
        continueWith();
        
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
