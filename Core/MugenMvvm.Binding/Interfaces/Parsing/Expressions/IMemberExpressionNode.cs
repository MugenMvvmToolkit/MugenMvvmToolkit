using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMemberExpressionNode : IExpressionNode
    {
        MemberInfo? Member { get; }

        string MemberName { get; }

        IExpressionNode? Target { get; }
    }
}