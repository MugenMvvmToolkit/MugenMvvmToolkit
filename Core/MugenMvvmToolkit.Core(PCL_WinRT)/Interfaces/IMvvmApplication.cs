#region Copyright

// ****************************************************************************
// <copyright file="IMvvmApplication.cs">
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
using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the base interface that is used to start MVVM application.
    /// </summary>
    public interface IMvvmApplication
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        [NotNull]
        PlatformInfo Platform { get; }

        /// <summary>
        ///     Gets or sets the load mode of current <see cref="IMvvmApplication" />.
        /// </summary>
        LoadMode Mode { get; }

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        IIocContainer IocContainer { get; }

        /// <summary>
        ///     Gets the current application context.
        /// </summary>
        IDataContext Context { get; }

        /// <summary>
        ///     Gets the default view model settings.
        /// </summary>
        IViewModelSettings ViewModelSettings { get; }

        /// <summary>
        ///     Initializes the current application.
        /// </summary>
        void Initialize(PlatformInfo platform, IIocContainer iocContainer, IList<Assembly> assemblies, IDataContext context);

        /// <summary>
        ///     Gets the type of start view model.
        /// </summary>
        [NotNull]
        Type GetStartViewModelType();
    }
}