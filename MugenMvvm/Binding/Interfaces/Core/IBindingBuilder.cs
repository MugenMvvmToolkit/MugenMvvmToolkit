using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBindingBuilder
    {
        IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null);
    }
}