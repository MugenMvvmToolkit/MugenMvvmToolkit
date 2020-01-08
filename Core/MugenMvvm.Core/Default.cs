using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm
{
    public static class Default
    {
        #region Fields

        internal const string IndexerName = "Item[]";

        private static int _counter;

        internal static readonly PropertyChangedEventArgs IsSuspendedChangedArgs = new PropertyChangedEventArgs(nameof(ISuspendable.IsSuspended));
        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs = new PropertyChangedEventArgs(string.Empty);
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs = new PropertyChangedEventArgs(nameof(IList.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs = new PropertyChangedEventArgs(IndexerName);
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        internal static readonly Action NoDoAction = NoDo;

        public static readonly IReadOnlyMetadataContext Metadata = EmptyContext.Instance;
        public static readonly IDisposable Disposable = EmptyContext.Instance;
        public static readonly IWeakReference WeakReference = EmptyContext.Instance;
        public static readonly Task CompletedTask = Task.FromResult<object?>(null);
        public static readonly Task<bool> TrueTask = Task.FromResult(true);
        public static readonly Task<bool> FalseTask = Task.FromResult(false);
        public static readonly INavigationProvider NavigationProvider = EmptyContext.Instance;

        #endregion

        #region Methods

        public static IEnumerator<T> SingleValueEnumerator<T>(T value)
        {
            yield return value;
        }

        public static T[] EmptyArray<T>()
        {
            return EmptyArrayImpl<T>.Instance;
        }

        public static Task<T> CanceledTask<T>()
        {
            return CanceledTaskImpl<T>.Instance;
        }

        public static ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>()
        {
            return EmptyDictionaryImpl<TKey, TValue>.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullable<T>()
        {
            return GenericChecker<T>.IsNullableType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType<T>()
        {
            return GenericChecker<T>.IsValueType;
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

        private sealed class EmptyContext : IReadOnlyMetadataContext, IDisposable, INavigationProvider, IWeakReference
        {
            #region Fields

            public static readonly EmptyContext Instance = new EmptyContext();

            #endregion

            #region Constructors

            private EmptyContext()
            {
            }

            #endregion

            #region Properties

            public int Count => 0;

            public string Id => string.Empty;

            bool IWeakItem.IsAlive => false;

            object? IWeakReference.Target => null;

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

            public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
            {
                value = contextKey.GetDefaultValue(this, defaultValue);
                return false;
            }

            public bool Contains(IMetadataContextKey contextKey)
            {
                return false;
            }

            void IWeakReference.Release()
            {
            }

            #endregion
        }

        private static class EmptyArrayImpl<T>
        {
            #region Fields

            public static readonly T[] Instance = new T[0];

            #endregion
        }

        private static class EmptyDictionaryImpl<TKey, TValue>
        {
            #region Fields

            public static readonly ReadOnlyDictionary<TKey, TValue> Instance = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

            #endregion
        }

        private static class CanceledTaskImpl<T>
        {
            #region Fields

            public static readonly Task<T> Instance = GetTask();

            #endregion

            #region Methods

            private static Task<T> GetTask()
            {
                var tcs = new TaskCompletionSource<T>();
                tcs.SetCanceled();
                return tcs.Task;
            }

            #endregion
        }

        private static class GenericChecker<T>
        {
            #region Fields

            public static readonly bool IsNullableType = default(T)! == null;
            public static readonly bool IsValueType = typeof(T).IsValueType;

            #endregion
        }

        #endregion
    }
}