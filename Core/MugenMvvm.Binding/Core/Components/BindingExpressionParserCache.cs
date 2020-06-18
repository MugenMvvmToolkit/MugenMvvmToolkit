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
    public sealed class BindingExpressionParserCache : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<object?> _cache;

        #endregion

        #region Constructors

        public BindingExpressionParserCache()
        {
            _cache = new StringOrdinalLightDictionary<object?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>([DisallowNull] in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TExpression>() || !(expression is string s))
                return Components.TryParseBindingExpression(expression, metadata);

            if (!_cache.TryGetValue(s, out var value))
            {
                value = Components.TryParseBindingExpression(expression, metadata).GetRawValue();
                _cache[s] = value;
            }

            return ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(value);
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