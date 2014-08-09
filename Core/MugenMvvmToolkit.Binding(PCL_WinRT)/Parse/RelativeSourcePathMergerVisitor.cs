#region Copyright
// ****************************************************************************
// <copyright file="RelativeSourcePathMergerVisitor.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse
{
    /// <summary>
    ///     Represents the expression visitor that allows to merge relative source path from {Relative ControlType}.Test.Value to {Relative ControlType, Path=Test.Value}
    /// </summary>
    public class RelativeSourcePathMergerVisitor : IExpressionVisitor
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="RelativeSourcePathMergerVisitor" />.
        /// </summary>
        public static RelativeSourcePathMergerVisitor Instance = new RelativeSourcePathMergerVisitor();

        #endregion

        #region Constructors

        private RelativeSourcePathMergerVisitor()
        {
        }

        #endregion

        #region Implementation of IExpressionVisitor

        /// <summary>
        ///     Dispatches the expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public IExpressionNode Visit(IExpressionNode node)
        {
            var nodes = new List<IExpressionNode>();
            var members = new List<string>();
            string memberName = node.TryGetMemberName(true, true, nodes, members);
            if (memberName == null)
            {
                var relativeExp = nodes[0] as IRelativeSourceExpressionNode;
                if (relativeExp != null)
                {
                    relativeExp.MergePath(string.Join(".", members));
                    return relativeExp;
                }
            }
            return node;
        }

        #endregion
    }
}