#region Copyright
// ****************************************************************************
// <copyright file="IModuleContext.cs">
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
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the module context.
    /// </summary>
    public interface IModuleContext
    {
        /// <summary>
        ///     Gets the <see cref="IIocContainer" />.
        /// </summary>
        [CanBeNull]
        IIocContainer IocContainer { get; }

        /// <summary>
        ///     Gets the <see cref="IDataContext" />.
        /// </summary>
        [NotNull]
        IDataContext Context { get; }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        [NotNull]
        IList<Assembly> Assemblies { get; }

        /// <summary>
        ///     Gets the module load mode.
        /// </summary>
        LoadMode Mode { get; }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        [NotNull]
        PlatformInfo Platform { get; }
    }
}