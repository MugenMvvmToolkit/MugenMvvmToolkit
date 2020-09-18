﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Commands;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IBusyToken BeginBusy(this IBusyManager busyManager, object? request = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            var token = busyManager.TryBeginBusy(request, metadata);
            if (token == null)
                ExceptionManager.ThrowRequestNotSupported<IBusyManagerComponent>(busyManager, request, metadata);
            return token;
        }

        public static void ClearBusy(this IBusyManager busyManager)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            foreach (var t in busyManager.GetTokens().Iterator())
                t.Dispose();
        }

        public static IEntityTrackingCollection GetTrackingCollection(this IEntityManager entityManager, object? request = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var collection = entityManager.TryGetTrackingCollection(request, metadata);
            if (collection == null)
                ExceptionManager.ThrowRequestNotSupported<IEntityTrackingCollectionProviderComponent>(entityManager, request, metadata);
            return collection;
        }

        public static IEntityStateSnapshot GetSnapshot(this IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var snapshot = entityManager.TryGetSnapshot(entity, metadata);
            if (snapshot == null)
                ExceptionManager.ThrowRequestNotSupported<IEntityStateSnapshotProviderComponent>(entityManager, entity, metadata);
            return snapshot;
        }

        public static void Serialize(this ISerializer serializer, Stream stream, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TrySerialize(stream, request, metadata))
                ExceptionManager.ThrowRequestNotSupported<ISerializerComponent>(serializer, request, metadata);
        }

        public static object? Deserialize(this ISerializer serializer, Stream stream, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TryDeserialize(stream, metadata, out var result))
                ExceptionManager.ThrowRequestNotSupported<ISerializerComponent>(serializer, stream, metadata);
            return result;
        }

        public static IWeakReference GetWeakReference(this IWeakReferenceManager weakReferenceManager, object? item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(weakReferenceManager, nameof(weakReferenceManager));
            var result = weakReferenceManager.TryGetWeakReference(item, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IWeakReferenceProviderComponent>(weakReferenceManager, item, metadata);
            return result;
        }

        public static void ExecuteRaw(IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (!threadDispatcher.DefaultIfNull().TryExecute(executionMode, handler, state, metadata))
                ExceptionManager.ThrowRequestNotSupported<IThreadDispatcherComponent>(threadDispatcher.DefaultIfNull(), handler, metadata);
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action action, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, null, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action<object?> action, object? state, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, action, state, metadata);

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, object? state, IReadOnlyMetadataContext? metadata = null) =>
            ExecuteRaw(threadDispatcher, executionMode, handler, state, metadata);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<object>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<T>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand(this ICommandManager? commandManager, Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<object>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<T>(this ICommandManager? commandManager, Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            commandManager.DefaultIfNull().GetCommand<T>(DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify), metadata);

        public static ICompositeCommand GetCommand<TParameter>(this ICommandManager commandManager, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(commandManager, nameof(commandManager));
            var result = commandManager.TryGetCommand<TParameter>(request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<ICommandProviderComponent>(commandManager, request, metadata);
            return result;
        }

        public static object Wrap(this IWrapperManager wrapperManager, Type wrapperType, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = wrapperManager.TryWrap(wrapperType, request, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        public static IWrapperManagerComponent AddWrapper<TConditionRequest, TWrapRequest>(this IWrapperManager wrapperManager, Func<Type, TConditionRequest, IReadOnlyMetadataContext?, bool> condition,
            Func<Type, TWrapRequest, IReadOnlyMetadataContext?, object?> wrapperFactory, int priority = WrappingComponentPriority.WrapperManger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var wrapper = new DelegateWrapperManager<TConditionRequest, TWrapRequest>(condition, wrapperFactory) { Priority = priority };
            wrapperManager.Components.Add(wrapper, metadata);
            return wrapper;
        }

        public static bool IsInBackground(this IMugenApplication application, bool defaultValue = false)
        {
            Should.NotBeNull(application, nameof(application));
            return application.GetMetadataOrDefault().Get(ApplicationMetadata.IsInBackground, defaultValue);
        }

        public static T? TryGetUnderlyingItem<T>(object target) where T : class
        {
            while (true)
            {
                if (target is T r)
                    return r;

                if (!(target is IWrapper<object> t))
                    return null;

                target = t.Target;
            }
        }

        public static object GetUnderlyingItem(object target)
        {
            while (true)
            {
                if (!(target is IWrapper<object> t))
                    return target;
                target = t.Target;
            }
        }

        public static TTo CastGeneric<TFrom, TTo>(TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((Func<TFrom, TTo>)(object)GenericCaster<TFrom>.Cast).Invoke(value);
            return (TTo)(object)value!;
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args) => string.Format(format, args);

        [Pure]
        public static string Flatten(this Exception exception, bool includeStackTrace = false) => exception.Flatten(string.Empty, includeStackTrace);

        [Pure]
        public static string Flatten(this Exception exception, string message, bool includeStackTrace = false)
        {
            Should.NotBeNull(exception, nameof(exception));
            var sb = new StringBuilder(message);
            FlattenInternal(exception, sb, includeStackTrace);
            return sb.ToString();
        }

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference>? valueHolder) => valueHolder?.Value?.Release();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item) => MugenService.WeakReferenceManager.GetWeakReference(item);

        internal static bool LazyInitialize<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class => Interlocked.CompareExchange(ref item, value, null) == null;

        internal static bool LazyInitializeDisposable<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

        internal static bool WhenAny(this bool[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    return true;
            }

            return false;
        }

        private static void FlattenInternal(Exception? exception, StringBuilder sb, bool includeStackTrace)
        {
            if (exception == null)
                return;
            if (exception is AggregateException aggregateException)
            {
                sb.AppendLine(aggregateException.Message);
                if (includeStackTrace)
                {
                    sb.Append(exception.StackTrace);
                    sb.AppendLine();
                }

                for (var index = 0; index < aggregateException.InnerExceptions.Count; index++)
                    FlattenInternal(aggregateException.InnerExceptions[index], sb, includeStackTrace);
                return;
            }

            while (exception != null)
            {
                sb.AppendLine(exception.Message);
                if (includeStackTrace)
                    sb.Append(exception.StackTrace);

                if (exception is ReflectionTypeLoadException loadException && loadException.LoaderExceptions != null)
                {
                    if (includeStackTrace)
                        sb.AppendLine();
                    for (var index = 0; index < loadException.LoaderExceptions.Length; index++)
                        FlattenInternal(loadException.LoaderExceptions[index], sb, includeStackTrace);
                }

                exception = exception.InnerException;
                if (exception != null && includeStackTrace)
                    sb.AppendLine();
            }
        }

        #endregion

        #region Nested types

        private static class GenericCaster<T>
        {
            #region Fields

            public static readonly Func<T, T> Cast = arg1 => arg1;

            #endregion
        }

        #endregion
    }
}