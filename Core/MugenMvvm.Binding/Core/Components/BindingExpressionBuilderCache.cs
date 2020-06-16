using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionBuilderCache : ComponentCacheBase<IBindingManager, IBindingExpressionBuilderComponent>, IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<object?> _cache;

        #endregion

        #region Constructors

        public BindingExpressionBuilderCache()
        {
            _cache = new StringOrdinalLightDictionary<object?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>([DisallowNull] in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TExpression>() || !(expression is string s))
                return Components.TryBuildBindingExpression(expression, metadata);

            if (!_cache.TryGetValue(s, out var value))
            {
                value = Components.TryBuildBindingExpression(expression, metadata).GetRawValue();
                _cache[s] = value;
            }

            return ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>>.FromRawValue(value);
        }

        #endregion

        #region Methods

        public override void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            _cache.Clear();
        }

        #endregion
    }
}