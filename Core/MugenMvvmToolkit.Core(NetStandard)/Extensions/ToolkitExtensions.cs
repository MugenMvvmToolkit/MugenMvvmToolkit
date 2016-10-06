#region Copyright

// ****************************************************************************
// <copyright file="ToolkitExtensions.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.ViewModels;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit
{
    public static class ToolkitExtensions
    {
        #region Nested types

        private sealed class ThreadManagerClosure<TTarget, TArg1, TArg2>
        {
            #region Fields

            private readonly Action<TTarget, TArg1, TArg2> _actionTwoArg;
            private readonly Action<TTarget, TArg1> _actionOneArg;
            private readonly TTarget _target;
            private readonly TArg1 _arg1;
            private readonly TArg2 _arg2;

            #endregion

            #region Constructors

            private ThreadManagerClosure(TTarget target, TArg1 arg1, TArg2 arg2)
            {
                _target = target;
                _arg1 = arg1;
                _arg2 = arg2;
            }

            public ThreadManagerClosure(Action<TTarget, TArg1> actionOneArg, TTarget target, TArg1 arg1)
                : this(target, arg1, default(TArg2))
            {
                _actionOneArg = actionOneArg;
            }

            public ThreadManagerClosure(Action<TTarget, TArg1, TArg2> actionTwoArg, TTarget target, TArg1 arg1, TArg2 arg2)
                : this(target, arg1, arg2)
            {
                _actionTwoArg = actionTwoArg;
            }

            #endregion

            #region Methods

            public void Invoke()
            {
                if (_actionOneArg == null)
                    _actionTwoArg(_target, _arg1, _arg2);
                else
                    _actionOneArg(_target, _arg1);
            }

            #endregion
        }

        private sealed class DataContextDictionaryWrapper : IDictionary<object, object>
        {
            #region Fields

            public readonly IDataContext Context;

            #endregion

            #region Constructors

            public DataContextDictionaryWrapper(IDataContext context)
            {
                Context = context;
            }

            #endregion

            #region Implementation of IDictionary<object,object>

            public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
            {
                return Context
                    .ToList()
                    .Select(value => new KeyValuePair<object, object>(value.DataConstant, value.Value))
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(KeyValuePair<object, object> item)
            {
                Add(item.Key, item.Value);
            }

            public void Clear()
            {
                Context.Clear();
            }

            public bool Contains(KeyValuePair<object, object> item)
            {
                return ContainsKey(item.Key);
            }

            public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
            {
                Context.ToList()
                    .Select(value => new KeyValuePair<object, object>(value.DataConstant, value.Value))
                    .ToList()
                    .CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<object, object> item)
            {
                return Remove(item.Key);
            }

            public int Count => Context.Count;

            public bool IsReadOnly => false;

            public void Add(object key, object value)
            {
                Context.Add(GetConstant(key), value);
            }

            public bool ContainsKey(object key)
            {
                return Context.Contains(GetSimpleConstant(key));
            }

            public bool Remove(object key)
            {
                return Context.Remove(GetSimpleConstant(key));
            }

            public bool TryGetValue(object key, out object value)
            {
                return Context.TryGetData(GetConstant(key), out value);
            }

            public object this[object key]
            {
                get { return Context.GetData(GetConstant(key), true); }
                set { Context.AddOrUpdate(GetConstant(key), value); }
            }

            public ICollection<object> Keys
            {
                get
                {
                    if (Context.Count == 0)
                        return Empty.Array<object>();
                    var list = Context.ToList();
                    var values = new List<object>(list.Count);
                    values.AddRange(list.Select(value => (object)value.DataConstant));
                    return values;
                }
            }

            public ICollection<object> Values
            {
                get
                {
                    if (Context.Count == 0)
                        return Empty.Array<object>();
                    var list = Context.ToList();
                    var values = new List<object>(list.Count);
                    values.AddRange(list.Select(value => value.Value));
                    return values;
                }
            }

            #endregion

            #region Methods

            private static DataConstant GetSimpleConstant(object key)
            {
                var s = key as string;
                if (s == null)
                    return (DataConstant)key;
                return new DataConstant((string)key, false);
            }

            private static DataConstant<object> GetConstant(object key)
            {
                return key as string ?? new DataConstant<object>((DataConstant)key);
            }

            #endregion

        }

        #endregion

        #region Fields

        private static readonly ManualResetEvent Sleeper;

        #endregion

        #region Constructors

        static ToolkitExtensions()
        {
            Sleeper = new ManualResetEvent(false);
            ShortDuration = 2000;
            LongDuration = 3500;
        }

        #endregion

        #region Properties

        public static float ShortDuration { get; set; }

        public static float LongDuration { get; set; }

        #endregion

        #region Ioc adapter extensions

        [Pure]
        public static IIocContainer GetRoot([NotNull] this IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            while (iocContainer.Parent != null)
                iocContainer = iocContainer.Parent;
            return iocContainer;
        }

        [Pure]
        public static T Get<T>([NotNull] this IIocContainer iocContainer, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            return (T)iocContainer.Get(typeof(T), name, parameters);
        }

        [Pure]
        public static bool TryGet<T>([NotNull] this IIocContainer iocContainer, out T service, string name = null,
            params IIocParameter[] parameters)
        {
            object objService;
            var tryGet = iocContainer.TryGet(typeof(T), out objService, name, parameters);
            if (tryGet)
            {
                service = (T)objService;
                return true;
            }
            service = default(T);
            return false;
        }

        [Pure]
        public static bool TryGet([NotNull] this IIocContainer iocContainer, [NotNull] Type serviceType, out object service, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            Should.NotBeNull(serviceType, nameof(serviceType));
            if (iocContainer.CanResolve(serviceType))
            {
                try
                {
                    service = iocContainer.Get(serviceType, name, parameters);
                    return true;
                }
                catch
                {
                    service = null;
                    return false;
                }
            }
            service = null;
            return false;
        }

        [Pure]
        public static IEnumerable<T> GetAll<T>([NotNull] this IIocContainer iocContainer, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            return iocContainer.GetAll(typeof(T), name, parameters).Cast<T>();
        }

        public static void Bind<T, TTypeTo>([NotNull] this IIocContainer iocContainer, DependencyLifecycle lifecycle,
            string name = null, params IIocParameter[] parameters)
            where TTypeTo : T
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            iocContainer.Bind(typeof(T), typeof(TTypeTo), lifecycle, name, parameters);
        }

        public static void BindToConstant<T>([NotNull] this IIocContainer iocContainer, T instance, string name = null)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            iocContainer.BindToConstant(typeof(T), instance, name);
        }

        public static void BindToMethod<T>([NotNull] this IIocContainer iocContainer,
            [NotNull] Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            iocContainer.BindToMethod(typeof(T), methodBindingDelegate.AsMethodBindingDelegateObject, lifecycle, name, parameters);
        }

        public static void BindToBindingInfo<T>(this IIocContainer iocContainer, BindingInfo<T> binding)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            if (binding.IsEmpty)
                return;
            if (iocContainer.CanResolve<T>(binding.Name))
                Tracer.Info("The binding with type {0} already exists.", typeof(T));
            else
                binding.SetBinding(iocContainer);
        }

        public static void Unbind<T>([NotNull] this IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            iocContainer.Unbind(typeof(T));
        }

        [Pure]
        public static bool CanResolve<T>([NotNull] this IIocContainer iocContainer, string name = null)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            return iocContainer.CanResolve(typeof(T), name);
        }

        [Pure]
        public static T GetService<T>([NotNull] this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            var service = serviceProvider.GetService(typeof(T));
            if (service == null)
                return default(T);
            return (T)service;
        }

        #endregion

        #region Exception extension

        [Pure]
        public static string Flatten([NotNull] this Exception exception, bool includeStackTrace = false)
        {
            return exception.Flatten(string.Empty, includeStackTrace);
        }

        [Pure]
        public static string Flatten([NotNull] this Exception exception, string message, bool includeStackTrace = false)
        {
            Should.NotBeNull(exception, nameof(exception));
            var sb = new StringBuilder(message);
            FlattenInternal(exception, sb, includeStackTrace);
            return sb.ToString();
        }

        private static void FlattenInternal(Exception exception, StringBuilder sb, bool includeStackTrace)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                sb.AppendLine(aggregateException.Message);
                if (includeStackTrace)
                {
                    sb.Append(exception.StackTrace);
                    sb.AppendLine();
                }
                for (int index = 0; index < aggregateException.InnerExceptions.Count; index++)
                    FlattenInternal(aggregateException.InnerExceptions[index], sb, includeStackTrace);
                return;
            }

            while (exception != null)
            {
                sb.AppendLine(exception.Message);
                if (includeStackTrace)
                    sb.Append(exception.StackTrace);

                var loadException = exception as ReflectionTypeLoadException;
                if (loadException != null)
                {
                    if (includeStackTrace)
                        sb.AppendLine();
                    for (int index = 0; index < loadException.LoaderExceptions.Length; index++)
                        FlattenInternal(loadException.LoaderExceptions[index], sb, includeStackTrace);
                }

                exception = exception.InnerException;
                if (exception != null && includeStackTrace)
                    sb.AppendLine();
            }
        }

        #endregion

        #region String extensions

        [Pure]
        public static bool SafeContains([CanBeNull] this string source, [CanBeNull] string value,
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
                return false;
            return source.IndexOf(value, stringComparison) >= 0;
        }

        #endregion

        #region Task extensions

        [Pure]
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public static Task TryExecuteSynchronously<T>([NotNull] this Task<T> task, [NotNull] Action<Task<T>> action,
            CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(action, nameof(action));
            if (!task.IsCompleted)
                return task.ContinueWith(action, token,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            try
            {
                action(task);
                return task;
            }
            catch (OperationCanceledException e)
            {
                return CreateExceptionTask<object>(e, true);
            }
            catch (Exception exception)
            {
                return CreateExceptionTask<object>(exception, false);
            }
        }

        public static Task<TResult> TryExecuteSynchronously<T, TResult>([NotNull] this Task<T> task,
            [NotNull] Func<Task<T>, TResult> action, CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(action, nameof(action));
            if (!task.IsCompleted)
                return task.ContinueWith(action, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            try
            {
                return FromResult(action(task));
            }
            catch (OperationCanceledException e)
            {
                return CreateExceptionTask<TResult>(e, true);
            }
            catch (Exception exception)
            {
                return CreateExceptionTask<TResult>(exception, false);
            }
        }

        public static Task TryExecuteSynchronously([NotNull] this Task task, [NotNull] Action<Task> action,
            CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(action, nameof(action));
            if (!task.IsCompleted)
                return task.ContinueWith(action, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            try
            {
                action(task);
                return task;
            }
            catch (OperationCanceledException e)
            {
                return CreateExceptionTask<object>(e, true);
            }
            catch (Exception exception)
            {
                return CreateExceptionTask<object>(exception, false);
            }
        }

        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(viewModel, nameof(viewModel));
            return task.WithTaskExceptionHandler(viewModel, viewModel.GetIocContainer(true, false));
        }

        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] object sender,
            IIocContainer iocContainer = null)
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(sender, nameof(sender));
            if (task.IsCompleted)
                TryHandleTaskException(task, sender, iocContainer);
            else
                task.TryExecuteSynchronously(t => TryHandleTaskException(t, sender, iocContainer));
            return task;
        }

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task)
        {
            Should.NotBeNull(tcs, nameof(tcs));
            Should.NotBeNull(task, nameof(task));
            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.TrySetException(task.Exception.InnerExceptions);
                        break;
                    case TaskStatus.RanToCompletion:
                        var t = task as Task<TResult>;
                        tcs.TrySetResult(t == null ? default(TResult) : t.Result);
                        break;
                }
            }
            else
            {
#if NET_STANDARD
                task.ContinueWith((t, o) => ((TaskCompletionSource<TResult>)o).TrySetFromTask(t), tcs, TaskContinuationOptions.ExecuteSynchronously);
#else
                task.ContinueWith(tcs.TrySetFromTask, TaskContinuationOptions.ExecuteSynchronously);
#endif
            }

        }

        internal static Task WhenAll(Task[] tasks)
        {
            if (tasks == null)
                return Empty.Task;
            if (tasks.Length == 0)
                return Empty.Task;
            if (tasks.Length == 1)
                return tasks[0];
            return Task.Factory.ContinueWhenAll(tasks, waitTasks =>
            {
                for (int index = 0; index < waitTasks.Length; index++)
                    waitTasks[index].Wait();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        internal static void TryHandleTaskException(Task task, object sender, IIocContainer iocContainer)
        {
            if (!task.IsCanceled && !task.IsFaulted)
                return;
            if (task.Exception != null)
                Tracer.Error(task.Exception.Flatten(true));
            if (iocContainer == null || iocContainer.IsDisposed)
                iocContainer = ServiceProvider.IocContainer;
            if (iocContainer == null || iocContainer.IsDisposed || !iocContainer.CanResolve<ITaskExceptionHandler>())
                return;
            foreach (ITaskExceptionHandler handler in iocContainer.GetAll<ITaskExceptionHandler>())
                handler.Handle(sender, task);
        }

        private static Task<T> CreateExceptionTask<T>(Exception exception, bool isCanceled)
        {
            if (isCanceled)
                return Empty.CanceledTask<T>();
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        #endregion

        #region Collection extensions

        public static bool MoveUpItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            if (itemsSource == null)
                return false;
            int indexOf = itemsSource.IndexOf(item);
            if (indexOf <= 0)
                return false;
            itemsSource.RemoveAt(indexOf);
            itemsSource.Insert(indexOf - 1, item);
            return true;
        }

        public static bool MoveDownItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            if (itemsSource == null)
                return false;
            int indexOf = itemsSource.IndexOf(item);
            if (indexOf < 0 || indexOf >= itemsSource.Count - 1)
                return false;
            itemsSource.RemoveAt(indexOf);
            itemsSource.Insert(indexOf + 1, item);
            return true;
        }

        public static bool CanMoveUpItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            return itemsSource != null && itemsSource.IndexOf(item) > 0;
        }

        public static bool CanMoveDownItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            if (itemsSource == null)
                return false;
            var indexOf = itemsSource.IndexOf(item);
            return indexOf >= 0 && indexOf < itemsSource.Count - 1;
        }

        public static int IndexOf([CanBeNull]this IEnumerable enumerable, object value, IEqualityComparer<object> comparer = null)
        {
            if (enumerable == null)
                return -1;
            if (comparer == null)
                comparer = EqualityComparer<object>.Default;
            var list = enumerable as IList;
            if (list != null)
                return list.IndexOf(value);
            var enumerator = enumerable.GetEnumerator();
            try
            {
                int index = 0;
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, value))
                        return index;
                    index++;
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
            return -1;
        }

        public static object ElementAtIndex([NotNull]this IEnumerable enumerable, int index)
        {
            Should.NotBeNull(enumerable, nameof(enumerable));
            var list = enumerable as IList;
            if (list != null)
                return list[index];
            return enumerable.OfType<object>().ElementAt(index);
        }

        public static T[] ToArrayEx<T>([NotNull] this IList<T> list)
        {
            Should.NotBeNull(list, nameof(list));
            if (list.Count == 0)
                return Empty.Array<T>();
            var array = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }

        public static TResult[] ToArrayEx<T, TResult>([NotNull] this IList<T> list,
            [NotNull] Func<T, TResult> selector)
        {
            Should.NotBeNull(list, nameof(list));
            Should.NotBeNull(selector, nameof(selector));
            if (list.Count == 0)
                return Empty.Array<TResult>();
            var array = new TResult[list.Count];
            for (int i = 0; i < list.Count; i++)
                array[i] = selector(list[i]);
            return array;
        }

        public static TResult[] ToArrayEx<T, TResult>([NotNull] this ICollection<T> collection,
            [NotNull] Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            int count = collection.Count;
            if (count == 0)
                return Empty.Array<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (T item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static T[] ToArrayEx<T>([NotNull] this ICollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            int count = collection.Count;
            if (count == 0)
                return Empty.Array<T>();
            var array = new T[count];
            count = 0;
            foreach (T item in collection)
                array[count++] = item;
            return array;
        }

        public static void AddIfNotNull<T>([NotNull] this ICollection<T> collection, T item)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (item != null)
                collection.Add(item);
        }

        [Pure]
        public static bool IsNullOrEmpty<T>([CanBeNull] this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        [Pure]
        public static bool IsNullOrEmpty([CanBeNull] this IEnumerable enumerable)
        {
            return enumerable == null || enumerable.IsEmpty();
        }

        [Pure]
        public static bool IsEmpty([NotNull] this IEnumerable enumerable)
        {
            Should.NotBeNull(enumerable, nameof(enumerable));
            var collection = enumerable as ICollection;
            if (collection != null)
                return collection.Count == 0;
#if NET_STANDARD
            var readOnlyCollection = enumerable as IReadOnlyCollection<object>;
            if (readOnlyCollection != null)
                return readOnlyCollection.Count == 0;
#endif
            IEnumerator enumerator = enumerable.GetEnumerator();
            try
            {
                return !enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        [Pure]
        public static int Count([CanBeNull] this IEnumerable source)
        {
            if (source == null)
                return 0;
            var collection = source as ICollection;
            if (collection != null)
                return collection.Count;
#if NET_STANDARD
            var readOnlyCollection = source as IReadOnlyCollection<object>;
            if (readOnlyCollection != null)
                return readOnlyCollection.Count;
#endif
            int num = 0;
            var enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    checked { ++num; }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
            return num;
        }

        public static void ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(action, nameof(action));
            var list = collection as IList<T>;
            if (list == null)
            {
                foreach (T o in collection)
                    action(o);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                    action(list[i]);
            }
        }

        public static void AddRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items, bool suspendNotifications = true)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            var list = collection as List<T>;
            if (list != null)
            {
                list.AddRange(items);
                return;
            }
            var notifiableCollection = collection as INotifiableCollection<T>;
            if (notifiableCollection != null)
            {
                notifiableCollection.AddRange(items, suspendNotifications);
                return;
            }
            foreach (T item in items)
                collection.Add(item);
        }

        public static void AddRange<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> target,
            [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            foreach (var keyValuePair in source)
                target[keyValuePair.Key] = keyValuePair.Value;
        }

        public static void RemoveRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items, bool suspendNotifications = true)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            var notifiableCollection = collection as INotifiableCollection<T>;
            if (notifiableCollection != null)
            {
                notifiableCollection.RemoveRange(items, suspendNotifications);
                return;
            }
            var list = items.ToList();
            for (int index = 0; index < list.Count; index++)
                collection.Remove(list[index]);
        }

        [Pure]
        public static SynchronizedNotifiableCollection<T> ToSynchronizedCollection<T>(
            [NotNull] this IEnumerable<T> collection, [NotNull] IThreadManager threadManager = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            return collection as SynchronizedNotifiableCollection<T> ??
                   new SynchronizedNotifiableCollection<T>(collection, threadManager);
        }

        public static FilterableNotifiableCollection<T> ToFilterableCollection<T>(
            [NotNull] this IEnumerable<T> collection, IThreadManager threadManager = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            return collection as FilterableNotifiableCollection<T> ??
                   new FilterableNotifiableCollection<T>(collection, threadManager);
        }

        #endregion

        #region Callbacks

        [NotNull]
        public static IAsyncOperation ContinueWith([NotNull] this IAsyncOperation operation, [NotNull] Action<IOperationResult> continuationAction)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationAction, nameof(continuationAction));
            return operation.ContinueWith(continuationAction: new DelegateContinuation<object, object, object>(continuationAction));
        }

        [NotNull]
        public static IAsyncOperation<TResult> ContinueWith<TResult>([NotNull] this IAsyncOperation operation,
            [NotNull] Func<IOperationResult, TResult> continuationFunction)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationFunction, nameof(continuationFunction));
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<object, TResult, object>(continuationFunction));
        }

        [NotNull]
        public static IAsyncOperation ContinueWith<TResult>([NotNull] this IAsyncOperation<TResult> operation, [NotNull] Action<IOperationResult<TResult>> continuationActionGeneric)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationActionGeneric, nameof(continuationActionGeneric));
            return operation.ContinueWith(continuationAction: new DelegateContinuation<TResult, object, object>(continuationActionGeneric));
        }

        [NotNull]
        public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>([NotNull] this IAsyncOperation<TResult> operation,
            [NotNull] Func<IOperationResult<TResult>, TNewResult> continuationFunctionGeneric)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationFunctionGeneric, nameof(continuationFunctionGeneric));
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<TResult, TNewResult, object>(continuationFunctionGeneric));
        }

        [NotNull]
        public static IAsyncOperation ContinueWith<TSource, TResult>([NotNull] this IAsyncOperation<TResult> operation, [NotNull] Action<TSource, IOperationResult<TResult>> continuationAction)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationAction, nameof(continuationAction));
            return operation.ContinueWith(continuationAction: new DelegateContinuation<TResult, object, TSource>(continuationAction));
        }

        [NotNull]
        public static IAsyncOperation<TNewResult> ContinueWith<TSource, TResult, TNewResult>([NotNull] this IAsyncOperation<TResult> operation,
             [NotNull] Func<TSource, IOperationResult<TResult>, TNewResult> continuationFunction)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(continuationFunction, nameof(continuationFunction));
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<TResult, TNewResult, TSource>(continuationFunction));
        }

        public static Task<T> AsTask<T>([NotNull] this IAsyncOperation<T> operation)
        {
            Should.NotBeNull(operation, nameof(operation));
            var tcs = new TaskCompletionSource<T>();
            operation.ContinueWith(continuationActionGeneric: tcs.AsContinuationAction);
            return tcs.Task;
        }

        public static Task AsTask([NotNull] this IAsyncOperation operation)
        {
            Should.NotBeNull(operation, nameof(operation));
            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(tcs.AsContinuationAction);
            return tcs.Task;
        }

        public static IAsyncOperationAwaiter GetAwaiter(this IAsyncOperationAwaiter awaiter)
        {
            return awaiter;
        }

        public static IAsyncOperationAwaiter<TResult> GetAwaiter<TResult>(this IAsyncOperationAwaiter<TResult> awaiter)
        {
            return awaiter;
        }

        public static IAsyncOperationAwaiter GetAwaiter([NotNull] this IAsyncOperation operation)
        {
            Should.NotBeNull(operation, nameof(operation));
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, DataContext.Empty);
        }

        public static IAsyncOperationAwaiter<TResult> GetAwaiter<TResult>([NotNull] this IAsyncOperation<TResult> operation)
        {
            Should.NotBeNull(operation, nameof(operation));
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, DataContext.Empty);
        }

        public static IAsyncOperationAwaiter ConfigureAwait([NotNull] this IAsyncOperation operation, bool continueOnCapturedContext)
        {
            Should.NotBeNull(operation, nameof(operation));
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, new DataContext(OpeartionCallbackConstants.ContinueOnCapturedContext.ToValue(continueOnCapturedContext)));
        }

        public static IAsyncOperationAwaiter<TResult> ConfigureAwait<TResult>([NotNull] this IAsyncOperation<TResult> operation, bool continueOnCapturedContext)
        {
            Should.NotBeNull(operation, nameof(operation));
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, new DataContext(OpeartionCallbackConstants.ContinueOnCapturedContext.ToValue(continueOnCapturedContext)));
        }

        #endregion

        #region Extensions

        public static void OnPropertyChanged<TModel>(this TModel model, Func<Expression<Func<TModel, object>>> expression, ExecutionMode? executionMode = null)
            where TModel : NotifyPropertyChangedBase
        {
            model.OnPropertyChanged(expression.GetMemberName(), executionMode);
        }

        public static void SetProperty<TModel, T>([NotNull] this TModel model, ref T field, T newValue, Func<Expression<Func<TModel, T>>> expression, ExecutionMode? executionMode = null)
            where TModel : NotifyPropertyChangedBase
        {
            Should.NotBeNull(model, nameof(model));
            model.SetProperty(ref field, newValue, expression.GetMemberName(), executionMode);
        }

        public static void OnPropertyChanged([NotNull] this NotifyPropertyChangedBase model, string propertyName, ExecutionMode? executionMode = null)
        {
            Should.NotBeNull(model, nameof(model));
            if (executionMode == null)
                model.OnPropertyChanged(propertyName);
            else
                model.OnPropertyChanged(propertyName, executionMode.Value);
        }

        public static void Register<T>([NotNull] this IValidatorProvider validatorProvider)
            where T : IValidator
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            validatorProvider.Register(typeof(T));
        }

        public static void Unregister<T>([NotNull] this IValidatorProvider validatorProvider)
            where T : IValidator
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            validatorProvider.Unregister(typeof(T));
        }

        [CanBeNull]
        public static ISubscriber Subscribe<TMessage>([NotNull] this IObservable observable, Action<object, TMessage> action, bool weakSubscription = true)
        {
            Should.NotBeNull(observable, nameof(observable));
            ISubscriber subscriber;
            if (weakSubscription && action.Target != null)
            {
                Should.BeSupported(!action.Target.GetType().IsAnonymousClass(),
                    "The anonymous delegate cannot be converted to weak delegate.");
                subscriber = new WeakActionSubscriber<TMessage>(action.Target, action.GetMethodInfo());
            }
            else
                subscriber = new ActionSubscriber<TMessage>(action);
            return observable.Subscribe(subscriber) ? subscriber : null;
        }

        public static bool Unsubscribe<TMessage>([NotNull] this IObservable observable, Action<object, TMessage> action)
        {
            Should.NotBeNull(observable, nameof(observable));
            return observable.Unsubscribe(new ActionSubscriber<TMessage>(action));
        }

        [CanBeNull]
        public static ISubscriber Subscribe([NotNull] this IObservable observable, [NotNull] object instance, IDataContext context = null)
        {
            Should.NotBeNull(observable, nameof(observable));
            var subscriber = ServiceProvider.ObjectToSubscriberConverter?.Invoke(instance, context);
            if (subscriber == null)
                return null;
            return observable.Subscribe(subscriber) ? subscriber : null;
        }

        public static bool Unsubscribe([NotNull] this IObservable observable, [NotNull]object instance, IDataContext context = null)
        {
            Should.NotBeNull(observable, nameof(observable));
            var subscriber = ServiceProvider.ObjectToSubscriberConverter?.Invoke(instance, context);
            if (subscriber == null)
                return false;
            return observable.Unsubscribe(subscriber);
        }

        public static WeakReference GetWeakReference(object item)
        {
            var hasWeak = item as IHasWeakReference;
            if (hasWeak == null)
                return ServiceProvider.WeakReferenceFactory(item);
            return hasWeak.WeakReference;
        }

        public static TView GetUnderlyingView<TView>([CanBeNull] this IView view)
        {
            return GetUnderlyingView<TView>(viewObj: view);
        }

        public static TView GetUnderlyingView<TView>([CanBeNull]object viewObj)
        {
            var wrapper = viewObj as IViewWrapper;
            if (wrapper == null)
                return (TView)viewObj;
            return (TView)wrapper.View;
        }

        public static IValidatorAggregator GetValidatorAggregator([NotNull] this IValidatorProvider validatorProvider,
            [NotNull] object instanceToValidate)
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            var aggregator = validatorProvider.GetValidatorAggregator();
            aggregator.AddInstance(instanceToValidate);
            return aggregator;
        }

        public static void SetValidatorErrors<T>([NotNull] this IValidatorAggregator aggregator, Func<Expression<Func<T, object>>> expresssion, params object[] errors)
        {
            Should.NotBeNull(aggregator, nameof(aggregator));
            aggregator.Validator.SetErrors(expresssion, errors);
        }

        public static void SetValidatorErrors<T>([NotNull] this T aggregator, Func<Expression<Func<T, object>>> expresssion, params object[] errors)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(aggregator, nameof(aggregator));
            aggregator.Validator.SetErrors(expresssion, errors);
        }

        public static void SetValidatorErrors([NotNull] this IValidatorAggregator aggregator, string property, params object[] errors)
        {
            aggregator.Validator.SetErrors(property, errors);
        }

        public static TValidator AddValidator<TValidator>([NotNull] this IValidatorAggregator aggregator,
            [NotNull] object instanceToValidate) where TValidator : IValidator
        {
            Should.NotBeNull(aggregator, nameof(aggregator));
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            var validator = ServiceProvider.GetOrCreate<TValidator>();
            validator.Initialize(aggregator.CreateContext(instanceToValidate));
            aggregator.AddValidator(validator);
            return validator;
        }

        public static void ClearErrors<TModel>([NotNull] this IValidator validator, Func<Expression<Func<TModel, object>>> expresssion)
        {
            validator.ClearErrors(GetMemberName(expresssion));
        }

        public static void Publish([NotNull] this IEventPublisher eventPublisher, [NotNull] object sender, [NotNull] object message, ExecutionMode mode)
        {
            Should.NotBeNull(eventPublisher, nameof(eventPublisher));
            ServiceProvider.ThreadManager.Invoke(mode, eventPublisher, sender, message, (publisher, o, arg3) => publisher.Publish(o, arg3));
        }

        public static byte[] ToArray([NotNull] this Stream stream)
        {
            return stream.ToArray(0);
        }

        public static byte[] ToArray([NotNull] this Stream stream, int? position)
        {
            Should.NotBeNull(stream, nameof(stream));
            if (position.HasValue && position.Value != stream.Position)
                stream.Position = position.Value;
            var memoryStream = stream as MemoryStream;
            if (memoryStream != null)
                return memoryStream.ToArray();
            using (memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string SerializeToBase64String(this ISerializer serializer, object item)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            using (var stream = serializer.Serialize(item))
            {
                stream.Position = 0;
                var memoryStream = stream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                }
                using (memoryStream)
                    return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static T Deserialize<T>(this ISerializer serializer, string stringData)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            using (var ms = new MemoryStream(Convert.FromBase64String(stringData)))
                return (T)serializer.Deserialize(ms);
        }

        public static void Sleep(int millisecondsTimeout)
        {
            // Set is never called, so we wait always until the timeout occurs
            Sleeper.WaitOne(millisecondsTimeout);
        }

        public static void Sleep(TimeSpan timeout)
        {
            Sleep((int)timeout.TotalMilliseconds);
        }

        public static IToast ShowAsync(this IToastPresenter toastPresenter, object content, ToastDuration duration, ToastPosition position = ToastPosition.Bottom, IDataContext context = null)
        {
            float floatDuration;
            switch (duration)
            {
                case ToastDuration.Short:
                    floatDuration = ShortDuration;
                    break;
                case ToastDuration.Long:
                    floatDuration = LongDuration;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(duration));
            }
            return toastPresenter.ShowAsync(content, floatDuration, position, context);
        }

        public static IDictionary<object, object> ToDictionary([CanBeNull] this IDataContext context)
        {
            if (context == null)
                return new Dictionary<object, object>();
            return new DataContextDictionaryWrapper(context);
        }

        public static IDataContext ToDataContext([CanBeNull] this IEnumerable<KeyValuePair<object, object>> dictionary)
        {
            if (dictionary == null)
                return new DataContext();
            var wrapper = dictionary as DataContextDictionaryWrapper;
            if (wrapper == null)
                return new DataContext(dictionary
                    .Where(pair => pair.Key is DataConstant)
                    .ToDictionary(pair => (DataConstant)pair.Key, pair => pair.Value));
            return wrapper.Context;
        }

        public static void AddIfNotNull<T>([NotNull] this IDataContext context, [NotNull] DataConstant<T> data, T value)
            where T : class
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(data, nameof(data));
            if (value != null)
                context.Add(data, value);
        }

        public static T GetData<T>([NotNull] this IDataContext context, [NotNull] DataConstant<T> dataConstant,
            bool throwIfNotFound) where T : class
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            T result;
            if (!context.TryGetData(dataConstant, out result) && throwIfNotFound)
                throw ExceptionManager.DataConstantNotFound(dataConstant);
            return result;
        }

        [Pure]
        public static bool PropertyNameEqual<T>([NotNull] this PropertyChangedEventArgs args, [NotNull] Func<Expression<Func<T, object>>> getProperty)
        {
            Should.NotBeNull(args, nameof(args));
            return MemberNameEqual(args.PropertyName, getProperty.GetMemberName());
        }

        [Pure]
        public static bool PropertyNameEqual<T>([NotNull] this PropertyChangedEventArgs args, T item, [NotNull] Func<Expression<Func<T, object>>> getProperty)
        {
            return args.PropertyNameEqual(getProperty);
        }

        [Pure]
        public static bool MemberNameEqual<T>(string memberName, [NotNull] Func<Expression<Func<T, object>>> getMember)
        {
            return MemberNameEqual(memberName, getMember.GetMemberName(), false);
        }

        public static bool MemberNameEqual(string changedMember, string listenedMember, bool emptyListenedMemberResult = false)
        {
            if (string.IsNullOrEmpty(changedMember) ||
                changedMember.Equals(listenedMember, StringComparison.Ordinal))
                return true;
            if (string.IsNullOrEmpty(listenedMember))
                return emptyListenedMemberResult;

            if (listenedMember[0] == '[')
            {
                if (changedMember.Equals(ReflectionExtensions.IndexerName, StringComparison.Ordinal))
                    return true;
                if (changedMember.StartsWith("Item[", StringComparison.Ordinal))
                {
                    int i = 4, j = 0;
                    while (i < changedMember.Length)
                    {
                        var c1 = changedMember[i];
                        var c2 = listenedMember[j];
                        if (c1 == c2)
                        {
                            ++i;
                            ++j;
                        }
                        else if (c1 == '"')
                            ++i;
                        else if (c2 == '"')
                            ++j;
                        else
                            return false;
                    }
                    return j == listenedMember.Length;
                }
            }
            return false;
        }

        [Pure]
        public static string GetMemberName([NotNull] this Func<LambdaExpression> getLambdaExpression)
        {
            return getLambdaExpression.GetMemberInfo().Name;
        }

        [Pure]
        public static string GetMemberName<T>([NotNull] this Func<Expression<Func<T, object>>> expression)
        {
            return GetMemberName(getLambdaExpression: expression);
        }

        [Pure]
        public static string GetMemberName<T>([CanBeNull] T item, [NotNull] Func<Expression<Func<T, object>>> expression)
        {
            return GetMemberName(getLambdaExpression: expression);
        }

        public static void SetStateForAll([NotNull] this ITrackingCollection collection, EntityState state)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var item in collection)
                collection.UpdateState(item.Entity, state);
        }

        [Pure]
        public static bool HasChanges<T>(this IEntitySnapshot snapshot, T item, Func<Expression<Func<T, object>>> memberExpression)
        {
            Should.NotBeNull(snapshot, nameof(snapshot));
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(memberExpression, nameof(memberExpression));
            return snapshot.HasChanges(item, memberExpression.GetMemberName());
        }

        public static void SetStateForAll([NotNull] this ITrackingCollection collection, [NotNull] Func<TrackingEntity<object>, bool> predicate, EntityState state)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(predicate, nameof(predicate));
            foreach (var item in collection)
            {
                if (predicate(item))
                    collection.UpdateState(item.Entity, state);
            }
        }

        public static bool UpdateState([NotNull] this ITrackingCollection collection, [NotNull] object item, EntityState newState, EntityState updateState)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(item, nameof(item));
            if (collection.Contains(item))
                return collection.UpdateState(item, updateState);
            return collection.UpdateState(item, newState);
        }

        public static bool UpdateState([NotNull] this ITrackingCollection collection, [NotNull] IEntityStateEntry item)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(item, nameof(item));
            return collection.UpdateState(item.Entity, item.State);
        }

        public static void UpdateStates([NotNull] this ITrackingCollection collection, [NotNull] IEnumerable items, EntityState state)
        {
            Should.NotBeNull(items, nameof(items));
            foreach (object value in items)
                collection.UpdateState(value, state);
        }

        public static void UpdateStates([NotNull] this ITrackingCollection collection, [NotNull] IEnumerable<IEntityStateEntry> items)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            foreach (IEntityStateEntry entityStateEntry in items)
                collection.UpdateState(entityStateEntry);
        }

        public static bool Detach([NotNull] this ITrackingCollection collection, [NotNull] object entity)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(entity, nameof(entity));
            return collection.UpdateState(entity, EntityState.Detached);
        }

        [Pure]
        public static bool IsUnchanged(this EntityState es)
        {
            return (es & EntityState.Unchanged) == EntityState.Unchanged;
        }

        [Pure]
        public static bool IsAdded(this EntityState es)
        {
            return (es & EntityState.Added) == EntityState.Added;
        }

        [Pure]
        public static bool IsModified(this EntityState es)
        {
            return (es & EntityState.Modified) == EntityState.Modified;
        }

        [Pure]
        public static bool IsDetached(this EntityState es)
        {
            return (es & EntityState.Detached) == EntityState.Detached;
        }

        [Pure]
        public static bool IsDeleted(this EntityState es)
        {
            return (es & EntityState.Deleted) == EntityState.Deleted;
        }

        [Pure]
        public static bool IsAddedOrModified(this EntityState es)
        {
            return es.IsAdded() || es.IsModified();
        }

        [Pure]
        public static bool IsAddedOrModifiedOrUnchanged(this EntityState es)
        {
            return es.IsAddedOrModified() || es.IsUnchanged();
        }

        [Pure]
        public static bool IsAddedOrModifiedOrDeleted(this EntityState es)
        {
            return es.IsAddedOrModified() || es.IsDeleted();
        }

        [Pure]
        public static bool IsDeletedOrDetached(this EntityState es)
        {
            return es.IsDeleted() || es.IsDetached();
        }

        [Pure]
        public static bool IsDeletedOrModified(this EntityState es)
        {
            return es.IsDeleted() || es.IsModified();
        }

        [Pure]
        public static bool IsDesignMode(this LoadMode mode)
        {
            return mode.HasFlagEx(LoadMode.Design);
        }

        [Pure]
        public static bool IsUnitTestMode(this LoadMode mode)
        {
            return mode.HasFlagEx(LoadMode.UnitTest);
        }

        [Pure]
        public static bool IsRuntimeMode(this LoadMode mode)
        {
            return mode.HasFlagEx(LoadMode.Runtime);
        }

        [Pure]
        public static bool IsRuntimeDebugMode(this LoadMode mode)
        {
            return mode.HasFlagEx(LoadMode.RuntimeDebug);
        }

        [Pure]
        public static bool IsRuntimeReleaseMode(this LoadMode mode)
        {
            return mode.HasFlagEx(LoadMode.RuntimeRelease);
        }

        [NotNull]
        public static IDataContext ToNonReadOnly([CanBeNull] this IDataContext context)
        {
            if (context == null)
                return new DataContext();
            if (context.IsReadOnly)
                return new DataContext(context);
            return context;
        }

        public static void Invoke(this IThreadManager threadManager, ExecutionMode mode, Action invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default(CancellationToken))
        {
            switch (mode)
            {
                case ExecutionMode.SynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThread(invokeAction, priority, token);
                    break;
                case ExecutionMode.Asynchronous:
                    threadManager.InvokeAsync(invokeAction, priority, token);
                    break;
                case ExecutionMode.AsynchronousOnUiThread:
                    if (priority != OperationPriority.Low && threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThreadAsync(invokeAction, priority, token);
                    break;
                default:
                    invokeAction();
                    break;
            }
        }

        public static void Invoke<TTarget, TArg>(this IThreadManager threadManager, ExecutionMode mode, TTarget target, TArg arg1, Action<TTarget, TArg> invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default(CancellationToken))
        {
            switch (mode)
            {
                case ExecutionMode.SynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThread(new ThreadManagerClosure<TTarget, TArg, object>(invokeAction, target, arg1).Invoke, priority, token);
                    break;
                case ExecutionMode.Asynchronous:
                    threadManager.InvokeAsync(new ThreadManagerClosure<TTarget, TArg, object>(invokeAction, target, arg1).Invoke, priority, token);
                    break;
                case ExecutionMode.AsynchronousOnUiThread:
                    if (priority != OperationPriority.Low && threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThreadAsync(new ThreadManagerClosure<TTarget, TArg, object>(invokeAction, target, arg1).Invoke, priority, token);
                    break;
                default:
                    invokeAction(target, arg1);
                    break;
            }
        }

        public static void Invoke<TTarget, TArg1, TArg2>(this IThreadManager threadManager, ExecutionMode mode, TTarget target, TArg1 arg1, TArg2 arg2, Action<TTarget, TArg1, TArg2> invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default(CancellationToken))
        {
            switch (mode)
            {
                case ExecutionMode.SynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThread(new ThreadManagerClosure<TTarget, TArg1, TArg2>(invokeAction, target, arg1, arg2).Invoke, priority, token);
                    break;
                case ExecutionMode.Asynchronous:
                    threadManager.InvokeAsync(new ThreadManagerClosure<TTarget, TArg1, TArg2>(invokeAction, target, arg1, arg2).Invoke, priority, token);
                    break;
                case ExecutionMode.AsynchronousOnUiThread:
                    if (priority != OperationPriority.Low && threadManager.IsUiThread)
                        goto default;
                    threadManager.InvokeOnUiThreadAsync(new ThreadManagerClosure<TTarget, TArg1, TArg2>(invokeAction, target, arg1, arg2).Invoke, priority, token);
                    break;
                default:
                    invokeAction(target, arg1, arg2);
                    break;
            }
        }

        internal static void TraceModule(this IModule module, bool load)
        {
            if (Tracer.TraceInformation)
                Tracer.Info("The module '{0}' was {1}loaded.", module.GetType(), load ? null : "un");
        }

        internal static WeakReference GetWeakReferenceOrDefault(object item, WeakReference defaultValue, bool checkHasWeakReference)
        {
            if (item == null)
                return defaultValue;
            if (checkHasWeakReference)
                return GetWeakReference(item);
            return ServiceProvider.WeakReferenceFactory(item);
        }

        [Pure]
        internal static bool HasMemberFlag(this MemberFlags es, MemberFlags value)
        {
            return (es & value) == value;
        }

        [Pure]
        internal static bool HasState(this EntityState es, EntityState value)
        {
            return (es & value) == value;
        }

        internal static IDictionary<string, IList<object>> MergeDictionaries(IList<IDictionary<string, IList<object>>> dictionaries)
        {
            if (dictionaries.Count == 0)
                return new Dictionary<string, IList<object>>();
            if (dictionaries.Count == 1)
                return dictionaries[0];
            var result = new Dictionary<string, IList<object>>();
            for (int index = 0; index < dictionaries.Count; index++)
            {
                var dictionary = dictionaries[index];
                foreach (var keyPair in dictionary)
                {
                    IList<object> list;
                    if (!result.TryGetValue(keyPair.Key, out list))
                    {
                        list = new List<object>();
                        result[keyPair.Key] = list;
                    }

                    IList<object> value;
                    if (dictionary.TryGetValue(keyPair.Key, out value))
                        list.AddRange(value);
                }
            }
            return result;
        }

        internal static bool HasFlagEx(this LoadMode mode,
            LoadMode value)
        {
            return (mode & value) == value;
        }

        internal static bool HasFlagEx(this HandleMode handleMode, HandleMode value)
        {
            return (handleMode & value) == value;
        }

        internal static bool HasFlagEx(this ObservationMode handleType, ObservationMode value)
        {
            return (handleType & value) == value;
        }

        internal static bool HasFlagEx(this byte b, byte value)
        {
            return (b & value) == value;
        }

        private static object AsMethodBindingDelegateObject<T>(
            this Func<IIocContainer, IList<IIocParameter>, T> methodBinding, IIocContainer container,
            IList<IIocParameter> parameters)
        {
            return methodBinding(container, parameters);
        }

        private static void AsContinuationAction<T>(this TaskCompletionSource<T> tcs, IOperationResult result)
        {
            if (result.IsCanceled)
                tcs.SetCanceled();
            else if (result.IsFaulted)
                tcs.SetException(result.Exception);
            else
                tcs.SetResult((T)result.Result);
        }

        private static void AsContinuationAction<T>(this TaskCompletionSource<T> tcs, IOperationResult<T> genericResult)
        {
            AsContinuationAction(tcs, result: genericResult);
        }

        #endregion
    }
}
