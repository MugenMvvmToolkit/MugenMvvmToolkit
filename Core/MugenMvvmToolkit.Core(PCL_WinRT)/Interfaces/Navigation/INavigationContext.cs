#region Copyright

// ****************************************************************************
// <copyright file="INavigationContext.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    /// <summary>
    ///     Represents the navigation context.
    /// </summary>
    public interface INavigationContext : IDataContext
    {
        /// <summary>
        ///     Gets the value of the mode parameter from the originating Navigate call.
        /// </summary>
        NavigationMode NavigationMode { get; }

        /// <summary>
        ///     Gets the from navigate view model.
        /// </summary>
        [CanBeNull]
        IViewModel ViewModelFrom { get; }

        /// <summary>
        ///     Gets the view model to navigate.
        /// </summary>
        [CanBeNull]
        IViewModel ViewModelTo { get; }

        /// <summary>
        ///     Gets the navigation parameters.
        /// </summary>
        [NotNull]
        IDataContext Parameters { get; }

        /// <summary>
        ///     Gets the navigation type.
        /// </summary>
        NavigationType NavigationType { get; }

        /// <summary>
        ///     Gets the navigation provider that creates this context.
        /// </summary>
        [CanBeNull]
        object NavigationProvider { get; }
    }
}