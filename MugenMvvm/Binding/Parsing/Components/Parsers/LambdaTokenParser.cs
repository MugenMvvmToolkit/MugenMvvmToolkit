using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components.Parsers
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


            IParameterExpressionNode[] args;
            if (context.IsToken("()"))
            {
                args = Default.Array<IParameterExpressionNode>();
                context.MoveNext(2);
            }
            else if (context.IsToken('('))
            {
                var stringArgs = context.MoveNext().SkipWhitespaces().ParseStringArguments(")", false);
                if (stringArgs == null)
                    return null;

                args = new IParameterExpressionNode[stringArgs.Length];
                for (int i = 0; i < args.Length; i++)
                    args[i] = new ParameterExpressionNode(stringArgs[i]);
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

                args = new IParameterExpressionNode[] { new ParameterExpressionNode(context.GetValue(context.Position, end)) };
                context.Position = position;
            }


            if (!context.SkipWhitespaces().IsToken("=>"))
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseLambdaExpressionExpectedTokenFormat1.Format(string.Join<IExpressionNode>(",", args)));
                return null;
            }

            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var parameter = args[i];
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
                    context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseLambdaExpressionExpectedExpressionFormat1.Format(string.Join<IExpressionNode>(",", args)));
                    return null;
                }
                return new LambdaExpressionNode(body, args);
            }
            finally
            {
                for (var i = 0; i < args.Length; i++)
                    _currentParameters.Remove(args[i].Name);
            }
        }

        #endregion
    }
}