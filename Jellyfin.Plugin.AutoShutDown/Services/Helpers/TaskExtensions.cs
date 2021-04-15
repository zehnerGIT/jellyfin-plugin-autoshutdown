using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers
{
    public static class TaskExtensions
    {
        // both implementations of .WhenFirst from
        // https://stackoverflow.com/questions/38289158/how-to-implement-task-whenany-with-a-predicate

        public static Task<Task<T>> WhenFirst<T>(IEnumerable<Task<T>> tasks, Func<Task<T>, bool> predicate)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var tasksArray = (tasks as IReadOnlyList<Task<T>>) ?? tasks.ToArray();
            if (tasksArray.Count == 0)
            {
                throw new ArgumentException("Empty task list", nameof(tasks));
            }

            if (tasksArray.Any(t => t == null))
            {
                throw new ArgumentException("Tasks contains a null reference", nameof(tasks));
            }

            var tcs = new TaskCompletionSource<Task<T>>();
            var count = tasksArray.Count;

            Action<Task<T>> continuation = t =>
            {
                if (predicate(t))
                {
                    tcs.TrySetResult(t);
                }

                if (Interlocked.Decrement(ref count) == 0)
                {
                    tcs.TrySetResult(null);
                }
            };

            foreach (var task in tasksArray)
            {
                task.ContinueWith(continuation);
            }

            return tcs.Task;
        }

        /*
        public static Task<Task<T>> WhenFirst<T>(IEnumerable<Task<T>> tasks,
    Func<Task<T>, bool> predicate)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var tcs = new TaskCompletionSource<Task<T>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var pendingCount = 1; // The initial 1 represents the enumeration itself
            foreach (var task in tasks)
            {
                if (task == null) throw new ArgumentException($"The {nameof(tasks)}" +
                    " argument included a null value.", nameof(tasks));
                Interlocked.Increment(ref pendingCount);
                HandleTaskCompletion(task);
            }
            if (Interlocked.Decrement(ref pendingCount) == 0) tcs.TrySetResult(null);
            return tcs.Task;


            async void HandleTaskCompletion(Task<T> task)
            {
                try
                {
                    await task; // Continue on the captured context
                }
                catch { } // Ignore exception

                if (tcs.Task.IsCompleted) return;

                try
                {
                    if (predicate(task))
                        tcs.TrySetResult(task);
                    else
                        if (Interlocked.Decrement(ref pendingCount) == 0)
                        tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
        }
        */
    }
}
