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
    public class ConditionExpressionNode : ExpressionNode, IConditionExpressionNode
    {
        #region Fields

        private IExpressionNode _condition;
        private IExpressionNode _ifFalse;
        private IExpressionNode _ifTrue;

        #endregion

        #region Constructors

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

        public IExpressionNode Condition
        {
            get { return _condition; }
        }

        public IExpressionNode IfTrue
        {
            get { return _ifTrue; }
        }

        public IExpressionNode IfFalse
        {
            get { return _ifFalse; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            _condition = AcceptWithCheck(visitor, Condition, true);
            _ifTrue = AcceptWithCheck(visitor, IfTrue, true);
            _ifFalse = AcceptWithCheck(visitor, IfFalse, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new ConditionExpressionNode(Condition.Clone(), IfTrue.Clone(), IfFalse.Clone());
        }

        public override string ToString()
        {
            return string.Format("if ({0}) {{{1}}} else {{{2}}}", Condition, IfTrue, IfFalse);
        }

        #endregion
    }
}
