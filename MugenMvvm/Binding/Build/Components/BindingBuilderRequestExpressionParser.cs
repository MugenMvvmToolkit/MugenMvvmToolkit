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

namespace MugenMvvm.Binding.Build.Components
{
    public sealed class BindingBuilderRequestExpressionParser : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly ReferenceLightDictionary<object?> _cache;

        #endregion

        #region Constructors

        public BindingBuilderRequestExpressionParser()
        {
            _cache = new ReferenceLightDictionary<object?>(47);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TExpression) == typeof(BindingBuilderRequest))
            {
                var request = MugenExtensions.CastGeneric<TExpression, BindingBuilderRequest>(expression);
                if (!_cache.TryGetValue(request.OriginalDelegate, out var value))
                {
                    value = Components.TryParseBindingExpression(request.ToBindingExpressionRequest(), metadata).GetRawValue();
                    _cache[request.OriginalDelegate] = value;
                }

                return ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(value);
            }

            return Components.TryParseBindingExpression(expression, metadata);
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