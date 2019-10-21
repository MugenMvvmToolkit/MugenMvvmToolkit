using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceExpressionInterceptor : IComponent<IBindingManager>
    {
        IExpressionNode InterceptSourceExpression(IExpressionNode targetExpression, IExpressionNode sourceExpression, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata);
    }
}