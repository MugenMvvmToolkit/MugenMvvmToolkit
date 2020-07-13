using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingBuilderListExpressionParser : IBindingExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>(IBindingManager bindingManager, in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TExpression>() || !(expression is IReadOnlyList<IBindingBuilder> result))
                return default;
            return ItemOrList.FromList(result);
        }

        #endregion
    }
}