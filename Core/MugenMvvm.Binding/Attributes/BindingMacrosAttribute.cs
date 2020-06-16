﻿using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;

namespace MugenMvvm.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BindingMacrosAttribute : BindingSyntaxExtensionAttributeBase
    {
        #region Fields

        private readonly UnaryExpressionNode _expressionNode;

        #endregion

        #region Constructors

        public BindingMacrosAttribute(string resourceName, bool isStatic = false)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            _expressionNode = UnaryExpressionNode.Get(isStatic ? UnaryTokenType.StaticExpression : UnaryTokenType.DynamicExpression, MemberExpressionNode.Get(null, resourceName));
        }

        #endregion

        #region Properties

        public string ResourceName => ((MemberExpressionNode)_expressionNode.Operand).Member;

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