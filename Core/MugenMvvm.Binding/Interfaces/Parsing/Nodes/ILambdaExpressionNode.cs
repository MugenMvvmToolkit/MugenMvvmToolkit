using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface ILambdaExpressionNode : IExpressionNode
    {
        IReadOnlyList<string> Parameters { get; }

        IExpressionNode Body { get; }
    }
}