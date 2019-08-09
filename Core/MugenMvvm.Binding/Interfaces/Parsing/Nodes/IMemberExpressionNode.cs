using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IMemberExpressionNode : IExpressionNode
    {
        MemberInfo? Member { get; }

        string MemberName { get; }

        IExpressionNode? Target { get; }
    }
}