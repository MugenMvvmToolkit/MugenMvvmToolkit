#region Copyright

// ****************************************************************************
// <copyright file="IContentViewManager.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the interface that allows to set content in the specified view.
    /// </summary>
    public interface IContentViewManager
    {
        /// <summary>
        ///     Sets the specified content.
        /// </summary>
        void SetContent([NotNull] object view, [CanBeNull] object content);
    }
}