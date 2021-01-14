using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class NewArrayExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; set; } = ParsingComponentPriority.Convert;

        public static T[] NewArrayInit<T>(params T[] items) => items;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is NewArrayExpression newArrayExpression) || newArrayExpression.NodeType != ExpressionType.NewArrayInit)
                return null;

            return new MethodCallExpressionNode(ConstantExpressionNode.Get<NewArrayExpressionConverter>(), nameof(NewArrayInit),
                context.Convert(new ItemOrIReadOnlyList<Expression>(newArrayExpression.Expressions)), expression.Type.GetElementType()!.AssemblyQualifiedName!);
        }
    }
}