using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingBuilderComponent : IComponent<IBindingManager>
    {
    }

    public interface IDataBindingBuilderComponent<TExpression> : IDataBindingBuilderComponent
    {
        ItemOrList<IDataBinding, IReadOnlyList<IDataBinding>> TryBuildBindings(in TExpression expression, object target, in ItemOrList<object?, IReadOnlyList<object>> sources,
            IReadOnlyMetadataContext? metadata);
    }
}