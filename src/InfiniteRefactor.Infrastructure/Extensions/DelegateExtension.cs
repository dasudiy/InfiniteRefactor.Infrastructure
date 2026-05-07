using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Extensions
{
    public static class Try
    {
        public static Func<Exception, Task> TodoExceptionHook { get; set; }

        #region syntax async
        public static void Todo(this Action fn, Action<Exception> fail = null, Action finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                fn();
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                if (fail != null) { fail(ex); }
            }
            finally
            {
                if (finallyAction != null) { finallyAction(); }
            }
        }

        public static void TodoTimes(this Action fn, Action<Exception> fail = null, Action finallyAction = null, int times = 1)
        {
            if (fn == null) { return; }
            for (var i = 0; i < times; i++)
            {
                try
                {
                    fn();
                    break;
                }
                catch (Exception ex)
                {
                    if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                    if (fail != null) { fail(ex); }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }
        }

        public static async Task TodoAsync(this Func<Task> fn, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            try
            {
                await fn();
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null) { await fail(ex); }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task TodoTimesAsync(this Func<Task> fn, Func<Exception, Task> fail = null, Func<Task> finallyAction = null, int times = 1)
        {
            if (fn == null) { return; }
            for (var i = 0; i < times; i++)
            {
                try
                {
                    await fn();
                    break;
                }
                catch (Exception ex)
                {
                    if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                    if (fail != null) { await fail(ex); }
                }
                finally
                {
                    if (finallyAction != null) { await finallyAction(); }
                }
            }
        }

        public static T Todo<T>(this Func<T> fn, Action<Exception> fail = null, Action finallyAction = null)
        {
            var t = default(T);
            if (fn == null) { return t; }
            try
            {
                t = fn();
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                if (fail != null) { fail(ex); }
            }
            finally
            {
                if (finallyAction != null) { finallyAction(); }
            }
            return t;
        }

        public static T TodoTimes<T>(this Func<T> fn, Action<Exception> fail = null, Action finallyAction = null, int times = 1)
        {
            var t = default(T);
            if (fn == null) { return t; }
            for (var i = 0; i < times; i++)
            {
                try
                {
                    t = fn();
                    break;
                }
                catch (Exception ex)
                {
                    if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                    if (fail != null) { fail(ex); }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }
            return t;
        }

        public static async Task<T> TodoAsync<T>(this Func<Task<T>> fn, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            var t = default(T);
            try
            {
                t = await fn();
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null) { await fail(ex); }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
            return t;
        }

        public static async Task<T> TodoAsyncTimes<T>(this Func<Task<T>> fn, Func<Exception, Task> fail = null, Func<Task> finallyAction = null, int times = 1)
        {
            var t = default(T);
            for (var i = 0; i < times; i++)
            {
                try
                {
                    t = await fn();
                    break;
                }
                catch (Exception ex)
                {
                    if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                    if (fail != null) { await fail(ex); }
                }
                finally
                {
                    if (finallyAction != null) { await finallyAction(); }
                }
            }
            return t;
        }

        public static void Doing(this Action fn, Action success = null, Action<Exception> fail = null, Action finallyAction = null)
        {
            if (fn == null) { return; }

            try
            {
                fn();
                if (success != null) { success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                if (fail != null)
                {
                    fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { finallyAction(); }
            }
        }
        public static T Doing<T>(this Func<T> fn, Action success = null, Action<Exception> fail = null, Action finallyAction = null)
        {
            T t = default(T);
            if (fn == null) { return t; }

            try
            {
                t = fn();
                if (success != null) { success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(ex); }
                if (fail != null)
                {
                    fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { finallyAction(); }
            }
            return t;
        }

        public static async Task DoingAsync(this EventHandler fn, object sender, EventArgs args, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                fn(sender, args);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }
        public static async Task DoingAsync<T>(this EventHandler<T> fn, object sender, T args, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null) where T : EventArgs
        {
            if (fn == null) { return; }
            try
            {
                fn(sender, args);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }
        public static async Task DoingAsync(this Func<Task> fn, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                await fn();
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T>(this Func<T, Task> fn, T arg, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                await fn(arg);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2>(this Func<T1, T2, Task> fn, T1 arg1, T2 arg2, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                await fn(arg1, arg2);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2, T3>(this Func<T1, T2, T3, Task> fn, T1 arg1, T2 arg2, T3 arg3, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                await fn(arg1, arg2, arg3);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2, T3, T4>(this Func<T1, T2, T3, T4, Task> fn, T1 arg1, T2 arg2, T3 arg3, T4 arg4, Func<Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                await fn(arg1, arg2, arg3, arg4);
                if (success != null) { await success(); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<TResult>(this Func<Task<TResult>> fn, Func<TResult, Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                var result = await fn();
                if (success != null) { await success(result); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T, TResult>(this Func<T, Task<TResult>> fn, T arg1, Func<TResult, Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                var result = await fn(arg1);
                if (success != null) { await success(result); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> fn, T1 arg1, T2 arg2, Func<TResult, Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                var result = await fn(arg1, arg2);
                if (success != null) { await success(result); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>> fn, T1 arg1, T2 arg2, T3 arg3, Func<TResult, Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                var result = await fn(arg1, arg2, arg3);
                if (success != null) { await success(result); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }

        public static async Task DoingAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, Task<TResult>> fn, T1 arg1, T2 arg2, T3 arg3, T4 arg4, Func<TResult, Task> success = null, Func<Exception, Task> fail = null, Func<Task> finallyAction = null)
        {
            if (fn == null) { return; }
            try
            {
                var result = await fn(arg1, arg2, arg3, arg4);
                if (success != null) { await success(result); }
            }
            catch (Exception ex)
            {
                if (TodoExceptionHook != null) { await TodoExceptionHook(ex); }
                if (fail != null)
                {
                    await fail(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (finallyAction != null) { await finallyAction(); }
            }
        }
        #endregion

        #region Task
        public static Task TodoTask(this Action fn, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(null, e =>
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                if (fail != null) { fail(e); }
            }, finallyAction, taskCreationOptions);
        }

        public static IEnumerable<Task> TodoTaskTimes(this Action fn, Action<Exception> fail = null, Action finallyAction = null, int times = 1, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { yield return Task.CompletedTask; }
            for (var i = 0; i < times; i++)
            {
                yield return fn.DoingTask(null, e =>
                {
                    if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                    if (fail != null) { fail(e); }
                }, finallyAction, taskCreationOptions);
            }
        }

        public static Task DoingTask(this Action fn, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn();
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask(this EventHandler fn, object sender, EventArgs eventArgs, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(sender,eventArgs,null, e =>
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                if (fail != null) { fail(e); }
            }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask(this EventHandler fn, object sender, EventArgs eventArgs, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn(sender, eventArgs);
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T>(this Action<T> fn, T t, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, null, e =>
             {
                 if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                 if (fail != null) { fail(e); }
             }, finallyAction, taskCreationOptions);
        }

        public static IEnumerable<Task> TodoTaskTimes<T>(this Action<T> fn, T t, Action<Exception> fail = null, Action finallyAction = null, int times = 1, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { yield return Task.CompletedTask; }
            for (var i = 0; i < times; i++)
            {
                yield return fn.DoingTask(t, null, e =>
                 {
                     if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                     if (fail != null) { fail(e); }
                 }, finallyAction, taskCreationOptions);
            }
        }

        public static Task DoingTask<T>(this Action<T> fn, T t, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn(t);
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T>(this EventHandler<T> fn,object sender, T t, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(sender,t, null, e =>
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                if (fail != null) { fail(e); }
            }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask<T>(this EventHandler<T> fn, object sender, T t, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn(sender, t);
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T, T1>(this Action<T, T1> fn, T t, T1 t1, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, t1, null, e =>
             {
                 if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                 if (fail != null) { fail(e); }
             }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask<T, T1>(this Action<T, T1> fn, T t, T1 t1, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn(t, t1);
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T, T1, T2>(this Action<T, T1, T2> fn, T t, T1 t1, T2 t2, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, t1, t2, null, e =>
             {
                 if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                 if (fail != null) { fail(e); }
             }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask<T, T1, T2>(this Action<T, T1, T2> fn, T t, T1 t1, T2 t2, Action success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    fn(t, t1, t2);
                    if (success != null) { success(); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<TResult>(this Func<TResult> fn, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(null, e =>
            {
                if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                if (fail != null) { fail(e); }
            }, finallyAction, taskCreationOptions);
        }

        public static IEnumerable<Task> TodoTaskTimes<TResult>(this Func<TResult> fn, Action<Exception> fail = null, Action finallyAction = null, int times = 1, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { yield return Task.CompletedTask; }
            for (var i = 0; i < times; i++)
            {
                yield return fn.DoingTask(null, e =>
                {
                    if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                    if (fail != null) { fail(e); }
                }, finallyAction, taskCreationOptions);
            }
        }

        public static Task DoingTask<TResult>(this Func<TResult> fn, Action<TResult> success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = fn();
                    if (success != null) { success(result); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T, TResult>(this Func<T, TResult> fn, T t, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, null, e =>
             {
                 if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                 if (fail != null) { fail(e); }
             }, finallyAction, taskCreationOptions);
        }

        public static IEnumerable<Task> TodoTaskTimes<T, TResult>(this Func<T, TResult> fn, T t, Action<Exception> fail = null, Action finallyAction = null, int times = 1, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { yield return Task.CompletedTask; }
            for (var i = 0; i < times; i++)
            {
                yield return fn.DoingTask(t, null, e =>
                 {
                     if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                     if (fail != null) { fail(e); }
                 }, finallyAction, taskCreationOptions);
            }
        }

        public static Task DoingTask<T, TResult>(this Func<T, TResult> fn, T t, Action<TResult> success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = fn(t);
                    if (success != null) { success(result); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T, T1, TResult>(this Func<T, T1, TResult> fn, T t, T1 t1, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, t1, null, e =>
             {
                 if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                 if (fail != null) { fail(e); }
             }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask<T, T1, TResult>(this Func<T, T1, TResult> fn, T t, T1 t1, Action<TResult> success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = fn(t, t1);
                    if (success != null) { success(result); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        public static Task TodoTask<T, T1, T2, TResult>(this Func<T, T1, T2, TResult> fn, T t, T1 t1, T2 t2, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            return fn.DoingTask(t, t1, t2, null, e =>
              {
                  if (TodoExceptionHook != null) { TodoExceptionHook(e); }
                  if (fail != null) { fail(e); }
              }, finallyAction, taskCreationOptions);
        }

        public static Task DoingTask<T, T1, T2, TResult>(this Func<T, T1, T2, TResult> fn, T t, T1 t1, T2 t2, Action<TResult> success = null, Action<Exception> fail = null, Action finallyAction = null, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            if (fn == null) { return Task.CompletedTask; }
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = fn(t, t1, t2);
                    if (success != null) { success(result); }
                }
                catch (Exception ex)
                {
                    if (fail != null)
                    {
                        fail(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    if (finallyAction != null) { finallyAction(); }
                }
            }, taskCreationOptions);
        }

        #endregion
    }

    public static class While
    {
        public static void Todo(Func<int, bool> func)
        {
            var i = 0;
            while (true) { if (!func(i)) { break; } i++; }
        }
        public static async Task TodoAsync(Func<int, Task<bool>> func)
        {
            await Try.TodoAsync(async () =>
            {
                var i = 0;
                while (true) { var r = await func(i); if (!r) { break; } i++; }
            });
        }
        public static void Todo<T>(T defaultValue, Func<int, T, bool> func)
        {
            var i = 0;
            while (true) { if (!func(i, defaultValue)) { break; } i++; }
        }
        public static async Task TodoAsync<T>(T defaultValue, Func<int, T, Task<bool>> func)
        {
            await Try.TodoAsync(async () =>
            {
                var i = 0;
                while (true) { var r = await func(i, defaultValue); if (!r) { break; } i++; }
            });
        }

    }

    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) { return; }
            foreach (var s in source) { action(s); }
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null) { return; }
            var i = 0;
            foreach (var s in source) { action(s, i); i++; }
        }
        public static void ForEach<T>(this System.Collections.IEnumerator source, Action<T> action)
        {
            if (source == null) { return; }
            while (source.MoveNext())
            {
                action((T)source.Current);
            }
        }
        public static void ForEach<T>(this System.Collections.IEnumerable source, Action<T> action)
        {
            if (source == null) { return; }
            var enumer = source.GetEnumerator();
            while (enumer.MoveNext())
            {
                action((T)enumer.Current);
            }
        }

        // 可使用 IEnumerable<KeyValuePair<,>>版本
        //public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> action)
        //{
        //    if (source == null) { return; }
        //    foreach (var s in source) { action(s); }
        //}
        //public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>, int> action)
        //{
        //    if (source == null) { return; }
        //    var i = 0;
        //    foreach (var s in source) { action(s, i); i++; }
        //}
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            if (source == null) { return; }
            foreach (var s in source) 
            { 
                cancellationToken.ThrowIfCancellationRequested();
                await action(s); 
            }
        }
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> action, CancellationToken cancellationToken = default)
        {
            if (source == null) { return; }
            var i = 0;
            foreach (var s in source) 
            { 
                cancellationToken.ThrowIfCancellationRequested();
                await action(s, i); 
                i++; 
            }
        }
        public static async Task ForEachAsync<T>(this System.Collections.IEnumerator source, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            if (source == null) { return; }
            while (source.MoveNext())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action((T)source.Current);
            }
        }
        public static async Task ForEachAsync<T>(this System.Collections.IEnumerable source, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            if (source == null) { return; }
            var enumer = source.GetEnumerator();
            while (enumer.MoveNext())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action((T)enumer.Current);
            }
        }
        //public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, Task> action)
        //{
        //    if (source == null) { return; }
        //    foreach (var s in source) { await action(s); }
        //}
        //public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, int, Task> action)
        //{
        //    if (source == null) { return; }
        //    var i = 0;
        //    foreach (var s in source) { await action(s, i); i++; }
        //}
    }
}
