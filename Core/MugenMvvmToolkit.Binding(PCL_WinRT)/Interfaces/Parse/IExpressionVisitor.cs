#region Copyright

// ****************************************************************************
// <copyright file="IExpressionVisitor.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    public interface IExpressionVisitor
    {
        bool IsPostOrder { get; }

        IExpressionNode Visit([NotNull] IExpressionNode node);
    }
}
