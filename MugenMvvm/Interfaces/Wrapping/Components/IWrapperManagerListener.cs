using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping.Components
{
    public interface IWrapperManagerListener : IComponent<IWrapperManager>
    {
        void OnWrapped<TRequest>(IWrapperManager wrapperManager, object wrapper, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}