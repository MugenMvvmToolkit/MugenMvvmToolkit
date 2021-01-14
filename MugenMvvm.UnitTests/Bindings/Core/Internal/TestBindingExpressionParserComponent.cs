using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingExpressionParserComponent : IBindingExpressionParserComponent, IHasPriority
    {
        private readonly IBindingManager? _bindingManager;

        public TestBindingExpressionParserComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBindingBuilder>>? TryParseBindingExpression { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IBindingBuilder> IBindingExpressionParserComponent.TryParseBindingExpression(IBindingManager bindingManager, object expression,
            IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            return TryParseBindingExpression?.Invoke(expression, metadata) ?? default;
        }
    }
}