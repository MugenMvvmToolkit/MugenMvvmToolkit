using System;
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

        private IExpressionNode? _expressionNode;
        private bool _initialized;

        #endregion

        #region Constructors

        public BindingMacrosAttribute(string resourceName, bool isStatic = false)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            ResourceName = resourceName;
            IsStatic = isStatic;
            ResourceNameIndex = -1;
        }

        public BindingMacrosAttribute(int resourceNameIndex, bool isStatic = false)
        {
            Should.BeValid(nameof(resourceNameIndex), resourceNameIndex >= 0);
            ResourceNameIndex = resourceNameIndex;
            IsStatic = isStatic;
        }

        #endregion

        #region Properties

        public string? ResourceName { get; }

        public bool IsStatic { get; }

        public int ResourceNameIndex { get; }

        #endregion

        #region Methods

        protected override bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            if (!_initialized)
            {
                string? resourceName;
                //T Resource<T>(this IBindingBuilderContext ctx, string resource);
                if (ResourceNameIndex >= 0 && expression is MethodCallExpression methodCall &&
                    methodCall.Arguments.Count > ResourceNameIndex && methodCall.Arguments[ResourceNameIndex] is ConstantExpression constant && constant.Value != null)
                    resourceName = constant.Value.ToString();
                else
                    resourceName = ResourceName;
                if (resourceName != null)
                    _expressionNode = UnaryExpressionNode.Get(IsStatic ? UnaryTokenType.StaticExpression : UnaryTokenType.DynamicExpression, MemberExpressionNode.Get(null, resourceName));
                _initialized = true;
            }

            result = _expressionNode;
            return _expressionNode != null;
        }

        #endregion
    }
}