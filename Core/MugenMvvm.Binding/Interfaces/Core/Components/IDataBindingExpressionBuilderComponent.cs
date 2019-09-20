using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingExpressionBuilderComponent : IComponent<IBindingManager>
    {
    }

    public interface IDataBindingExpressionBuilderComponent<TExpression> : IDataBindingExpressionBuilderComponent
    {
        IReadOnlyList<IDataBindingExpression> TryBuildBindingExpressions(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}