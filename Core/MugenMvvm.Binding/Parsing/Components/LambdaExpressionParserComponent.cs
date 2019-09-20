using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class LambdaExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, IParameterExpression> _currentParameters;

        #endregion

        #region Constructors

        public LambdaExpressionParserComponent()
        {
            _currentParameters = new Dictionary<string, IParameterExpression>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Lambda;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
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

                args = stringArgs.ToArray(s => (IParameterExpression)new ParameterExpression(s));
            }


            if (!context.SkipWhitespaces().IsToken("=>"))
                return null;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var parameter = args[i];
                    if (_currentParameters.ContainsKey(parameter.Name))
                        throw new Exception();//todo add dups

                    _currentParameters[parameter.Name] = parameter;
                }

                var node = context.MoveNext(2).TryParseWhileNotNull(null, metadata);
                if (node == null)
                    return null;
                return new LambdaExpressionNode(node, args);
            }
            finally
            {
                for (int i = 0; i < args.Length; i++)
                    _currentParameters.Remove(args[i].Name);
            }
        }

        #endregion
    }
}