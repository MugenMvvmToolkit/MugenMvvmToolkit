#region Copyright

// ****************************************************************************
// <copyright file="IExpressionNode.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    public interface IExpressionNode
    {
        [NotNull]
        ExpressionNodeType NodeType { get; }

        IExpressionNode Accept([NotNull] IExpressionVisitor visitor);

        [NotNull]
        IExpressionNode Clone();
    }
}
