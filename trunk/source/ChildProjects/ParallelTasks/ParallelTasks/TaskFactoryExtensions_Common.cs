﻿
namespace System.Threading.Tasks
{
    /// <summary>Extensions for TaskFactory.</summary>
    public static partial class TaskFactoryExtensions
    {
        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory<TResult> ToGeneric<TResult>(this TaskFactory factory)
        {
            return new TaskFactory<TResult>(
                factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory ToNonGeneric<TResult>(this TaskFactory<TResult> factory)
        {
            return new TaskFactory(
                factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.Scheduler != null ? factory.Scheduler : TaskScheduler.Current;
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler<TResult>(this TaskFactory<TResult> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.Scheduler != null ? factory.Scheduler : TaskScheduler.Current;
        }

        /// <summary>Converts TaskCreationOptions into TaskContinuationOptions.</summary>
        /// <param name="creationOptions"></param>
        /// <returns></returns>
        private static TaskContinuationOptions ContinuationOptionsFromCreationOptions(TaskCreationOptions creationOptions)
        {
            return (TaskContinuationOptions)
                ((creationOptions & TaskCreationOptions.AttachedToParent) |
                 (creationOptions & TaskCreationOptions.PreferFairness) |
                 (creationOptions & TaskCreationOptions.LongRunning));
        }
    }
}
