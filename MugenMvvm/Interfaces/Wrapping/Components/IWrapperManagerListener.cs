using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping.Components
{
    public interface IWrapperManagerListener : IComponent<IWrapperManager>
    {
        void OnWrapped(IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata);
    }
}