using System.Collections.Generic;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface ILambdaExpressionNode : IExpressionNode
    {
        IReadOnlyList<IParameterExpressionNode> Parameters { get; }

        IExpressionNode Body { get; }
    }
}