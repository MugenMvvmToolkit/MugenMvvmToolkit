using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class LambdaTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, IParameterExpression> _currentParameters;

        #endregion

        #region Constructors

        public LambdaTokenParserComponent()
        {
            _currentParameters = new Dictionary<string, IParameterExpression>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Lambda;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            context.SkipWhitespaces();
            if (_currentParameters.Count != 0)
            {
                if (context.IsIdentifier(out var end) && _currentParameters.TryGetValue(context.GetValue(context.Position, end), out var value))
                {
                    context.SetPosition(end);
                    return value;
                }
            }


            IParameterExpression[] args;
            if (context.IsToken("()"))
            {
                args = Default.EmptyArray<IParameterExpression>();
                context.MoveNext(2);
            }
            else
            {
                if (!context.IsToken('('))
                    return null;

                var stringArgs = context.MoveNext().SkipWhitespaces().ParseStringArguments(")", false);
                if (stringArgs == null)
                    return null;

                args = stringArgs.ToArray(s => (IParameterExpression) new ParameterExpression(s));
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