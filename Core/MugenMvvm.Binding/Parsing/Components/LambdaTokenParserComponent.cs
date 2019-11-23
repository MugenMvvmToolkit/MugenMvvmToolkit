using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class LambdaTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IParameterExpressionNode> _currentParameters;

        #endregion

        #region Constructors

        public LambdaTokenParserComponent()
        {
            _currentParameters = new StringOrdinalLightDictionary<IParameterExpressionNode>(3);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParserComponentPriority.Lambda;

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
                args = Default.EmptyArray<IParameterExpressionNode>();
                context.MoveNext(2);
            }
            else if (context.IsToken('('))
            {
                var stringArgs = context.MoveNext().SkipWhitespaces().ParseStringArguments(")", false);
                if (stringArgs == null)
                    return null;

                args = new IParameterExpressionNode[stringArgs.Count];
                for (int i = 0; i < args.Length; i++)
                    args[i] = new ParameterExpressionNode(stringArgs[i], i);
            }
            else
            {
                if (!context.IsIdentifier(out var end))
                    return null;

                var position = context.SkipWhitespacesPosition(end);
                if (!context.IsToken("=>", position))
                    return null;

                args = new IParameterExpressionNode[] { new ParameterExpressionNode(context.GetValue(context.Position, end), 0) };
                context.Position = position;
            }


            if (!context.SkipWhitespaces().IsToken("=>"))
                return null;

            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var parameter = args[i];
                    if (_currentParameters.ContainsKey(parameter.Name))
                        BindingExceptionManager.ThrowDuplicateLambdaParameter(parameter.Name);

                    _currentParameters[parameter.Name] = parameter;
                }

                var node = context.MoveNext(2).TryParseWhileNotNull();
                if (node == null)
                    return null;
                return new LambdaExpressionNode(node, args);
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