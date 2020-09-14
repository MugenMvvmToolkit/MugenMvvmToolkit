using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Binding.Core.Internal
{
    public class TestBindingExpressionParserComponent : IBindingExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;

        #endregion

        #region Constructors

        public TestBindingExpressionParserComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, IReadOnlyMetadataContext?, ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>>? TryParseBindingExpression { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> IBindingExpressionParserComponent.TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            return TryParseBindingExpression?.Invoke(expression, metadata) ?? default;
        }

        #endregion
    }
}