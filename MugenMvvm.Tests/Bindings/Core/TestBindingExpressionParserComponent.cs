using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingExpressionParserComponent : IBindingExpressionParserComponent, IHasPriority
    {
        public Func<IBindingManager, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBindingBuilder>>? TryParseBindingExpression { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IBindingBuilder> IBindingExpressionParserComponent.TryParseBindingExpression(IBindingManager bindingManager, object expression,
            IReadOnlyMetadataContext? metadata) =>
            TryParseBindingExpression?.Invoke(bindingManager, expression, metadata) ?? default;
    }
}