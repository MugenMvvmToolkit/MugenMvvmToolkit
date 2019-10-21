using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpression : IParameterExpression
    {
        BindingMemberExpressionFlags Flags { get; set; }

        void SetIndex(int index);

        IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata);

        IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}