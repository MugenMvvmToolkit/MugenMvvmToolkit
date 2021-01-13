using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class ParsingComponentExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IExpressionNode? TryConvert<TExpression>(this ItemOrArray<IExpressionConverterComponent<TExpression>> components, IExpressionConverterContext<TExpression> context, TExpression expression)
            where TExpression : class
        {
            Should.NotBeNull(expression, nameof(expression));
            foreach (var c in components)
            {
                var r = c.TryConvert(context, expression);
                if (r != null)
                    return r;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<ExpressionParserResult> TryParse(this ItemOrArray<IExpressionParserComponent> components, IExpressionParser parser, object expression,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(parser, nameof(parser));
            Should.NotBeNull(expression, nameof(expression));
            foreach (var c in components)
            {
                var result = c.TryParse(parser, expression, metadata);
                if (!result.IsEmpty)
                    return result;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IExpressionNode? TryParse(this ItemOrArray<ITokenParserComponent> components, ITokenParserContext context, IExpressionNode? expression,
            Func<ITokenParserContext, ITokenParserComponent, bool>? condition)
        {
            Should.NotBeNull(context, nameof(context));
            foreach (var component in components)
            {
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