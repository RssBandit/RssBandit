
using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public static partial class TaskFactoryExtensions
    {
        #region TaskFactory with IEnumerable<Task> asyncIterator
        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, null, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, null, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator,
            TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return Iterate(factory, asyncIterator, null, creationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator,
            object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, state, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator,
            object state, TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, state, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> asyncIterator,
            object state, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            // Validate parameters
            if (factory == null) throw new ArgumentNullException("factory");
            if (asyncIterator == null) throw new ArgumentNullException("asyncIterator");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Get an enumerator from the enumerable
            var enumerator = asyncIterator.GetEnumerator();
            if (enumerator == null) throw new InvalidOperationException();

            // Create the task to be returned to the caller.  And ensure
            // that when everything is done, the enumerator is cleaned up.
            var trs = new TaskCompletionSource<object>(state, creationOptions);
            trs.Task.ContinueWith(_ => enumerator.Dispose(), factory.CancellationToken, TaskContinuationOptions.None, scheduler);

            // This will be called every time more work can be done.
            Action<Task> recursiveBody = null;
            recursiveBody = antecedent =>
            {
                try
                {
                    // If the previous task completed with any exceptions, bail
                    if (antecedent != null && antecedent.IsFaulted)
                        trs.TrySetException(antecedent.Exception.InnerExceptions);

                    // Else if the user requested cancellation, bail.
                    else if (factory.CancellationToken.IsCancellationRequested ||
                        (antecedent != null && antecedent.IsCanceled)) trs.TrySetCanceled();

                    // Else if we should continue iterating and there's more to iterate
                    // over, create a continuation to continue processing.  We only
                    // want to continue processing once the current Task (as yielded
                    // from the enumerator) is complete.
                    else if (enumerator.MoveNext())
                        enumerator.Current.ContinueWith(recursiveBody,
                            factory.CancellationToken, TaskContinuationOptions.None, scheduler).IgnoreExceptions();

                    // Otherwise, we're done!
                    else trs.TrySetResult(null);
                }
                // If MoveNext throws an exception, propagate that to the user
                catch (Exception exc) { trs.TrySetException(exc); }
            };

            // Get things started by launching the first task
            factory.StartNew(() => recursiveBody(null), CancellationToken.None, TaskCreationOptions.None, scheduler).IgnoreExceptions();

            // Return the representative task to the user
            return trs.Task;
        }
        #endregion

        #region TaskFactory<TResult> with Func<Action<TResult>, IEnumerable<Task>> asyncIterator
        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, null, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, null, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            return Iterate(factory, asyncIterator, null, creationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator,
            object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, state, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator,
            object state,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, asyncIterator, state, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="asyncIterator">
        /// A function that generates an enumerable containing the tasks to be iterated through.
        /// The function is provided with an action that can be used to provide the final return value.
        /// </param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task<TResult> Iterate<TResult>(
            this TaskFactory<TResult> factory,
            Func<Action<TResult>, IEnumerable<Task>> asyncIterator,
            object state,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            // Validate parameters
            if (factory == null) throw new ArgumentNullException("factory");
            if (asyncIterator == null) throw new ArgumentNullException("asyncIterator");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Create the task to be returned to the caller.  And ensure
            // that when everything is done, the enumerator is cleaned up.
            var trs = new TaskCompletionSource<TResult>(state, creationOptions);
            Action<TResult> completionAction = result => trs.SetResult(result);

            // Get an enumerator from the enumerable
            var enumerable = asyncIterator(completionAction);
            if (enumerable == null) throw new InvalidOperationException();
            var enumerator = enumerable.GetEnumerator();
            if (enumerator == null) throw new InvalidOperationException();
            trs.Task.ContinueWith(_ => enumerator.Dispose(), CancellationToken.None, TaskContinuationOptions.None, scheduler);

            // This will be called every time more work can be done.
            Action<Task> recursiveBody = null;
            recursiveBody = antecedent =>
            {
                try
                {
                    // If the previous task completed with any exceptions, bail
                    if (antecedent != null && antecedent.IsFaulted)
                        trs.TrySetException(antecedent.Exception.InnerExceptions);

                    // Else if the user requested cancellation, bail.
                    else if (factory.CancellationToken.IsCancellationRequested ||
                        (antecedent != null && antecedent.IsCanceled)) trs.TrySetCanceled();

                    // Else if we should continue iterating and there's more to iterate
                    // over, create a continuation to continue processing.  We only
                    // want to continue processing once the current Task (as yielded
                    // from the enumerator) is complete.
                    else if (enumerator.MoveNext())
                        enumerator.Current.ContinueWith(recursiveBody,
                            factory.CancellationToken, TaskContinuationOptions.None, scheduler).IgnoreExceptions();

                    // Otherwise, we're done!
                    else trs.TrySetResult(default(TResult));
                }
                // If MoveNext throws an exception, propagate that to the user
                catch (Exception exc) { trs.TrySetException(exc); }
            };

            // Get things started by launching the first task
            factory.StartNew(delegate
            {
                recursiveBody(null);
                return default(TResult); // since we only have a TaskFactory<TResult>
            }, CancellationToken.None, TaskCreationOptions.None, scheduler).IgnoreExceptions();

            // Return the representative task to the user
            return trs.Task;
        }
        #endregion
    }
}