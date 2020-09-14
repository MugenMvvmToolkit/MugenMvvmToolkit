using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Metadata.Internal
{
    public class TestMetadataContextValueManagerComponent : IMetadataContextValueManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContext _context;

        #endregion

        #region Constructors

        public TestMetadataContextValueManagerComponent(IMetadataContext context)
        {
            _context = context;
        }

        #endregion

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

        int IMetadataContextValueManagerComponent.GetCount(IMetadataContext context)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return GetCount?.Invoke() ?? 0;
        }

        IEnumerable<KeyValuePair<IMetadataContextKey, object?>> IMetadataContextValueManagerComponent.GetValues(IMetadataContext context)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return GetValues?.Invoke() ?? Default.Array<KeyValuePair<IMetadataContextKey, object?>>();
        }

        bool IMetadataContextValueManagerComponent.Contains(IMetadataContext context, IMetadataContextKey contextKey)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return Contains?.Invoke(contextKey) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryGetValue(IMetadataContext context, IMetadataContextKey contextKey, out object? rawValue)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            var tuple = TryGetValue?.Invoke(contextKey);
            if (tuple == null)
            {
                rawValue = null;
                return false;
            }

            rawValue = tuple.Value.Item2;
            return tuple.Value.Item1;
        }

        bool IMetadataContextValueManagerComponent.TrySetValue(IMetadataContext context, IMetadataContextKey contextKey, object? rawValue)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return TrySetValue?.Invoke(contextKey, rawValue) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryRemove(IMetadataContext context, IMetadataContextKey contextKey)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            context.ShouldEqual(_context);
            return TryClear?.Invoke(contextKey) ?? false;
        }

        void IMetadataContextValueManagerComponent.Clear(IMetadataContext context)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            context.ShouldEqual(_context);
            Clear?.Invoke();
        }

        #endregion
    }
}