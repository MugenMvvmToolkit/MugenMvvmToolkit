using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm
{
    public static class Default
    {
        #region Fields

        internal const string IndexerName = "Item[]";

        public static readonly object TrueObject;
        public static readonly object FalseObject;
        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs IsNotificationsSuspendedChangedArgs;
        internal static readonly PropertyChangedEventArgs IsBusyChangedArgs;
        internal static readonly PropertyChangedEventArgs BusyInfoChangedArgs;
        internal static readonly PropertyChangedEventArgs DisplayNameChangedArgs;
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs;
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs;
        internal static Action NoDoAction;
        private static int _counter;

        public static readonly NullValue SerializableNullValue;
        public static readonly IReadOnlyMetadataContext MetadataContext;
        public static readonly IDisposable Disposable;
        public static readonly WeakReference WeakReference;
        public static readonly Task CompletedTask;
        public static readonly Task<bool> TrueTask;
        public static readonly Task<bool> FalseTask;
        public static readonly INavigationProvider NavigationProvider;

        #endregion

        #region Constructors

        static Default()
        {
            TrueObject = true;
            FalseObject = false;
            ResetCollectionEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            EmptyPropertyChangedArgs = new PropertyChangedEventArgs(string.Empty);
            CountPropertyChangedArgs = new PropertyChangedEventArgs(nameof(IList.Count));
            IsNotificationsSuspendedChangedArgs = new PropertyChangedEventArgs(nameof(ISuspendNotifications.IsNotificationsSuspended));
            IsBusyChangedArgs = new PropertyChangedEventArgs("IsBusy");
            BusyInfoChangedArgs = new PropertyChangedEventArgs(nameof(IBusyIndicatorProvider.BusyInfo));
            DisplayNameChangedArgs = new PropertyChangedEventArgs(nameof(IHasDisplayName.DisplayName));
            IndexerPropertyChangedArgs = new PropertyChangedEventArgs(IndexerName);
            NoDoAction = NoDo;

            var emptyContext = new EmptyContext();
            MetadataContext = emptyContext;
            SerializableNullValue = new NullValue();
            WeakReference = new WeakReference(null, false);
            Disposable = (IDisposable) MetadataContext;
            TrueTask = Task.FromResult(true);
            FalseTask = Task.FromResult(false);
            CompletedTask = FalseTask;
            NavigationProvider = emptyContext;
        }

        #endregion

        #region Methods

        public static T[] EmptyArray<T>()
        {
            return Value<T>.ArrayInstance;
        }

        public static Task<T> CanceledTask<T>()
        {
            return Value<T>.CanceledTaskField;
        }

        public static object BoolToObject(bool value)
        {
            if (value)
                return TrueObject;
            return FalseObject;
        }

        internal static PropertyChangedEventArgs GetOrCreatePropertyChangedArgs(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return EmptyPropertyChangedArgs;
            return new PropertyChangedEventArgs(propertyName);
        }

        internal static int NextCounter()
        {
            return Interlocked.Increment(ref _counter);
        }

        private static void NoDo()
        {
        }

        #endregion

        #region Nested types

        [DataContract]
        public sealed class NullValue
        {
            #region Constructors

            internal NullValue()
            {
            }

            #endregion

            #region Methods

            public object To(object? value)
            {
                if (value == null)
                    return SerializableNullValue;
                return value;
            }

            public object? From(object? value)
            {
                if (value is NullValue)
                    return null;
                return value;
            }

            #endregion
        }

        private sealed class EmptyContext : IReadOnlyMetadataContext, IDisposable, INavigationProvider
        {
            #region Fields

            private static IMemento _emptyMemento;

            #endregion

            #region Properties

            public int Count => 0;

            public string Id => string.Empty;

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
            }

            public IEnumerator<MetadataContextValue> GetEnumerator()
            {
                return Enumerable.Empty<MetadataContextValue>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IMemento? GetMemento()
            {
                if (_emptyMemento == null)
                    _emptyMemento = StaticMemberMemento.Create(this, typeof(Default), nameof(MetadataContext));
                return _emptyMemento;
            }

            public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
            {
                value = contextKey.GetDefaultValue(this, defaultValue);
                return false;
            }

            public bool Contains(IMetadataContextKey contextKey)
            {
                return false;
            }

            #endregion
        }

        private static class Value<T>
        {
            #region Fields

            public static readonly T[] ArrayInstance;
            public static readonly Task<T> CanceledTaskField;

            #endregion

            #region Constructors

            static Value()
            {
                ArrayInstance = new T[0];
                var tcs = new TaskCompletionSource<T>();
                tcs.SetCanceled();
                CanceledTaskField = tcs.Task;
            }

            #endregion
        }

        #endregion
    }
}