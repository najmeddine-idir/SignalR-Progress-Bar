﻿using System.Collections.Concurrent;

namespace SignalRProgressionBackendTest;

public static class EnumerableExtension
{
    public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction, int maxDegreeOfParallelism = 10)
    {
        if (source != null && source.Any())
        {
            var semaphoreSlim = new SemaphoreSlim(maxDegreeOfParallelism);
            var tcs = new TaskCompletionSource<object>();
            var exceptions = new ConcurrentBag<Exception>();
            bool addingCompleted = false;

            foreach (T item in source)
            {
                await semaphoreSlim.WaitAsync();
                asyncAction(item).ContinueWith(t =>
                {
                    semaphoreSlim.Release();

                    if (t.Exception != null)
                    {
                        exceptions.Add(t.Exception);
                    }

                    if (Volatile.Read(ref addingCompleted) && semaphoreSlim.CurrentCount == maxDegreeOfParallelism)
                    {
                        tcs.TrySetResult(null);
                    }
                });
            }

            Volatile.Write(ref addingCompleted, true);
            await tcs.Task;
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
