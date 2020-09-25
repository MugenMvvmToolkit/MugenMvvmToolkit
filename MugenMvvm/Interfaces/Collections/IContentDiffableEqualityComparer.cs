namespace MugenMvvm.Interfaces.Collections
{
    public interface IContentDiffableEqualityComparer : IDiffableEqualityComparer
    {
        bool AreContentsTheSame(object? x, object? y);
    }
}