using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IgnoreBindingMemberAttribute : BindingSyntaxExtensionAttributeBase
    {
        #region Methods

        protected override bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            result = BindingMemberAttribute.GetTarget(context, expression);
            return result != null;
        }

        #endregion
    }
}