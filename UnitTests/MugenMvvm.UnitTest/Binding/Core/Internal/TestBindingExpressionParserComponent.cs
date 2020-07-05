using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingExpressionParserComponent : IBindingExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IBindingManager, object, Type, IReadOnlyMetadataContext?, ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>>? TryParseBindingExpression { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> IBindingExpressionParserComponent.TryParseBindingExpression<TExpression>(IBindingManager bindingManager, in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            return TryParseBindingExpression?.Invoke(bindingManager, expression!, typeof(TExpression), metadata) ?? default;
        }

        #endregion
    }
}