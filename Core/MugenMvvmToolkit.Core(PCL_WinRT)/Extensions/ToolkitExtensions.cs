#region Copyright
// ****************************************************************************
// <copyright file="ToolkitExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
        #region Constructors

        static ToolkitExtensions()
        {
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
        /// <param name="constValue">The specified constant value.</param>
        /// <param name="name">The specified binding name.</param>
        public static void BindToConstant<T>([NotNull] this IIocContainer iocContainer, T constValue, string name = null)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            iocContainer.BindToConstant(typeof(T), constValue, name);
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
            iocContainer.BindToMethod(typeof(T), (container, list) => methodBindingDelegate(container, list), lifecycle, name);
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
            CancellationToken? token = null)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
            if (!task.IsCompleted)
                return task.ContinueWith(action, token.GetValueOrDefault(CancellationToken.None),
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
            [NotNull] Func<Task<T>, TResult> action,
            CancellationToken? token = null)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
            if (!task.IsCompleted)
                return task.ContinueWith(action, token.GetValueOrDefault(CancellationToken.None),
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
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
            CancellationToken? token = null)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(action, "action");
            if (!task.IsCompleted)
                return task.ContinueWith(action, token.GetValueOrDefault(CancellationToken.None),
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
        /// Uses the <see cref="ITaskExceptionHandler"/> to notify abount an error.
        /// </summary>
        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(viewModel, "viewModel");
            return task.WithTaskExceptionHandler(viewModel, viewModel.GetIocContainer(true));
        }

        /// <summary>
        /// Uses the <see cref="ITaskExceptionHandler"/> to notify abount an error.
        /// </summary>
        public static Task WithTaskExceptionHandler([NotNull] this Task task, [NotNull] object sender, IIocContainer iocContainer = null)
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
            var source = new TaskCompletionSource<T>();
            if (isCanceled)
                source.SetCanceled();
            else
                source.SetException(exception);
            return source.Task;
        }

        #endregion

        #region Collection extensions

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IEnumerable"/>.
        /// </summary>
        public static int IndexOf([CanBeNull]this IEnumerable enumerable, object value)
        {
            if (enumerable == null)
                return -1;
            var list = enumerable as IList;
            if (list != null)
                return list.IndexOf(value);
            var enumerator = enumerable.GetEnumerator();
            try
            {
                int index = 0;
                while (enumerator.MoveNext())
                {
                    if (Equals(enumerator.Current, value))
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
        public static T[] ToArrayFast<T>([NotNull] this IList<T> list)
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
        public static TResult[] ToArrayFast<T, TResult>([NotNull] this IList<T> list,
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
        public static TResult[] ToArrayFast<T, TResult>([NotNull] this ICollection<T> collection,
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
        public static T[] ToArrayFast<T>([NotNull] this ICollection<T> collection)
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
            operation.ContinueWith(result =>
            {
                if (result.IsCanceled)
                    tcs.SetCanceled();
                else if (result.IsFaulted)
                    tcs.SetException(result.Exception);
                else
                    tcs.SetResult(result.Result);
            });
            return tcs.Task;
        }

        /// <summary>
        /// Converts an instance of <see cref="IAsyncOperation{TResult}"/> to an instance of <see cref="Task{TResult}"/>
        /// </summary>
        public static Task AsTask([NotNull] this IAsyncOperation operation)
        {
            Should.NotBeNull(operation, "operation");
            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(result =>
            {
                if (result.IsCanceled)
                    tcs.SetCanceled();
                else if (result.IsFaulted)
                    tcs.SetException(result.Exception);
                else
                    tcs.SetResult(result.Result);
            });
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
        /// Gets or creates an instance of <see cref="WeakReference"/> for the specified item.
        /// </summary>
        public static WeakReference GetWeakReference(object item)
        {
            var hasWeak = item as IHasWeakReference;
            if (hasWeak == null)
                return ServiceProvider.WeakReferenceFactory(item, true);
            return hasWeak.WeakReference;
        }

        /// <returns>
        /// Gets the underlying view object.
        /// </returns>
        public static object GetUnderlyingView([CanBeNull]this IView view)
        {
            var wrapper = view as IViewWrapper;
            if (wrapper == null)
                return view;
            return wrapper.View;
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
        ///     Sets errors for a property
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="aggregator">The specified validator aggregator.</param>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public static void SetErrors<TModel>([NotNull] this IValidatorAggregator aggregator,
            [NotNull] Expression<Func<TModel, object>> propertyExpresssion, [CanBeNull] params object[] errors)
        {
            Should.NotBeNull(aggregator, "aggregator");
            aggregator.SetErrors(GetPropertyName(propertyExpresssion), errors);
        }

        /// <summary>
        ///     Notifies listener about an event.
        /// </summary>
        /// <param name="target">The specified listener to notify.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        public static void Publish([NotNull] this IObservable target, [NotNull] object sender, [NotNull] object message)
        {
            EventAggregator.Publish(target, sender, message);
        }

        /// <summary>
        /// Writes the stream contents to a byte array.
        /// </summary>
        public static byte[] ToArray([NotNull] this Stream stream, int position = 0)
        {
            Should.NotBeNull(stream, "stream");
            stream.Position = position;
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
            using (var mre = new ManualResetEvent(false))
                mre.WaitOne(millisecondsTimeout);
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
            Tracer.Info("The module '{0}' was {1}loaded.", module.GetType(), load ? null : "un");
        }

        /// <summary>
        ///     Converts the <see cref="IDataContext" /> to the <see cref="IDictionary{TKey,TValue}" />
        /// </summary>
        public static IDictionary<object, object> ToDictionary([CanBeNull] this IDataContext context)
        {
            if (context == null)
                context = DataContext.Empty;
            return context
                .ToList()
                .ToDictionary(value => (object)value.DataConstant, value => value.Value);
        }

        /// <summary>
        ///     Converts the <see cref="IDictionary{TKey,TValue}" /> to the <see cref="IDataContext" />
        /// </summary>
        public static IDataContext ToDataContext([CanBeNull] this IEnumerable<KeyValuePair<object, object>> dictionary)
        {
            if (dictionary == null)
                return new DataContext();
            return new DataContext(dictionary
                .Where(pair => pair.Key is DataConstant)
                .ToDictionary(pair => (DataConstant)pair.Key, pair => pair.Value));
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
        ///     Checks whether the properties are equal.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        /// <param name="getProperty">The expression to get property.</param>
        /// <returns>If true property is equal, otherwise false.</returns>
        [Pure]
        public static bool PropertyNameEqual<T>(string propertyName, [NotNull] Expression<Func<T, object>> getProperty)
        {
            return getProperty.GetMemberInfo().Name == propertyName;
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
            return PropertyNameEqual(args.PropertyName, getProperty);
        }

        /// <summary>
        ///     Checks whether the properties are equal.
        /// </summary>
        /// <param name="args">The specified property changed args.</param>
        /// /// <param name="item">The specified model.</param>
        /// <param name="getProperty">The expression to get property.</param>
        /// <returns>If true property is equal, otherwise false.</returns>
        [Pure]
        public static bool PropertyNameEqual<T>([NotNull] this PropertyChangedEventArgs args, T item, [NotNull] Expression<Func<T, object>> getProperty)
        {
            return PropertyNameEqual(args, getProperty);
        }

        /// <summary>
        ///     Checks whether the properties are equal.
        /// </summary>
        public static bool PropertyNameEqual(string changedProperty, string sourceProperty, bool emptySourcePathResult = false)
        {
            if (string.IsNullOrEmpty(changedProperty) || changedProperty.Equals(sourceProperty))
                return true;
            if (string.IsNullOrEmpty(sourceProperty))
                return emptySourcePathResult;
            if (sourceProperty.StartsWith("[", StringComparison.Ordinal) &&
                (changedProperty == "Item" || changedProperty == "Item[]" || changedProperty == "Item" + sourceProperty))
                return true;
            return false;
        }

        /// <summary>
        ///     Gets property name from the specified expression.
        /// </summary>
        /// <typeparam name="T">The type of model.</typeparam>
        /// <param name="expression">The specified expression.</param>
        /// <returns>An instance of string.</returns>
        [Pure]
        public static string GetPropertyName<T>([NotNull] Expression<Func<T, object>> expression)
        {
            return expression.GetMemberInfo().Name;
        }

        /// <summary>
        ///     Gets property name from the specified expression.
        /// </summary>
        /// <typeparam name="T">The type of model.</typeparam>
        /// <param name="item">The specified model.</param>
        /// <param name="expression">The specified expression.</param>
        /// <returns>An instance of string.</returns>
        [Pure]
        public static string GetPropertyName<T>([CanBeNull] T item, [NotNull] Expression<Func<T, object>> expression)
        {
            return GetPropertyName(expression);
        }

        /// <summary>
        ///     Sets the specified state for all items in the collection.
        /// </summary>
        /// <param name="collection">The specified collection.</param>
        /// <param name="state">The state value.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        public static void SetStateForAll([NotNull] this ITrackingCollection collection, EntityState state,
            bool validateState = false)
        {
            Should.NotBeNull(collection, "collection");
            foreach (var item in collection)
                collection.UpdateState(item.Entity, state, validateState);
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        [Pure]
        public static bool HasChanges<T>(this IEntitySnapshot snapshot, T item, Expression<Func<T, object>> propertyExpression)
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
        public static void SetStateForAll([NotNull] this ITrackingCollection collection, [NotNull] Func<TrackingEntity<object>, bool> predicate, EntityState state, bool validateState = false)
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
            EntityState newState, EntityState updateState, bool validateState = false)
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
            bool validateState = false)
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
            EntityState state, bool validateState = false)
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
            [NotNull] IEnumerable<IEntityStateEntry> items, bool validateState = false)
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
        public static IDataContext ToNonReadOnly([CanBeNull] this IDataContext context)
        {
            if (context == null)
                return new DataContext();
            if (context.IsReadOnly)
                return new DataContext(context);
            return context;
        }

        /// <summary>
        ///     Creates an array from a <see cref="IDataContext" />.
        /// </summary>
        public static DataConstantValue[] ToArray([NotNull] this IDataContext context)
        {
            Should.NotBeNull(context, "context");
            return context.ToList().ToArrayFast();
        }

        internal static void Invoke(this IThreadManager threadManager, ExecutionMode mode, Action invokeAction)
        {
            switch (mode)
            {
                case ExecutionMode.SynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        invokeAction();
                    else
                        threadManager.InvokeOnUiThread(invokeAction);
                    break;
                case ExecutionMode.Asynchronous:
                    threadManager.InvokeAsync(invokeAction);
                    break;
                case ExecutionMode.AsynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        invokeAction();
                    else
                        threadManager.InvokeOnUiThreadAsync(invokeAction);
                    break;
                default:
                    invokeAction();
                    break;
            }
        }

        internal static void Invoke<TTarget, TArg>(this IThreadManager threadManager, ExecutionMode mode, TTarget target, TArg arg1, Action<TTarget, TArg> invokeAction)
        {
            switch (mode)
            {
                case ExecutionMode.SynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        invokeAction(target, arg1);
                    else
                        threadManager.InvokeOnUiThread(() => invokeAction(target, arg1));
                    break;
                case ExecutionMode.Asynchronous:
                    threadManager.InvokeAsync(() => invokeAction(target, arg1));
                    break;
                case ExecutionMode.AsynchronousOnUiThread:
                    if (threadManager.IsUiThread)
                        invokeAction(target, arg1);
                    else
                        threadManager.InvokeOnUiThreadAsync(() => invokeAction(target, arg1));
                    break;
                default:
                    invokeAction(target, arg1);
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

        #endregion
    }
}