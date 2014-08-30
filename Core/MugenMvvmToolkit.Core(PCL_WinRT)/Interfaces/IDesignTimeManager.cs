#region Copyright
// ****************************************************************************
// <copyright file="IDesignTimeManager.cs">
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
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface for the design time manager.
    /// </summary>
    public interface IDesignTimeManager : IDisposable
    {
        /// <summary>
        ///     Gets the value indicating whether the control is in design mode (running under Blend or Visual Studio).
        /// </summary>
        bool IsDesignMode { get; }

        /// <summary>
        ///     Gets the load-priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        PlatformInfo Platform { get; }

        /// <summary>
        ///     Gets the design time <see cref="IocContainer" />, if any.
        /// </summary>
        [CanBeNull]
        IIocContainer IocContainer { get; }

        /// <summary>
        ///     Gets the design context.
        /// </summary>
        [CanBeNull]
        IDataContext Context { get; }

        /// <summary>
        ///     Initializes the current design time manager.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Initializes the view model in design mode.
        /// </summary>
        void InitializeViewModel([NotNull] IViewModel viewModel);
    }
}