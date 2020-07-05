using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionParserComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>(IBindingManager bindingManager, [DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}