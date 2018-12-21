namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMementoResult
    {
        bool IsRestored { get; }

        IReadOnlyContext Context { get; }

        object? Target { get; }
    }
}