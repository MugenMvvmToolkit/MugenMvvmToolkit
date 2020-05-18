using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionBuilderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}