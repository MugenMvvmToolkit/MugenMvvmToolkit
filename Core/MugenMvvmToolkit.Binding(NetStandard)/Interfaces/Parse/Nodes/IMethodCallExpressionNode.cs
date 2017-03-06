#region Copyright

// ****************************************************************************
// <copyright file="IMethodCallExpressionNode.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    public interface IMethodCallExpressionNode : IExpressionNode
    {
        [NotNull]
        IList<string> TypeArgs { get; }

        [NotNull]
        string Method { get; }

        [CanBeNull]
        IExpressionNode Target { get; }

        [NotNull]
        IList<IExpressionNode> Arguments { get; }
    }
}
