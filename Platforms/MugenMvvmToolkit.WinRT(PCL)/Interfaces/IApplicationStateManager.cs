﻿#region Copyright

// ****************************************************************************
// <copyright file="IApplicationStateManager.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
#if WINDOWS_PHONE
using System.Windows;

namespace MugenMvvmToolkit.WinPhone.Interfaces
#else
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.WinRT.Interfaces
#endif
{
    /// <summary>
    ///     Represents the application state manager.
    /// </summary>
    public interface IApplicationStateManager
    {
        /// <summary>
        ///     Gets the collection of known types.
        /// </summary>
        IList<Type> KnownTypes { get; }

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        void OnSaveState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state, object args,
            IDataContext context = null);

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        void OnLoadState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state, object args,
            IDataContext context = null);
    }
}