using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace InfiniteRefactor.Infrastructure.Extensions
{
    public static class AutoRetry
    {
        public static T InvokeWithAutoRetry<T>(this Func<int, T> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null)
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    var ret = action(i);
                    return ret;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (interval > 0)
                    {
                        Thread.Sleep(interval * (intervalIncrease ? (i + 1) : 1));
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static T InvokeWithAutoRetry<T>(this Func<T> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null)
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    var ret = action();
                    return ret;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (interval > 0)
                    {
                        Thread.Sleep(interval * (intervalIncrease ? (i + 1) : 1));
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static void InvokeWithAutoRetry(this Action action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null)
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (interval > 0)
                    {
                        Thread.Sleep(interval * (intervalIncrease ? (i + 1) : 1));
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static void InvokeWithAutoRetry(this Action<int> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null)
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    action(i);
                    return;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (interval > 0)
                    {
                        Thread.Sleep(interval * (intervalIncrease ? (i + 1) : 1));
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }


        public static async Task<T> InvokeWithAutoRetryAsync<T>(this Func<int, CancellationToken, Task<T>> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null, CancellationToken token = default(CancellationToken))
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    var ret = await action(i, token);
                    return ret;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (!token.IsCancellationRequested)
                    {
                        if (interval > 0)
                        {
                            await Task.Delay(interval * (intervalIncrease ? (i + 1) : 1));
                        }
                    }
                    else
                    {
                        throw new TaskCanceledException();
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static async Task<T> InvokeWithAutoRetryAsync<T>(this Func<CancellationToken, Task<T>> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null, CancellationToken token = default(CancellationToken))
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    var ret = await action(token);
                    return ret;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (!token.IsCancellationRequested)
                    {
                        if (interval > 0)
                        {
                            await Task.Delay(interval * (intervalIncrease ? (i + 1) : 1));
                        }
                    }
                    else
                    {
                        throw new TaskCanceledException();
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static async Task InvokeWithAutoRetryAsync(this Func<CancellationToken, Task> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null, CancellationToken token = default(CancellationToken))
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    await action(token);
                    return;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (!token.IsCancellationRequested)
                    {
                        if (interval > 0)
                        {
                            await Task.Delay(interval * (intervalIncrease ? (i + 1) : 1));
                        }
                    }
                    else
                    {
                        throw new TaskCanceledException();
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

        public static async Task InvokeWithAutoRetryAsync(this Func<int, CancellationToken, Task> action, int times = 3, int interval = 0, bool intervalIncrease = false, string actionDescription = null, Logger log = null, CancellationToken token = default(CancellationToken))
        {
            var exs = new List<Exception>();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    await action(i, token);
                    return;
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                    if (log != null) { log.Error(ex, ex.Message); }

                    if (!token.IsCancellationRequested)
                    {
                        if (interval > 0)
                        {
                            await Task.Delay(interval * (intervalIncrease ? (i + 1) : 1));
                        }
                    }
                    else
                    {
                        throw new TaskCanceledException();
                    }
                }
            }
            throw new AggregateException(string.Format("{0}操作在{1}次重试后失败。", actionDescription ?? string.Empty, times), exs);
        }

    }
}
