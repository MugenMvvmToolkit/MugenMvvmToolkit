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
    /// <summary>
    ///     Represents an expression that has a lambda operator.
    /// </summary>
    public class LambdaExpressionNode : ExpressionNode, ILambdaExpressionNode
    {
        #region Fields

        private readonly IList<string> _parameters;
        private IExpressionNode _expression;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpressionNode" /> class.
        /// </summary>
        public LambdaExpressionNode([NotNull] IExpressionNode expression, [CanBeNull] IEnumerable<string> parameters)
            : base(ExpressionNodeType.Lambda)
        {
            Should.NotBeNull(expression, "expression");
            _expression = expression;
            _parameters = parameters == null
                ? Empty.Array<string>()
                : parameters.ToArray();
            BindingExtensions.CheckDuplicateLambdaParameter(Parameters);
        }

        #endregion

        #region Implementation of ILambdaExpressionNode

        /// <summary>
        ///     Gets the parameters of lambda expression.
        /// </summary>
        public IList<string> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        ///     Gets the lambda expression.
        /// </summary>
        public IExpressionNode Expression
        {
            get { return _expression; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _expression = AcceptWithCheck(visitor, Expression, true);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new LambdaExpressionNode(Expression.Clone(), Parameters);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (Parameters.Count == 0)
                return "() => " + Expression;
            return string.Format("({0}) => {1}", string.Join(", ", Parameters), Expression);
        }

        #endregion
    }
}