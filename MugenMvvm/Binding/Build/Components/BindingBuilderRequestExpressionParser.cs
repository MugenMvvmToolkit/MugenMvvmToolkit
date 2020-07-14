using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
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

        private readonly Dictionary<object, object?> _cache;

        #endregion

        #region Constructors

        public BindingBuilderRequestExpressionParser()
        {
            _cache = new Dictionary<object, object?>(47, InternalComparer.Reference);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>(IBindingManager bindingManager, [DisallowNull] in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TExpression) == typeof(BindingBuilderRequest))
            {
                var request = MugenExtensions.CastGeneric<TExpression, BindingBuilderRequest>(expression);
                if (!_cache.TryGetValue(request.OriginalDelegate, out var value))
                {
                    value = Components.TryParseBindingExpression(bindingManager, request.ToBindingExpressionRequest(), metadata).GetRawValue();
                    _cache[request.OriginalDelegate] = value;
                }

                return ItemOrList.FromRawValue<IBindingBuilder, IReadOnlyList<IBindingBuilder>>(value, true);
            }

            return Components.TryParseBindingExpression(bindingManager, expression, metadata);
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