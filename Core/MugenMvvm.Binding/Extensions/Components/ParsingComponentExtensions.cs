using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ParsingComponentExtensions
    {
        #region Methods

        public static IExpressionNode? TryConvert<TExpression>(this IExpressionConverterParserComponent<TExpression>[] components, IExpressionConverterContext<TExpression> context, TExpression expression)
            where TExpression : class
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var r = components[i].TryConvert(context, expression);
                if (r != null)
                    return r;
            }

            return null;
        }

        public static ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse<TExpression>(this IExpressionParserComponent[] components, in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryParse(expression, metadata);
                if (!result.Item.IsEmpty || result.List != null)
                    return result;
            }

            return default;
        }

        public static IExpressionNode? TryParse(this ITokenParserComponent[] components, ITokenParserContext context, IExpressionNode? expression, Func<ITokenParserContext, ITokenParserComponent, bool>? condition)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (condition != null && !condition(context, component))
                    continue;

                var result = component.TryParse(context, expression);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion
    }
}