using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMemberExpressionNode : IHasTargetExpressionNode
    {
        MemberInfo? Member { get; }

        string MemberName { get; }
    }
}