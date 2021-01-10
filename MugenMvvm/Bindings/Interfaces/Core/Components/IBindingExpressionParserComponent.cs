using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingExpressionParserComponent : IComponent<IBindingManager>
    {
        ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata);
    }
}