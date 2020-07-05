using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping.Components
{
    public interface IWrapperManagerComponent : IComponent<IWrapperManager>
    {
        bool CanWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);

        object? TryWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}