#region Copyright

// ****************************************************************************
// <copyright file="ExpressionCounterVisitor.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

        #region Properties

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of IExpressionVisitor

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            _count++;
            return node;
        }

        #endregion
    }
}
