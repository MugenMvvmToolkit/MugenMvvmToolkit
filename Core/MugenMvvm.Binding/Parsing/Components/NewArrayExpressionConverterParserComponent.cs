using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class NewArrayExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Convert;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterParserContext<Expression> context, Expression expression)
        {
            if (!(expression is NewArrayExpression newArrayExpression) || newArrayExpression.NodeType != ExpressionType.NewArrayInit)
                return null;

            return new MethodCallExpressionNode(ConstantExpressionNode.Get<NewArrayExpressionConverterParserComponent>(), nameof(NewArrayInit),
                context.Convert(newArrayExpression.Expressions), new[] {expression.Type.GetElementType()!.AssemblyQualifiedName});
        }

        #endregion

        #region Methods

        public static T[] NewArrayInit<T>(params T[] items)
        {
            return items;
        }

        #endregion
    }
}