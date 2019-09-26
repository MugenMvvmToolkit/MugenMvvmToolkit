using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface IExpressionCompiler : IComponentOwner<IExpressionCompiler>, IComponent<IBindingManager>
    {
        ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null);
    }
}