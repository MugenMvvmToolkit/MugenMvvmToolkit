using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Converters
{
    public sealed class MethodCallExpressionConverterComponent : IExpressionConverterComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Method;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is MethodCallExpression methodCallExpression))
                return null;

            var method = methodCallExpression.Method;
            ParameterInfo[]? parameters = null;
            IExpressionNode target;
            string[]? typeArgs = null;
            var args = context.Convert(methodCallExpression.Arguments);
            if (method.GetAccessModifiers(true, ref parameters).HasFlagEx(MemberFlags.Extension))
            {
                target = args[0];
                args.RemoveAt(0);
            }
            else
                target = context.ConvertTarget(methodCallExpression.Object, method);

            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();
                typeArgs = new string[genericArguments.Length];
                for (var i = 0; i < typeArgs.Length; i++)
                    typeArgs[i] = genericArguments[i].AssemblyQualifiedName;
            }

            return new MethodCallExpressionNode(target, method.Name, args, typeArgs);
        }

        #endregion
    }
}