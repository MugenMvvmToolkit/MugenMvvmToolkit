using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IComponentOwner<IWrapperManager>, IComponent<IMugenApplication>
    {
        bool CanWrap<TRequest>(Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        object Wrap<TRequest>(Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}