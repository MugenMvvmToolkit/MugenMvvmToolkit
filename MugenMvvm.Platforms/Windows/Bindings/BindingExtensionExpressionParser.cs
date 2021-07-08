using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

#if AVALONIA
using MugenMvvm.Avalonia.Bindings.Markup;

namespace MugenMvvm.Avalonia.Bindings
#else
using MugenMvvm.Windows.Bindings.Markup;

namespace MugenMvvm.Windows.Bindings
#endif
{
    public sealed class BindingExtensionExpressionParser : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent
    {
        private readonly Dictionary<MugenBindingExtension, object?> _cache;

        public BindingExtensionExpressionParser(int priority = BindingComponentPriority.BuilderCache) : base(priority)
        {
            _cache = new Dictionary<MugenBindingExtension, object?>(59);
        }

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is not MugenBindingExtension ext)
                return Components.TryParseBindingExpression(bindingManager, expression, metadata);

            if (!_cache.TryGetValue(ext, out var value))
            {
                value = Components.TryParseBindingExpression(bindingManager, ext.ToRequest(), metadata).GetRawValue();
                _cache[ext] = value;
            }

            return ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(value);
        }

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata) => _cache.Clear();
    }
}