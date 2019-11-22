using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionInterceptorComponent : IComponent<IBindingManager>
    {
        bool IsCachePerTypeRequired { get; }

        void Intercept(object target, object? source, ref IExpressionNode targetExpression, ref IExpressionNode sourceExpression,
            ref ItemOrList<IExpressionNode?, List<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata);
    }
}