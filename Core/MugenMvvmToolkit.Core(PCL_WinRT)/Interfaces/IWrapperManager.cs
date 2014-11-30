#region Copyright
// ****************************************************************************
// <copyright file="IWrapperManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface that allows to wrap an object to another object.
    /// </summary>
    public interface IWrapperManager
    {
        /// <summary>
        ///     Determines whether the specified view can be wrapped to wrapper type.
        /// </summary>
        bool CanWrap([NotNull] Type type, [NotNull] Type wrapperType, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Wraps the specified view object to the wrapper type.
        /// </summary>
        [NotNull]
        object Wrap([NotNull] object item, [NotNull] Type wrapperType, [CanBeNull] IDataContext dataContext);
    }
}