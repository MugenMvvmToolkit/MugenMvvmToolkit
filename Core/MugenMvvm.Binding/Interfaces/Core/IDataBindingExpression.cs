using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IDataBindingExpression : IMetadataOwner<IReadOnlyMetadataContext>
    {
        IDataBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null);
    }
}