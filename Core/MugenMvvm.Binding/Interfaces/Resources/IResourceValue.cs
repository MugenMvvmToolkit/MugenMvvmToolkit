using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceValue : IWeakItem
    {
        object? Value { get; }
    }
}