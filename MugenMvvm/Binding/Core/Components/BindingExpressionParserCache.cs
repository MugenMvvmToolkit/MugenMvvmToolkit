using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingExpressionParserCache : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent
    {
        #region Fields

        private readonly Dictionary<string, object?> _cache;

        #endregion

        #region Constructors

        public BindingExpressionParserCache(int priority = ComponentPriority.Cache)
            : base(priority)
        {
            _cache = new Dictionary<string, object?>(59, StringComparer.Ordinal);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (!(expression is string s))
                return Components.TryParseBindingExpression(bindingManager, expression, metadata);

            if (!_cache.TryGetValue(s, out var value))
            {
                value = Components.TryParseBindingExpression(bindingManager, expression, metadata).GetRawValue();
                _cache[s] = value;
            }

            return ItemOrList.FromRawValueReadonly<IBindingBuilder>(value, true);
        }

        #endregion

        #region Methods

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => _cache.Clear();

        #endregion
    }
}