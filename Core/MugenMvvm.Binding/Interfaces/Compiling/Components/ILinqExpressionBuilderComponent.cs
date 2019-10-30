using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Compiling.Components
{
    public interface ILinqExpressionBuilderComponent : IComponent<IExpressionCompiler>
    {
        Expression? TryBuild(ILinqExpressionBuilderContext context, IExpressionNode expression);
    }
}