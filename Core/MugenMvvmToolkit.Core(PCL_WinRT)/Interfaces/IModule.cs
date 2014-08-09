#region Copyright
// ****************************************************************************
// <copyright file="IModule.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface that is used to initialize MVVM application.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        ///     Gets the priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        bool Load([NotNull] IModuleContext context);

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        void Unload([NotNull] IModuleContext context);
    }
}