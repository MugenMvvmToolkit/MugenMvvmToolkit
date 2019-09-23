using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingExpression : IMetadataOwner<IReadOnlyMetadataContext>
    {
        IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null);
    }
}