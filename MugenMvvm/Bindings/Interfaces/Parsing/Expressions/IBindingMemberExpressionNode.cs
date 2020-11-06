using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpressionNode : IExpressionNode
    {
        EnumFlags<BindingMemberExpressionFlags> Flags { get; set; }

        EnumFlags<MemberFlags> MemberFlags { get; }

        int Index { get; set; }

        string Path { get; }

        object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path);

        object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}