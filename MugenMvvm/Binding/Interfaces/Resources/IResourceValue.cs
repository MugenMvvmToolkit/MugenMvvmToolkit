namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceValue
    {
        bool IsStatic { get; }

        object? Value { get; }
    }
}