using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IDiffableEqualityComparer : IComponent
    {
        bool AreItemsTheSame(object? x, object? y);
    }
}