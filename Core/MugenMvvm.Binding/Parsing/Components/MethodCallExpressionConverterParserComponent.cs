using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MethodCallExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        public MethodCallExpressionConverterParserComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Method;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is MethodCallExpression methodCallExpression))
                return null;

            var method = methodCallExpression.Method;
            var target = context.ConvertOptional(methodCallExpression.Object) ?? ConstantExpressionNode.Get(method.DeclaringType);
            var args = context.Convert(methodCallExpression.Arguments);
            if (_memberProvider.DefaultIfNull().GetMember(method.DeclaringType, method.Name, MemberType.Method, method.GetAccessModifiers(), context.Metadata) is IMethodInfo memberInfo)
                return new MethodCallExpressionNode(target, memberInfo, args);
            return new MethodCallExpressionNode(target, method.Name, args);
        }

        #endregion
    }
}