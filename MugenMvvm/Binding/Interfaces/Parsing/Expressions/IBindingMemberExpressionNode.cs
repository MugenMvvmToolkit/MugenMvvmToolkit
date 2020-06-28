using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpressionNode : IExpressionNode
    {
        BindingMemberExpressionFlags Flags { get; set; }

        int Index { get; set; }

        string Path { get; }

        object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}