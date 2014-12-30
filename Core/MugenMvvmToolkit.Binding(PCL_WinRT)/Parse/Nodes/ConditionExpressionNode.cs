#region Copyright

// ****************************************************************************
// <copyright file="ConditionExpressionNode.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    /// <summary>
    ///     Represents an expression that has a conditional operator.
    /// </summary>
    public class ConditionExpressionNode : ExpressionNode, IConditionExpressionNode
    {
        #region Fields

        private IExpressionNode _condition;
        private IExpressionNode _ifFalse;
        private IExpressionNode _ifTrue;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConditionExpressionNode" /> class.
        /// </summary>
        public ConditionExpressionNode([NotNull] IExpressionNode condition, [NotNull] IExpressionNode ifTrue,
            [NotNull] IExpressionNode ifFalse)
            : base(ExpressionNodeType.Condition)
        {
            Should.NotBeNull(condition, "condition");
            Should.NotBeNull(ifTrue, "ifTrue");
            Should.NotBeNull(ifFalse, "ifFalse");
            _condition = condition;
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
        }

        #endregion

        #region Implementation of IConditionExpressionNode

        /// <summary>
        ///     Gets the test of the conditional operation.
        /// </summary>
        public IExpressionNode Condition
        {
            get { return _condition; }
        }

        /// <summary>
        ///     Gets the expression to execute if the test evaluates to true.
        /// </summary>
        public IExpressionNode IfTrue
        {
            get { return _ifTrue; }
        }

        /// <summary>
        ///     Gets the expression to execute if the test evaluates to false.
        /// </summary>
        public IExpressionNode IfFalse
        {
            get { return _ifFalse; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _condition = AcceptWithCheck(visitor, Condition, true);
            _ifTrue = AcceptWithCheck(visitor, IfTrue, true);
            _ifFalse = AcceptWithCheck(visitor, IfFalse, true);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new ConditionExpressionNode(Condition.Clone(), IfTrue.Clone(), IfFalse.Clone());
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("if ({0}) {{{1}}} else {{{2}}}", Condition, IfTrue, IfFalse);
        }

        #endregion
    }
}