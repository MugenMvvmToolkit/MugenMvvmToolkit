using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Bindings.Interfaces.Compiling.Components
{
    public interface IExpressionBuilderComponent : IComponent<IExpressionCompiler>
    {
        Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression);
    }
}