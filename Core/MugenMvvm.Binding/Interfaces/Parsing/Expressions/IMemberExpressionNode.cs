using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMemberExpressionNode : IHasTargetExpressionNode<IMemberExpressionNode>
    {
        IBindingMemberAccessorInfo? Member { get; }

        string MemberName { get; }
    }
}