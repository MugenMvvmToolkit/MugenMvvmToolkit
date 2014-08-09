#region Copyright
// ****************************************************************************
// <copyright file="ExpressionCounterVisitor.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

namespace MugenMvvmToolkit.Binding.Parse
{
    internal class ExpressionCounterVisitor : IExpressionVisitor
    {
        #region Fields

        private int _count;

        #endregion

        #region Methods

        public int GetCount([NotNull] IExpressionNode node)
        {
            _count = 0;
            node.Accept(this);
            return _count;
        }

        #endregion

        #region Implementation of IExpressionVisitor

        /// <summary>
        ///     Dispatches the expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            _count++;
            return node;
        }

        #endregion
    }
}