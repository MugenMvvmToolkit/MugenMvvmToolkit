using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Metadata.Internal
{
    public class TestMetadataContextValueManagerComponent : IMetadataContextValueManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<int>? GetCount { get; set; }

        public Func<IEnumerable<KeyValuePair<IMetadataContextKey, object?>>>? GetValues { get; set; }

        public Func<IMetadataContextKey, bool>? Contains { get; set; }

        public Func<IMetadataContextKey, (bool, object?)>? TryGetValue { get; set; }

        public Func<IMetadataContextKey, object?, bool>? TrySetValue { get; set; }

        public Func<IMetadataContextKey, bool>? TryClear { get; set; }

        public Action? Clear { get; set; }

        #endregion

        #region Implementation of interfaces

        int IMetadataContextValueManagerComponent.GetCount()
        {
            return GetCount?.Invoke() ?? 0;
        }

        IEnumerable<KeyValuePair<IMetadataContextKey, object?>> IMetadataContextValueManagerComponent.GetValues()
        {
            return GetValues?.Invoke() ?? Default.EmptyArray<KeyValuePair<IMetadataContextKey, object?>>();
        }

        bool IMetadataContextValueManagerComponent.Contains(IMetadataContextKey contextKey)
        {
            return Contains?.Invoke(contextKey) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryGetValue(IMetadataContextKey contextKey, out object? rawValue)
        {
            var tuple = TryGetValue?.Invoke(contextKey);
            if (tuple == null)
            {
                rawValue = null;
                return false;
            }

            rawValue = tuple.Value.Item2;
            return tuple.Value.Item1;
        }

        bool IMetadataContextValueManagerComponent.TrySetValue(IMetadataContextKey contextKey, object? rawValue)
        {
            return TrySetValue?.Invoke(contextKey, rawValue) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryClear(IMetadataContextKey contextKey)
        {
            return TryClear?.Invoke(contextKey) ?? false;
        }

        void IMetadataContextValueManagerComponent.Clear()
        {
            Clear?.Invoke();
        }

        #endregion
    }
}