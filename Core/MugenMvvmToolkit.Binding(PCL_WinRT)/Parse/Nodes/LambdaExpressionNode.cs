#region Copyright

// ****************************************************************************
// <copyright file="LambdaExpressionNode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class LambdaExpressionNode : ExpressionNode, ILambdaExpressionNode
    {
        #region Fields

        private readonly IList<string> _parameters;
        private IExpressionNode _expression;

        #endregion

        #region Constructors

        public LambdaExpressionNode([NotNull] IExpressionNode expression, [CanBeNull] IEnumerable<string> parameters)
            : base(ExpressionNodeType.Lambda)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expression = expression;
            _parameters = parameters == null
                ? Empty.Array<string>()
                : parameters.ToArray();
            BindingExtensions.CheckDuplicateLambdaParameter(Parameters);
        }

        #endregion

        #region Implementation of ILambdaExpressionNode

        public IList<string> Parameters
        {
            get { return _parameters; }
        }

        public IExpressionNode Expression
        {
            get { return _expression; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _expression = AcceptWithCheck(visitor, Expression, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new LambdaExpressionNode(Expression.Clone(), Parameters);
        }

        public override string ToString()
        {
            if (Parameters.Count == 0)
                return "() => " + Expression;
            return $"({string.Join(", ", Parameters)}) => {Expression}";
        }

        #endregion
    }
}
