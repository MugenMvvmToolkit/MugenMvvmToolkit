using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IBindingMemberExpressionNode : IExpressionNode
    {
        EnumFlags<BindingMemberExpressionFlags> Flags { get; }

        EnumFlags<MemberFlags> MemberFlags { get; }

        int Index { get; }

        string Path { get; }

        string? ObservableMethodName { get; }

        IExpressionNode? Expression { get; }

        object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path);

        object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata); //todo generic?

        IBindingMemberExpressionNode Update(int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags, string? observableMethodName);
    }
}