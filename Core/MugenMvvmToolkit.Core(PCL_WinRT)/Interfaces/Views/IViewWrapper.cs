#region Copyright

// ****************************************************************************
// <copyright file="IViewWrapper.cs">
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

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represents the wrapper of view object.
    /// </summary>
    public interface IViewWrapper : IView
    {
        /// <summary>
        ///     Gets the view object.
        /// </summary>
        [NotNull]
        object View { get; }
    }
}