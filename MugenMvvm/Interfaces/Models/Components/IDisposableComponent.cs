using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models.Components
{
    public interface IDisposableComponent<T> : IComponent<T> where T : class
    {
        void OnDisposing(T owner, IReadOnlyMetadataContext? metadata);

        void OnDisposed(T owner, IReadOnlyMetadataContext? metadata);
    }
}