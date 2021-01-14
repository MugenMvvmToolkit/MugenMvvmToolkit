using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Bindings.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IgnoreBindingMemberAttribute : BindingSyntaxExtensionAttributeBase
    {
        protected override bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            result = BindingMemberAttribute.GetTarget(context, expression);
            return result != null;
        }
    }
}