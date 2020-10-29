using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingBuilderListExpressionParser : IBindingExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata) =>
            expression is IReadOnlyList<IBindingBuilder> result ? ItemOrList.FromList(result) : default;

        #endregion
    }
}