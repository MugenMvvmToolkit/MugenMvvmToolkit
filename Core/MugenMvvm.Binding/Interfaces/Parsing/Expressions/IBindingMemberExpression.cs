using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpression : IParameterExpression
    {
        void SetIndex(int index);

        IMemberPathObserver GetObserver(object target, object? source, IReadOnlyMetadataContext? metadata);//todo review
    }
}