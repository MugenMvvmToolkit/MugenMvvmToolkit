#region Copyright

// ****************************************************************************
// <copyright file="ToolkitExtensions.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
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
    /// <summary>
    ///     Represents the extensions method for mvvm application.
    /// </summary>
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

            public int Count
            {
                get { return Context.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

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

        /// <summary>
        /// Gets or sets the default duration of <see cref="ToastDuration.Short"/>.
        /// </summary>
        public static float ShortDuration { get; set; }

        /// <summary>
        /// Gets or sets the default duration of <see cref="ToastDuration.Long"/>.
        /// </summary>
        public static float LongDuration { get; set; }

        #endregion

        #region Ioc adapter extensions

        /// <summary>
        ///     Gets a root container.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IIocContainer" />
        /// </returns>
        [Pure]
        public static IIocContainer GetRoot([NotNull] this IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            while (iocContainer.Parent != null)
                iocContainer = iocContainer.Parent;
            return iocContainer;
        }

        /// <summary>
        ///     Gets an instance of the specified service.
        /// </summary>
        /// <typeparam name="T">The specified service type.</typeparam>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        /// <param name="parameters">The specified parameters.</param>
        /// <param name="name">The specified binding name.</param>
        /// <returns>An instance of T.</returns>
        [Pure]
        public static T Get<T>([NotNull] this IIocContainer iocContainer, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            return (T)iocContainer.Get(typeof(T), name, parameters);
        }

        /// <summary>
        ///    Tries to get an instance of the specified service.
        /// </summary>
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

        /// <summary>
        ///    Tries to get an instance of the specified service.
        /// </summary>
        [Pure]
        public static bool TryGet([NotNull] this IIocContainer iocContainer, [NotNull] Type serviceType, out object service, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            Should.NotBeNull(serviceType, "serviceType");
            if (iocContainer.CanResolve(serviceType))
            {
                service = iocContainer.Get(serviceType, name, parameters);
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        ///     Gets all instances of the specified service.
        /// </summary>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        [Pure]
        public static IEnumerable<T> GetAll<T>([NotNull] this IIocContainer iocContainer, string name = null,
            params IIocParameter[] parameters)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            return iocContainer.GetAll(typeof(T), name, parameters).Cast<T>();
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified type.
        /// </summary>
        /// <typeparam name="T">The specified service type.</typeparam>
        /// <typeparam name="TTypeTo">The specified to type</typeparam>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        /// <param name="lifecycle">
        ///     Specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="name">The specified binding name.</param>
        public static void Bind<T, TTypeTo>([NotNull] this IIocContainer iocContainer, DependencyLifecycle lifecycle,
            string name = null)
            where TTypeTo : T
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            iocContainer.Bind(typeof(T), typeof(TTypeTo), lifecycle, name);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        /// <param name="instance">The specified constant value.</param>
        /// <param name="name">The specified binding name.</param>
        public static void BindToConstant<T>([NotNull] this IIocContainer iocContainer, T instance, string name = null)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            iocContainer.BindToConstant(typeof(T), instance, name);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified method.
        /// </summary>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        /// <param name="methodBindingDelegate">The specified factory delegate.</param>
        /// <param name="lifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="name">The specified binding name.</param>
        public static void BindToMethod<T>([NotNull] this IIocContainer iocContainer,
            [NotNull] Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            Should.NotBeNull(methodBindingDelegate, "methodBindingDelegate");
            iocContainer.BindToMethod(typeof(T), methodBindingDelegate.AsMethodBindingDelegateObject, lifecycle, name);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void BindToBindingInfo<T>(this IIocContainer iocContainer, BindingInfo<T> binding)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            if (binding.IsEmpty)
                return;
            if (iocContainer.CanResolve<T>(binding.Name))
                Tracer.Info("The binding with type {0} already exists.", typeof(T));
            else
                binding.SetBinding(iocContainer);
        }

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <typeparam name="T">The specified service type.</typeparam>
        /// <param name="iocContainer">
        ///     The specified <see cref="IIocContainer" />.
        /// </param>
        public static void Unbind<T>([NotNull] this IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            iocContainer.Unbind(typeof(T));
        }

        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="iocContainer">The specified <see cref="IIocContainer" />.</param>
        /// <param name="name">The specified binding name.</param>
        /// <returns>
        ///     <c>True</c> if the specified service has been resolved; otherwise, <c>false</c>.
        /// </returns>
        [Pure]
        public static bool CanResolve<T>([NotNull] this IIocContainer iocContainer, string name = null)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            return iocContainer.CanResolve(typeof(T), name);
        }

        /// <summary>
        ///     Gets an instance of the specified service.
        /// </summary>
        /// <typeparam name="T">The specified service type.</typeparam>
        /// <param name="serviceProvider">
        ///     The specified <see cref="IServiceProvider" />.
        /// </param>
        /// <returns>An instance of T.</returns>
        [Pure]
        public static T GetService<T>([NotNull] this IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, "serviceProvider");
            var service = serviceProvider.GetService(typeof(T));
            if (service == null)
                return default(T);
            return (T)service;
        }

        #endregion

        #region Exception extension

        /// <summary>
        ///     Flatten the exception and inner exception data.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="includeStackTrace">True to include stack trace at end</param>
        /// <returns>String with Message and all InnerException messages appended together</returns>
        [Pure]
        public static string Flatten([NotNull] this Exception exception, bool includeStackTrace = false)
        {
            return exception.Flatten(string.Empty, includeStackTrace);
        }

        /// <summary>
        ///     Flatten the exception and inner exception data.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Any string prefix to add</param>
        /// <param name="includeStackTrace">True to include stack trace at end</param>
        /// <returns>String with Message and all InnerException messages appended together</returns>
        [Pure]
        public static string Flatten([NotNull] this Exception exception, string message, bool includeStackTrace = false)
        {
            Should.NotBeNull(exception, "exception");
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

        /// <summary>
        ///     Returns a value indicating whether the specified <see cref="T:System.String" /> object occurs within this string.
        ///     Not throws an exception.
        /// </summary>
        /// <param name="source">The specified source string.</param>
        /// <param name="value">The string to seek. </param>
        /// <param name="stringComparison">The specified <see cref="StringComparison" />.</param>
        /// <returns>
        ///     true if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the
        ///     empty string (""); otherwise, false.
        /// </returns>
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

        /// <summary>
        ///     Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed successfully with the specified result.
        /// </summary>
        /// <returns>
        ///     The successfully completed task.
        /// </returns>
        /// <param name="result">The result to store into the completed task.</param>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        [Pure]
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>
        ///     Tries to inline task callback method.
        /// </summary>
        public static Task TryExecuteSynchronously<T>([NotNull] this Task<T> task, [NotNull] Action<Task<T>> action,
            CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
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
                return CreateExceptionTask<bool>(e, true);
            }
            catch (Exception exception)
            {
                return CreateExceptionTask<bool>(exception, false);
            }
        }

        /// <summary>
        ///     Tries to inline task callback method.
        /// </summary>
        public static Task<TResult> TryExecuteSynchronously<T, TResult>([NotNull] this Task<T> task,
            [NotNull] Func<Task<T>, TResult> action, CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
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

        /// <summary>
        ///     Tries to inline task callback method.
        /// </summary>
        public static Task TryExecuteSynchronously([NotNull] this Task task, [NotNull] Action<Task> action,
            CancellationToken token = default(CancellationToken))
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
            if (!task.IsCompleted)
                return task.ContinueWith(action, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            try
            {
                action(task);
                return task;
            }
            catch (OperationCanceledException e)
            {
                return CreateExceptionTask<bool>(e, true);
            }
            catch (Exception exception)
            {
                return CreateExceptionTask<bool>(exception, false);
            }
        }

        /// <summary>
        ///     Uses the <see cref="ITaskExceptionHandler" /> to notify abount an error.
        /// </summary>
        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(viewModel, "viewModel");
            return task.WithTaskExceptionHandler(viewModel, viewModel.GetIocContainer(true, false));
        }

        /// <summary>
        ///     Uses the <see cref="ITaskExceptionHandler" /> to notify abount an error.
        /// </summary>
        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] object sender,
            IIocContainer iocContainer = null)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(sender, "sender");
            if (task.IsCompleted)
                TryHandleTaskException(task, sender, iocContainer);
            else
                task.TryExecuteSynchronously(t => TryHandleTaskException(t, sender, iocContainer));
            return task;
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

        /// <summary>
        ///     Moves up the item in the specified <see cref="IList" />.
        /// </summary>
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

        /// <summary>
        ///     Moves down the item in the specified <see cref="IList" />.
        /// </summary>
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

        /// <summary>
        ///     Determines whether the collection can move up the item.
        /// </summary>
        public static bool CanMoveUpItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            return itemsSource != null && itemsSource.IndexOf(item) > 0;
        }

        /// <summary>
        ///     Determines whether the collection can move down the item.
        /// </summary>
        public static bool CanMoveDownItem([CanBeNull] this IList itemsSource, [CanBeNull] object item)
        {
            if (itemsSource == null)
                return false;
            var indexOf = itemsSource.IndexOf(item);
            return indexOf >= 0 && indexOf < itemsSource.Count - 1;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IEnumerable"/>.
        /// </summary>
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
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            return -1;
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence.
        /// </summary>
        public static object ElementAtIndex([NotNull]this IEnumerable enumerable, int index)
        {
            Should.NotBeNull(enumerable, "enumerable");
            var list = enumerable as IList;
            if (list != null)
                return list[index];
            return enumerable.OfType<object>().ElementAt(index);
        }

        /// <summary>
        ///     Creates an array from a <see cref="IList{T}" />.
        /// </summary>
        /// <returns>
        ///     An array that contains the elements from the input sequence.
        /// </returns>
        /// <param name="list">An <see cref="IList{T}" /> to create an array from.</param>
        public static T[] ToArrayEx<T>([NotNull] this IList<T> list)
        {
            Should.NotBeNull(list, "list");
            if (list.Count == 0)
                return Empty.Array<T>();
            var array = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }

        /// <summary>
        ///     Creates an array from a <see cref="IList{T}" />.
        /// </summary>
        /// <returns>
        ///     An array that contains the elements from the input sequence.
        /// </returns>
        /// <param name="list">An <see cref="IList{T}" /> to create an array from.</param>
        /// <param name="selector">
        ///     A transform function to apply to each source element; the second parameter of the function
        ///     represents the index of the source element.
        /// </param>
        public static TResult[] ToArrayEx<T, TResult>([NotNull] this IList<T> list,
            [NotNull] Func<T, TResult> selector)
        {
            Should.NotBeNull(list, "list");
            Should.NotBeNull(selector, "selector");
            if (list.Count == 0)
                return Empty.Array<TResult>();
            var array = new TResult[list.Count];
            for (int i = 0; i < list.Count; i++)
                array[i] = selector(list[i]);
            return array;
        }

        /// <summary>
        ///     Creates an array from a <see cref="IList{T}" />.
        /// </summary>
        /// <returns>
        ///     An array that contains the elements from the input sequence.
        /// </returns>
        /// <param name="collection">An <see cref="ICollection{T}" /> to create an array from.</param>
        /// <param name="selector">
        ///     A transform function to apply to each source element; the second parameter of the function
        ///     represents the index of the source element.
        /// </param>
        public static TResult[] ToArrayEx<T, TResult>([NotNull] this ICollection<T> collection,
            [NotNull] Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, "collection");
            int count = collection.Count;
            if (count == 0)
                return Empty.Array<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (T item in collection)
            {
                array[count] = selector(item);
                count++;
            }
            return array;
        }

        /// <summary>
        ///     Creates an array from a <see cref="IList{T}" />.
        /// </summary>
        /// <returns>
        ///     An array that contains the elements from the input sequence.
        /// </returns>
        /// <param name="collection">An <see cref="ICollection{T}" /> to create an array from.</param>
        public static T[] ToArrayEx<T>([NotNull] this ICollection<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            int count = collection.Count;
            if (count == 0)
                return Empty.Array<T>();
            var array = new T[count];
            count = 0;
            foreach (T item in collection)
            {
                array[count] = item;
                count++;
            }
            return array;
        }

        /// <summary>
        ///     Adds an item to the collection if the item is not null.
        /// </summary>
        public static void AddIfNotNull<T>([NotNull] this ICollection<T> collection, T item)
        {
            Should.NotBeNull(collection, "collection");
            if (item != null)
                collection.Add(item);
        }

        /// <summary>
        ///     Returns <c>true</c> if and only if <paramref name="collection" /> is empty (has no elements) or null.
        /// </summary>
        [Pure]
        public static bool IsNullOrEmpty<T>([CanBeNull] this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        ///     Returns <c>true</c> if and only if <paramref name="enumerable" /> is empty (has no elements) or null.
        /// </summary>
        [Pure]
        public static bool IsNullOrEmpty([CanBeNull] this IEnumerable enumerable)
        {
            return enumerable == null || enumerable.IsEmpty();
        }

        /// <summary>
        ///     Returns <c>true</c> if and only if <paramref name="enumerable" /> is empty (has no elements).
        /// </summary>
        [Pure]
        public static bool IsEmpty([NotNull] this IEnumerable enumerable)
        {
            Should.NotBeNull(enumerable, "enumerable");
            var collection = enumerable as ICollection;
            if (collection != null)
                return collection.Count == 0;
            IEnumerator enumerator = enumerable.GetEnumerator();
            try
            {
                return !enumerator.MoveNext();
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        /// <summary>
        ///     Returns the number of elements in a sequence.
        /// </summary>
        /// <returns>
        ///     The number of elements in the input sequence.
        /// </returns>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        [Pure]
        public static int Count([CanBeNull] this IEnumerable source)
        {
            if (source == null)
                return 0;
            var collection = source as ICollection;
            if (collection != null)
                return collection.Count;
            int num = 0;
            var enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    checked { ++num; }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            return num;
        }

        /// <summary>
        ///     Enumerates a collection and executes a predicate against each item
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <param name="action">Action to execute on each element</param>
        public static void ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(action, "action");
            var array = collection as IList<T>;
            if (array != null)
            {
                for (int i = 0; i < array.Count; i++)
                    action(array[i]);
            }
            else
            {
                foreach (T o in collection)
                    action(o);
            }
        }

        /// <summary>
        ///     Adds a range of IEnumerable collection to an existing Collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <param name="items">Items to add</param>
        public static void AddRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(items, "items");
            var list = collection as List<T>;
            if (list != null)
            {
                list.AddRange(items);
                return;
            }
            var notifiableCollection = collection as SynchronizedNotifiableCollection<T>;
            if (notifiableCollection != null)
            {
                notifiableCollection.AddRange(items);
                return;
            }
            foreach (T item in items)
                collection.Add(item);
        }

        /// <summary>
        ///     Adds all items from the source into the target dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void AddRange<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> target,
            [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(source, "source");
            foreach (var keyValuePair in source)
                target[keyValuePair.Key] = keyValuePair.Value;
        }

        /// <summary>
        ///     Removes a range of IEnumerable collection to an existing Collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <param name="items">Items to remove</param>
        public static void RemoveRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(items, "items");
            var notifiableCollection = collection as SynchronizedNotifiableCollection<T>;
            if (notifiableCollection != null)
            {
                notifiableCollection.RemoveRange(items);
                return;
            }
            var list = items.ToList();
            for (int index = 0; index < list.Count; index++)
                collection.Remove(list[index]);
        }

        /// <summary>
        ///     Converts a collection to the <see cref="SynchronizedNotifiableCollection{T}" /> collection.
        /// </summary>
        /// <typeparam name="T">The type of collection.</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <param name="threadManager">The specified <see cref="IThreadManager" />.</param>
        /// <returns>An instance of <see cref="SynchronizedNotifiableCollection{T}" />.</returns>
        [Pure]
        public static SynchronizedNotifiableCollection<T> ToSynchronizedCollection<T>(
            [NotNull] this IEnumerable<T> collection, [NotNull] IThreadManager threadManager = null)
        {
            Should.NotBeNull(collection, "collection");
            return collection as SynchronizedNotifiableCollection<T> ??
                   new SynchronizedNotifiableCollection<T>(collection, threadManager);
        }

        /// <summary>
        ///     Converts a collection to the <see cref="FilterableNotifiableCollection{T}" /> collection.
        /// </summary>
        /// <typeparam name="T">The type of collection.</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <param name="threadManager">The specified <see cref="IThreadManager" />.</param>
        /// <returns>An instance of <see cref="FilterableNotifiableCollection{T}" />.</returns>
        public static FilterableNotifiableCollection<T> ToFilterableCollection<T>(
            [NotNull] this IEnumerable<T> collection, IThreadManager threadManager = null)
        {
            Should.NotBeNull(collection, "collection");
            return collection as FilterableNotifiableCollection<T> ??
                   new FilterableNotifiableCollection<T>(collection, threadManager);
        }

        #endregion

        #region Callbacks

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation ContinueWith([NotNull] this IAsyncOperation operation, [NotNull] Action<IOperationResult> continuationAction)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationAction, "continuationAction");
            return operation.ContinueWith(continuationAction: new DelegateContinuation<object, object, object>(continuationAction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation<TResult> ContinueWith<TResult>([NotNull] this IAsyncOperation operation,
            [NotNull] Func<IOperationResult, TResult> continuationFunction)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationFunction, "continuationFunction");
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<object, TResult, object>(continuationFunction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation ContinueWith<TResult>([NotNull] this IAsyncOperation<TResult> operation, [NotNull] Action<IOperationResult<TResult>> continuationActionGeneric)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationActionGeneric, "continuationActionGeneric");
            return operation.ContinueWith(continuationAction: new DelegateContinuation<TResult, object, object>(continuationActionGeneric));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>([NotNull] this IAsyncOperation<TResult> operation,
            [NotNull] Func<IOperationResult<TResult>, TNewResult> continuationFunctionGeneric)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationFunctionGeneric, "continuationFunctionGeneric");
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<TResult, TNewResult, object>(continuationFunctionGeneric));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation ContinueWith<TSource, TResult>([NotNull] this IAsyncOperation<TResult> operation, [NotNull] Action<TSource, IOperationResult<TResult>> continuationAction)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationAction, "continuationAction");
            return operation.ContinueWith(continuationAction: new DelegateContinuation<TResult, object, TSource>(continuationAction));
        }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        public static IAsyncOperation<TNewResult> ContinueWith<TSource, TResult, TNewResult>([NotNull] this IAsyncOperation<TResult> operation,
             [NotNull] Func<TSource, IOperationResult<TResult>, TNewResult> continuationFunction)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(continuationFunction, "continuationFunction");
            return operation.ContinueWith(continuationFunction: new DelegateContinuation<TResult, TNewResult, TSource>(continuationFunction));
        }

        /// <summary>
        /// Converts an instance of <see cref="IAsyncOperation{TResult}"/> to an instance of <see cref="Task{TResult}"/>
        /// </summary>
        public static Task<T> AsTask<T>([NotNull] this IAsyncOperation<T> operation)
        {
            Should.NotBeNull(operation, "operation");
            var tcs = new TaskCompletionSource<T>();
            operation.ContinueWith(continuationActionGeneric: tcs.AsContinuationAction);
            return tcs.Task;
        }

        /// <summary>
        /// Converts an instance of <see cref="IAsyncOperation{TResult}"/> to an instance of <see cref="Task{TResult}"/>
        /// </summary>
        public static Task AsTask([NotNull] this IAsyncOperation operation)
        {
            Should.NotBeNull(operation, "operation");
            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(tcs.AsContinuationAction);
            return tcs.Task;
        }

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter" />.
        /// </summary>
        public static IAsyncOperationAwaiter GetAwaiter([NotNull] this IAsyncOperation operation)
        {
            Should.NotBeNull(operation, "operation");
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, DataContext.Empty);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter{TResult}" />.
        /// </summary>
        public static IAsyncOperationAwaiter<TResult> GetAwaiter<TResult>([NotNull] this IAsyncOperation<TResult> operation)
        {
            Should.NotBeNull(operation, "operation");
            return ServiceProvider.OperationCallbackFactory.CreateAwaiter(operation, DataContext.Empty);
        }

        #endregion

        #region Extensions

        /// <summary>
        ///     Registers the specified validator.
        /// </summary>
        public static void Register<T>([NotNull] this IValidatorProvider validatorProvider)
            where T : IValidator
        {
            Should.NotBeNull(validatorProvider, "validatorProvider");
            validatorProvider.Register(typeof(T));
        }

        /// <summary>
        ///     Unregisters the specified validator.
        /// </summary>
        public static void Unregister<T>([NotNull] this IValidatorProvider validatorProvider)
            where T : IValidator
        {
            Should.NotBeNull(validatorProvider, "validatorProvider");
            validatorProvider.Unregister(typeof(T));
        }

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="observable">The specified <see cref="IObservable"/></param>
        /// <param name="action">The handler to subscribe for event publication.</param>
        /// <param name="weakSubscribtion">If <c>true</c> use weak delegate to subscribe</param>
        [CanBeNull]
        public static ISubscriber Subscribe<TMessage>([NotNull] this IObservable observable, Action<object, TMessage> action, bool weakSubscribtion = true)
        {
            Should.NotBeNull(observable, "observable");
            ISubscriber subscriber;
            if (weakSubscribtion && action.Target != null)
            {
                Should.BeSupported(!action.Target.GetType().IsAnonymousClass(),
                    "The anonymous delegate cannot be converted to weak delegate.");
                subscriber = new WeakActionSubscriber<TMessage>(action.Target, action.GetMethodInfo());
            }
            else
                subscriber = new ActionSubscriber<TMessage>(action);
            return observable.Subscribe(subscriber) ? subscriber : null;
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="observable">The specified <see cref="IObservable"/></param>
        /// <param name="action">The handler to unsubscribe from event publication.</param>
        public static bool Unsubscribe<TMessage>([NotNull] this IObservable observable, Action<object, TMessage> action)
        {
            Should.NotBeNull(observable, "observable");
            return observable.Unsubscribe(new ActionSubscriber<TMessage>(action));
        }

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="observable">The specified <see cref="IObservable"/></param>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        /// <param name="context">The specified context, if any.</param>
        [CanBeNull]
        public static ISubscriber Subscribe([NotNull] this IObservable observable, [NotNull] object instance, IDataContext context = null)
        {
            Should.NotBeNull(observable, "observable");
            var converter = ServiceProvider.ObjectToSubscriberConverter;
            if (converter == null)
                return null;
            var subscriber = converter(instance, context);
            if (subscriber == null)
                return null;
            return observable.Subscribe(subscriber) ? subscriber : null;
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="observable">The specified <see cref="IObservable"/></param>
        /// <param name="instance">The instance to unsubscribe.</param>
        /// <param name="context">The specified context, if any.</param>
        public static bool Unsubscribe([NotNull] this IObservable observable, [NotNull]object instance, IDataContext context = null)
        {
            Should.NotBeNull(observable, "observable");
            var converter = ServiceProvider.ObjectToSubscriberConverter;
            if (converter == null)
                return false;
            var subscriber = converter(instance, context);
            if (subscriber == null)
                return false;
            return observable.Unsubscribe(subscriber);
        }

        /// <summary>
        ///     Gets or creates an instance of <see cref="WeakReference" /> for the specified item.
        /// </summary>
        public static WeakReference GetWeakReference(object item)
        {
            var hasWeak = item as IHasWeakReference;
            if (hasWeak == null)
                return ServiceProvider.WeakReferenceFactory(item, true);
            return hasWeak.WeakReference;
        }

        /// <summary>
        ///     Creates an instance an instance of <see cref="WeakReference" /> for the specified item.
        /// </summary>
        public static WeakReference GetWeakReferenceOrDefault(object item, WeakReference defaultValue, bool checkHasWeakReference)
        {
            if (item == null)
                return defaultValue;
            if (checkHasWeakReference)
                return GetWeakReference(item);
            return ServiceProvider.WeakReferenceFactory(item, true);
        }

        /// <returns>
        ///     Gets the underlying view object.
        /// </returns>
        public static TView GetUnderlyingView<TView>([CanBeNull] this IView view)
        {
            return GetUnderlyingView<TView>(viewObj: view);
        }

        /// <returns>
        ///     Gets the underlying view object.
        /// </returns>
        public static TView GetUnderlyingView<TView>([CanBeNull]object viewObj)
        {
            var wrapper = viewObj as IViewWrapper;
            if (wrapper == null)
                return (TView)viewObj;
            return (TView)wrapper.View;
        }

        /// <summary>
        ///     Creates an instance of <see cref="IValidatorAggregator" /> using specified instance.
        /// </summary>
        public static IValidatorAggregator GetValidatorAggregator([NotNull] this IValidatorProvider validatorProvider,
            [NotNull] object instanceToValidate)
        {
            Should.NotBeNull(validatorProvider, "validatorProvider");
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            var aggregator = validatorProvider.GetValidatorAggregator();
            aggregator.AddInstance(instanceToValidate);
            return aggregator;
        }

        /// <summary>
        ///     Sets errors for a property using the <see cref="IValidatorAggregator.Validator"/>.
        /// </summary>
        /// <param name="aggregator">The specified validator aggregator.</param>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public static void SetValidatorErrors<T>([NotNull] this IValidatorAggregator aggregator, Expression<Func<T>> propertyExpresssion, params object[] errors)
        {
            aggregator.Validator.SetErrors(propertyExpresssion, errors);
        }

        /// <summary>
        ///     Sets errors for a property using the <see cref="IValidatorAggregator.Validator"/>.
        /// </summary>
        /// <param name="aggregator">The specified validator aggregator.</param>
        /// <param name="property">The property name</param>
        /// <param name="errors">The collection of errors</param>
        public static void SetValidatorErrors([NotNull] this IValidatorAggregator aggregator, string property, params object[] errors)
        {
            aggregator.Validator.SetErrors(property, errors);
        }

        /// <summary>
        ///     Adds the specified validator.
        /// </summary>
        public static TValidator AddValidator<TValidator>([NotNull] this IValidatorAggregator aggregator,
            [NotNull] object instanceToValidate) where TValidator : IValidator
        {
            Should.NotBeNull(aggregator, "aggregator");
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            var validator = ServiceProvider.IocContainer == null
                ? Activator.CreateInstance<TValidator>()
                : ServiceProvider.IocContainer.Get<TValidator>();
            validator.Initialize(aggregator.CreateContext(instanceToValidate));
            aggregator.AddValidator(validator);
            return validator;
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        public static void ClearErrors<TModel>([NotNull] this IValidator validator, Expression<Func<TModel, object>> propertyExpresssion)
        {
            validator.ClearErrors(GetMemberName(propertyExpresssion));
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        public static void ClearErrors<TValue>([NotNull] this IValidator validator,
            [NotNull] Expression<Func<TValue>> propertyExpresssion)
        {
            Should.NotBeNull(validator, "validator");
            validator.ClearErrors(propertyExpresssion.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Notifies listener about an event.
        /// </summary>
        /// <param name="eventPublisher">The specified listener to notify.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        /// <param name="mode">The execution mode.</param>
        public static void Publish([NotNull] this IEventPublisher eventPublisher, [NotNull] object sender, [NotNull] object message, ExecutionMode mode)
        {
            Should.NotBeNull(eventPublisher, "eventPublisher");
            ServiceProvider.ThreadManager.Invoke(mode, eventPublisher, sender, message, (publisher, o, arg3) => publisher.Publish(o, arg3));
        }

        /// <summary>
        ///     Writes the stream contents to a byte array.
        /// </summary>
        public static byte[] ToArray([NotNull] this Stream stream)
        {
            return stream.ToArray(0);
        }

        /// <summary>
        ///     Writes the stream contents to a byte array.
        /// </summary>
        public static byte[] ToArray([NotNull] this Stream stream, int? position)
        {
            Should.NotBeNull(stream, "stream");
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

        /// <summary>
        ///     Serializes data to string.
        /// </summary>
        public static string SerializeToBase64String(this ISerializer serializer, object item)
        {
            Should.NotBeNull(serializer, "serializer");
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

        /// <summary>
        ///     Deserializes data using string.
        /// </summary>
        public static T Deserialize<T>(this ISerializer serializer, string stringData)
        {
            Should.NotBeNull(serializer, "serializer");
            using (var ms = new MemoryStream(Convert.FromBase64String(stringData)))
                return (T)serializer.Deserialize(ms);
        }

        /// <summary>
        /// Suspends the current thread for a specified time.
        /// </summary>
        public static void Sleep(int millisecondsTimeout)
        {
            // Set is never called, so we wait always until the timeout occurs
            Sleeper.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// Suspends the current thread for a specified time.
        /// </summary>
        public static void Sleep(TimeSpan timeout)
        {
            Sleep((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static Task ShowAsync(this IToastPresenter toastPresenter, object content, ToastDuration duration, ToastPosition position = ToastPosition.Bottom, IDataContext context = null)
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
                    throw new ArgumentOutOfRangeException("duration");
            }
            return toastPresenter.ShowAsync(content, floatDuration, position, context);
        }

        /// <summary>
        /// Loads the specified modules using module context;
        /// </summary>
        public static IList<IModule> LoadModules([NotNull] this IModuleContext context, [NotNull] IEnumerable<IModule> modules)
        {
            Should.NotBeNull(context, "context");
            Should.NotBeNull(modules, "modules");
            var list = new List<IModule>();
            foreach (var module in modules.OrderByDescending(module => module.Priority))
            {
                if (context.Context.GetData<object>(module.GetType().FullName) != null)
                    continue;
                var load = module.Load(context);
                if (load)
                {
                    list.Add(module);
                    module.TraceModule(true);
                }
            }
            return list;
        }

        internal static void TraceModule(this IModule module, bool load)
        {
            if (Tracer.TraceInformation)
                Tracer.Info("The module '{0}' was {1}loaded.", module.GetType(), load ? null : "un");
        }

        /// <summary>
        ///     Converts the <see cref="IDataContext" /> to the <see cref="IDictionary{TKey,TValue}" />
        /// </summary>
        public static IDictionary<object, object> ToDictionary([CanBeNull] this IDataContext context)
        {
            if (context == null)
                return new Dictionary<object, object>();
            return new DataContextDictionaryWrapper(context);
        }

        /// <summary>
        ///     Converts the <see cref="IDictionary{TKey,TValue}" /> to the <see cref="IDataContext" />
        /// </summary>
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

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        public static void AddIfNotNull<T>([NotNull] this IDataContext context, [NotNull] DataConstant<T> data, T value)
            where T : class
        {
            Should.NotBeNull(context, "context");
            Should.NotBeNull(data, "data");
            if (value != null)
                context.Add(data, value);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        public static T GetData<T>([NotNull] this IDataContext context, [NotNull] DataConstant<T> dataConstant,
            bool throwIfNotFound) where T : class
        {
            Should.NotBeNull(context, "context");
            Should.NotBeNull(dataConstant, "dataConstant");
            T result;
            if (!context.TryGetData(dataConstant, out result) && throwIfNotFound)
                throw ExceptionManager.DataConstantNotFound(dataConstant);
            return result;
        }

        /// <summary>
        ///     Checks whether the member names are equal.
        /// </summary>
        /// <param name="memberName">The specified member name.</param>
        /// <param name="getMember">The expression to get member.</param>
        /// <returns>If true member names is equal, otherwise false.</returns>
        [Pure]
        public static bool MemberNameEqual<T>(string memberName, [NotNull] Expression<Func<T, object>> getMember)
        {
            return getMember.GetMemberInfo().Name.Equals(memberName, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Checks whether the properties are equal.
        /// </summary>
        /// <param name="args">The specified property changed args.</param>
        /// <param name="getProperty">The expression to get property.</param>
        /// <returns>If true property is equal, otherwise false.</returns>
        [Pure]
        public static bool PropertyNameEqual<T>([NotNull] this PropertyChangedEventArgs args, [NotNull] Expression<Func<T, object>> getProperty)
        {
            Should.NotBeNull(args, "args");
            return MemberNameEqual(args.PropertyName, getProperty);
        }

        /// <summary>
        ///     Checks whether the properties are equal.
        /// </summary>
        /// <param name="args">The specified property changed args.</param>
        /// <param name="item">The specified model.</param>
        /// <param name="getProperty">The expression to get property.</param>
        /// <returns>If true property is equal, otherwise false.</returns>
        [Pure]
        public static bool PropertyNameEqual<T, TValue>([NotNull] this PropertyChangedEventArgs args, T item, [NotNull] Expression<Func<T, TValue>> getProperty)
        {
            return PropertyNameEqual(args.PropertyName, getProperty.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Checks whether the properties are equal.
        /// </summary>
        public static bool PropertyNameEqual(string changedProperty, string listenedProperty, bool emptyListenedPropertyResult = false)
        {
            if (string.IsNullOrEmpty(changedProperty) ||
                changedProperty.Equals(listenedProperty, StringComparison.Ordinal))
                return true;
            if (string.IsNullOrEmpty(listenedProperty))
                return emptyListenedPropertyResult;

            if (listenedProperty.StartsWith("[", StringComparison.Ordinal) &&
                (changedProperty == "Item[]" || changedProperty == "Item" + listenedProperty))
                return true;
            return false;
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        public static string GetMemberName([NotNull] LambdaExpression expression)
        {
            return expression.GetMemberInfo().Name;
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <typeparam name="T">The type of model.</typeparam>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        public static string GetMemberName<T>([NotNull] Expression<Func<T>> expression)
        {
            return expression.GetMemberInfo().Name;
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <typeparam name="T">The type of model.</typeparam>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        public static string GetMemberName<T>([NotNull] Expression<Func<T, object>> expression)
        {
            return expression.GetMemberInfo().Name;
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="item">The specified model.</param>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        public static string GetMemberName<T, TValue>([CanBeNull] T item, [NotNull] Expression<Func<T, TValue>> expression)
        {
            return GetMemberName(expression);
        }

        /// <summary>
        ///     Sets the specified state for all items in the collection.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="state">The state value.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static void SetStateForAll([NotNull] this ITrackingCollection collection, EntityState state,
            bool? validateState = null)
        {
            Should.NotBeNull(collection, "collection");
            foreach (var item in collection)
                collection.UpdateState(item.Entity, state, validateState);
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        [Pure]
        public static bool HasChanges<T, TValue>(this IEntitySnapshot snapshot, T item, Expression<Func<T, TValue>> propertyExpression)
        {
            Should.NotBeNull(snapshot, "snapshot");
            Should.NotBeNull(item, "item");
            Should.NotBeNull(propertyExpression, "propertyExpression");
            return snapshot.HasChanges(item, propertyExpression.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Updates states of entities.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="predicate">The specified condition.</param>
        /// <param name="state">The specified state.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static void SetStateForAll([NotNull] this ITrackingCollection collection, [NotNull] Func<TrackingEntity<object>, bool> predicate, EntityState state, bool? validateState = null)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(predicate, "predicate");
            foreach (var item in collection)
            {
                if (predicate(item))
                    collection.UpdateState(item.Entity, state, validateState);
            }
        }

        /// <summary>
        ///     Updates state of entity.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <param name="newState">The state if item is new.</param>
        /// <param name="updateState">The state if item exist in collection.</param>
        public static bool UpdateState([NotNull] this ITrackingCollection collection, [NotNull] object item,
            EntityState newState, EntityState updateState, bool? validateState = null)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(item, "item");
            if (collection.Contains(item))
                return collection.UpdateState(item, updateState, validateState);
            return collection.UpdateState(item, newState, validateState);
        }

        /// <summary>
        ///     Updates state of entity.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static bool UpdateState([NotNull] this ITrackingCollection collection, [NotNull] IEntityStateEntry item,
            bool? validateState = null)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(item, "item");
            return collection.UpdateState(item.Entity, item.State, validateState);
        }

        /// <summary>
        ///     Updates states of entities.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="items">The range of values.</param>
        /// <param name="state">The specified state.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static void UpdateStates([NotNull] this ITrackingCollection collection, [NotNull] IEnumerable items,
            EntityState state, bool? validateState = null)
        {
            Should.NotBeNull(items, "items");
            foreach (object value in items)
                collection.UpdateState(value, state, validateState);
        }

        /// <summary>
        ///     Updates states of entities.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="items">Items to add</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static void UpdateStates([NotNull] this ITrackingCollection collection,
            [NotNull] IEnumerable<IEntityStateEntry> items, bool? validateState = null)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(items, "items");
            foreach (IEntityStateEntry entityStateEntry in items)
                collection.UpdateState(entityStateEntry, validateState);
        }

        /// <summary>
        ///     Removes the object from the tracking collection.
        /// </summary>
        /// <param name="collection">The specified collection</param>
        /// <param name="entity">Object to be detached.</param>
        public static bool Detach([NotNull] this ITrackingCollection collection, [NotNull] object entity)
        {
            Should.NotBeNull(collection, "collection");
            Should.NotBeNull(entity, "entity");
            return collection.UpdateState(entity, EntityState.Detached, false);
        }

        /// <summary>
        ///     Whether this Entity is unchanged.
        /// </summary>
        [Pure]
        public static bool IsUnchanged(this EntityState es)
        {
            return (es & EntityState.Unchanged) == EntityState.Unchanged;
        }

        /// <summary>
        ///     Whether this Entity has been added.
        /// </summary>
        [Pure]
        public static bool IsAdded(this EntityState es)
        {
            return (es & EntityState.Added) == EntityState.Added;
        }

        /// <summary>
        ///     Whether this Entity has been modified.
        /// </summary>
        [Pure]
        public static bool IsModified(this EntityState es)
        {
            return (es & EntityState.Modified) == EntityState.Modified;
        }

        /// <summary>
        ///     Whether this Entity has been detached (either not yet attached or removed via RemoveFromManager).
        /// </summary>
        [Pure]
        public static bool IsDetached(this EntityState es)
        {
            return (es & EntityState.Detached) == EntityState.Detached;
        }

        /// <summary>
        ///     Whether this Entity has been deleted (but the change has not yet been persisted to the data source).
        /// </summary>
        [Pure]
        public static bool IsDeleted(this EntityState es)
        {
            return (es & EntityState.Deleted) == EntityState.Deleted;
        }

        /// <summary>
        ///     Whether this Entity has been either added or modified.
        /// </summary>
        [Pure]
        public static bool IsAddedOrModified(this EntityState es)
        {
            return es.IsAdded() || es.IsModified();
        }

        /// <summary>
        ///     Whether this Entity has been either added or modified.
        /// </summary>
        [Pure]
        public static bool IsAddedOrModifiedOrUnchanged(this EntityState es)
        {
            return es.IsAddedOrModified() || es.IsUnchanged();
        }

        /// <summary>
        ///     Whether this Entity has been added, modified or deleted.
        /// </summary>
        [Pure]
        public static bool IsAddedOrModifiedOrDeleted(this EntityState es)
        {
            return es.IsAddedOrModified() || es.IsDeleted();
        }

        /// <summary>
        ///     Whether this Entity has been either deleted or detached.
        /// </summary>
        [Pure]
        public static bool IsDeletedOrDetached(this EntityState es)
        {
            return es.IsDeleted() || es.IsDetached();
        }

        /// <summary>
        ///     Whether this Entity has been either deleted or modified
        /// </summary>
        [Pure]
        public static bool IsDeletedOrModified(this EntityState es)
        {
            return es.IsDeleted() || es.IsModified();
        }

        /// <summary>
        /// Converts the data context to non-read only.
        /// </summary>
        [NotNull]
        public static IDataContext ToNonReadOnly([CanBeNull] this IDataContext context)
        {
            if (context == null)
                return new DataContext();
            if (context.IsReadOnly)
                return new DataContext(context);
            return context;
        }

        /// <summary>
        ///     Invokes an action using the specified execution mode.
        /// </summary>
        public static void Invoke(this IThreadManager threadManager, ExecutionMode mode, Action invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default (CancellationToken))
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

        /// <summary>
        ///     Invokes an action using the specified execution mode.
        /// </summary>
        public static void Invoke<TTarget, TArg>(this IThreadManager threadManager, ExecutionMode mode, TTarget target, TArg arg1, Action<TTarget, TArg> invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default (CancellationToken))
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

        /// <summary>
        ///     Invokes an action using the specified execution mode.
        /// </summary>
        public static void Invoke<TTarget, TArg1, TArg2>(this IThreadManager threadManager, ExecutionMode mode, TTarget target, TArg1 arg1, TArg2 arg2, Action<TTarget, TArg1, TArg2> invokeAction, OperationPriority priority = OperationPriority.Normal, CancellationToken token = default (CancellationToken))
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

        internal static void InvokeOnUiThreadAsync(Action action)
        {
            ServiceProvider.ThreadManager.InvokeOnUiThreadAsync(action);
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

        internal static bool HasFlagEx(this NotificationCollectionMode mode,
            NotificationCollectionMode value)
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