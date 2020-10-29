using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class ParsingComponentExtensions
    {
        #region Methods

        public static IExpressionNode? TryConvert<TExpression>(this IExpressionConverterComponent<TExpression>[] components, IExpressionConverterContext<TExpression> context, TExpression expression)
            where TExpression : class
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(expression, nameof(expression));
            for (var i = 0; i < components.Length; i++)
            {
                var r = components[i].TryConvert(context, expression);
                if (r != null)
                    return r;
            }

            return null;
        }

        public static ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(this IExpressionParserComponent[] components, IExpressionParser parser, object expression,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(parser, nameof(parser));
            Should.NotBeNull(expression, nameof(expression));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryParse(parser, expression, metadata);
                if (!result.Item.IsEmpty || result.List != null)
                    return result;
            }

            return default;
        }

        public static IExpressionNode? TryParse(this ITokenParserComponent[] components, ITokenParserContext context, IExpressionNode? expression, Func<ITokenParserContext, ITokenParserComponent, bool>? condition)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
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