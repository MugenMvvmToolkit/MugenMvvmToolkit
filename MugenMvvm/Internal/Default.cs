using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Internal
{
    public static class Default
    {
        #region Fields

        private static int _counter;

        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs = new(string.Empty);
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs = new(nameof(IList.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs = new(InternalConstant.IndexerName);
        internal static readonly PropertyChangedEventArgs IsBusyPropertyChangedArgs = new(nameof(ViewModelBase.IsBusy));
        internal static readonly PropertyChangedEventArgs BusyTokenPropertyChangedArgs = new(nameof(ViewModelBase.BusyToken));
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs = new(NotifyCollectionChangedAction.Reset);

        public static readonly IReadOnlyMetadataContext Metadata = EmptyContext.Instance;
        public static readonly IDisposable Disposable = EmptyContext.Instance;
        public static readonly IWeakReference WeakReference = EmptyContext.Instance;
        public static readonly Task CompletedTask = Task.CompletedTask;
        public static readonly Task<bool> TrueTask = Task.FromResult(true);
        public static readonly Task<bool> FalseTask = Task.FromResult(false);
        public static readonly INavigationProvider NavigationProvider = EmptyContext.Instance;

        #endregion

        #region Methods

        public static IEnumerator<T> SingleValueEnumerator<T>(T value)
        {
            yield return value;
        }

        public static T[] Array<T>() => EmptyArrayImpl<T>.Instance;

        public static Type[] Types<T1>() => TypeCache<T1>.Types;

        public static Type[] Types<T1, T2>() => TypeCache<T1, T2>.Types;

        public static ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>() where TKey : notnull => EmptyDictionaryImpl<TKey, TValue>.Instance;

        internal static PropertyChangedEventArgs GetOrCreatePropertyChangedArgs(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return EmptyPropertyChangedArgs;
            return new PropertyChangedEventArgs(propertyName);
        }

        internal static int NextCounter() => Interlocked.Increment(ref _counter);

        #endregion

        #region Nested types

        private static class TypeCache<T1>
        {
            #region Fields

            public static readonly Type[] Types = {typeof(T1)};

            #endregion
        }

        private static class TypeCache<T1, T2>
        {
            #region Fields

            public static readonly Type[] Types = {typeof(T1), typeof(T2)};

            #endregion
        }

        private sealed class EmptyContext : IReadOnlyMetadataContext, IDisposable, INavigationProvider, IWeakReference
        {
            #region Fields

            public static readonly EmptyContext Instance = new();

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

            public ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>> GetValues() => default;

            public bool Contains(IMetadataContextKey contextKey) => false;

            public bool TryGetRaw(IMetadataContextKey contextKey, [MaybeNullWhen(false)] out object? value)
            {
                value = null;
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

        private static class EmptyDictionaryImpl<TKey, TValue> where TKey : notnull
        {
            #region Fields

            public static readonly ReadOnlyDictionary<TKey, TValue> Instance = new(new Dictionary<TKey, TValue>());

            #endregion
        }

        #endregion
    }
}