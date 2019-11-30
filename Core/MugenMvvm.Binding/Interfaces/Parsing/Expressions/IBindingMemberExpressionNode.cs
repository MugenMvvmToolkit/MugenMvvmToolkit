using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpressionNode : IExpressionNode
    {
        BindingMemberExpressionFlags Flags { get; set; }

        int Index { get; set; }

        string Path { get; }

        object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        IMemberPathObserver GetBindingTarget(object target, object? source, IReadOnlyMetadataContext? metadata);

        object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}