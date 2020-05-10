using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingExpressionBuilderComponent : IBindingExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>>>? TryBuildBindingExpression { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> IBindingExpressionBuilderComponent.TryBuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            return TryBuildBindingExpression?.Invoke(expression!, typeof(TExpression), metadata) ?? default;
        }

        #endregion
    }
}