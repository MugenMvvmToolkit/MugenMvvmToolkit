using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBindingComponentExpression : IExpressionNode
    {
        string Name { get; }

        IComponent<IBinding> GetComponent(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}