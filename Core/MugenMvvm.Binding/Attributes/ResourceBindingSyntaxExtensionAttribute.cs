using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;

namespace MugenMvvm.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ResourceBindingSyntaxExtensionAttribute : BindingSyntaxExtensionAttributeBase
    {
        #region Fields

        private readonly UnaryExpressionNode _expressionNode;

        #endregion

        #region Constructors

        public ResourceBindingSyntaxExtensionAttribute(string resourceName, bool isStatic = false)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            _expressionNode = new UnaryExpressionNode(isStatic ? UnaryTokenType.StaticExpression : UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, resourceName));
        }

        #endregion

        #region Properties

        public string ResourceName => ((MemberExpressionNode) _expressionNode.Operand).Member;

        public bool IsStatic => _expressionNode.Token == UnaryTokenType.StaticExpression;

        #endregion

        #region Methods

        protected override bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            result = _expressionNode;
            return true;
        }

        #endregion
    }
}