#region Copyright
// ****************************************************************************
// <copyright file="IViewWrapper.cs">
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
using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represents the wrapper of view object.
    /// </summary>
    public interface IViewWrapper : IView
    {
        /// <summary>
        ///     Gets the underlying view type.
        /// </summary>
        [NotNull]
        Type ViewType { get; }

        /// <summary>
        ///     Gets the view object.
        /// </summary>
        [NotNull]
        object View { get; }
    }
}