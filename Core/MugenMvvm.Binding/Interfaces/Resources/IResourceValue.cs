using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceValue : IWeakItem
    {
        bool IsStatic { get; }

        object? Value { get; }
    }
}