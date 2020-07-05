using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling.Components
{
    public interface IExpressionCompilerComponent : IComponent<IExpressionCompiler>
    {
        ICompiledExpression? TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata);
    }
}