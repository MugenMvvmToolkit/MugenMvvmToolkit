#region Copyright

// ****************************************************************************
// <copyright file="IConditionExpressionNode.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    public interface IConditionExpressionNode : IExpressionNode
    {
        [NotNull]
        IExpressionNode Condition { get; }

        [NotNull]
        IExpressionNode IfTrue { get; }

        [NotNull]
        IExpressionNode IfFalse { get; }
    }
}
