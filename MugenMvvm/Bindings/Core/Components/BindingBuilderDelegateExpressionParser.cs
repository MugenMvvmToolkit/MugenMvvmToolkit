using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingBuilderDelegateExpressionParser : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent
    {
        private readonly Dictionary<BindingBuilderDelegate, object?> _cache;

        public BindingBuilderDelegateExpressionParser(int priority = BindingComponentPriority.BuilderCache)
            : base(priority)
        {
            _cache = new Dictionary<BindingBuilderDelegate, object?>(47, InternalEqualityComparer.Reference);
        }

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is BindingBuilderDelegate del)
            {
                if (!_cache.TryGetValue(del, out var value))
                {
                    value = Components.TryParseBindingExpression(bindingManager, del().GetRawValue()!, metadata).GetRawValue();
                    _cache[del] = value;
                }

                if (value != this)
                    return ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(value);
            }

            return Components.TryParseBindingExpression(bindingManager, expression, metadata);
        }

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata) => _cache.Clear();
    }
}