using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models.Components
{
    public interface IHasCacheComponent<T> : IComponent<T> where T : class
    {
        void Invalidate(T owner, object? state, IReadOnlyMetadataContext? metadata);
    }
}