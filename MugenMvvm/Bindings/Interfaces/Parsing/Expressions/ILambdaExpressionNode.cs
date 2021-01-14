using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface ILambdaExpressionNode : IExpressionNode
    {
        ItemOrIReadOnlyList<IParameterExpressionNode> Parameters { get; }

        IExpressionNode Body { get; }
    }
}