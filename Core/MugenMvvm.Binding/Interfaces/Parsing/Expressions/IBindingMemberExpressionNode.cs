using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpressionNode : IParameterExpressionNode
    {
        BindingMemberExpressionFlags Flags { get; set; }

        void SetIndex(int index);

        object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata);

        IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}