using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Compiling.Components
{
    public interface IExpressionBuilderCompilerComponent : IComponent<IExpressionCompiler>
    {
        Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression);
    }
}