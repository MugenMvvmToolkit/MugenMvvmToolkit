namespace MugenMvvm.Interfaces.Collections
{
    public interface IDiffableEqualityComparer
    {
        bool AreItemsTheSame(object? x, object? y);
    }
}