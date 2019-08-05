using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface ILambdaExpressionNode : IExpressionNode
    {
        IReadOnlyList<string> Parameters { get; }

        IExpressionNode Body { get; }
    }
}