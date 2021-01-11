using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class LambdaTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, IParameterExpressionNode> _currentParameters;

        #endregion

        #region Constructors

        public LambdaTokenParser()
        {
            _currentParameters = new Dictionary<string, IParameterExpressionNode>(StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Lambda;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            context.SkipWhitespaces();
            if (_currentParameters.Count != 0)
            {
                if (context.IsIdentifier(out var end) && _currentParameters.TryGetValue(context.GetValue(context.Position, end), out var value))
                {
                    context.Position = end;
                    return value;
                }
            }

            ItemOrArray<IParameterExpressionNode> args;
            if (context.IsToken("()"))
            {
                args = default;
                context.MoveNext(2);
            }
            else if (context.IsToken('('))
            {
                var stringArgs = context.MoveNext().SkipWhitespaces().ParseStringArguments(")", false);
                if (stringArgs.IsEmpty)
                    return null;

                if (stringArgs.Item == null)
                {
                    var items = new IParameterExpressionNode[stringArgs.Count];
                    for (var i = 0; i < items.Length; i++)
                        items[i] = new ParameterExpressionNode(stringArgs[i]);
                    args = items;
                }
                else
                    args = new ParameterExpressionNode(stringArgs[0]);
            }
            else
            {
                if (!context.IsIdentifier(out var end))
                    return null;

                var position = context.SkipWhitespacesPosition(end);
                if (!context.IsToken("=>", position))
                {
                    context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseLambdaExpressionExpectedTokenFormat1.Format(context.GetValue(context.Position, end)));
                    return null;
                }

                args = new IParameterExpressionNode[] {new ParameterExpressionNode(context.GetValue(context.Position, end))};
                context.Position = position;
            }


            if (!context.SkipWhitespaces().IsToken("=>"))
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseLambdaExpressionExpectedTokenFormat1.Format(string.Join<IExpressionNode>(",", args.AsList())));
                return null;
            }

            try
            {
                foreach (var parameter in args)
                {
                    if (_currentParameters.ContainsKey(parameter.Name))
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.DuplicateLambdaParameterFormat1.Format(parameter.Name));
                        return null;
                    }

                    _currentParameters[parameter.Name] = parameter;
                }

                var body = context.MoveNext(2).TryParseWhileNotNull();
                if (body == null)
                {
                    context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseLambdaExpressionExpectedExpressionFormat1.Format(string.Join<IExpressionNode>(",", args.AsList())));
                    return null;
                }

                return new LambdaExpressionNode(body, args);
            }
            finally
            {
                foreach (var arg in args)
                    _currentParameters.Remove(arg.Name);
            }
        }

        #endregion
    }
}