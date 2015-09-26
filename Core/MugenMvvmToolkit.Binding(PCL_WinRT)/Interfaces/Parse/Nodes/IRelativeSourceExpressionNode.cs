#region Copyright

// ****************************************************************************
// <copyright file="IRelativeSourceExpressionNode.cs">
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
    public interface IRelativeSourceExpressionNode : IExpressionNode
    {
        [NotNull]
        string Type { get; }

        [CanBeNull]
        string ElementName { get; }

        [CanBeNull]
        string Path { get; }

        uint Level { get; }

        void MergePath(string path);
    }
}
