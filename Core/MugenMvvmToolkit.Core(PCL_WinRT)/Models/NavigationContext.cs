#region Copyright

// ****************************************************************************
// <copyright file="NavigationContext.cs">
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
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the base navigation context implementation.
    /// </summary>
    public class NavigationContext : DataContext, INavigationContext
    {
        #region Fields

        private readonly NavigationMode _navigationMode;
        private readonly object _navigationProvider;
        private readonly NavigationType _type;
        private readonly IViewModel _viewModelFrom;
        private readonly IViewModel _viewModelTo;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationContext" /> class.
        /// </summary>
        public NavigationContext([NotNull] NavigationType type, NavigationMode navigationMode, [CanBeNull] IViewModel viewModelFrom, [CanBeNull] IViewModel viewModelTo,
             [CanBeNull] object navigationProvider, [CanBeNull] IDataContext parameters = null)
        {
            Should.NotBeNull(type, "type");
            _type = type;
            _navigationMode = navigationMode;
            _navigationProvider = navigationProvider;
            _viewModelFrom = viewModelFrom;
            _viewModelTo = viewModelTo;
            if (parameters != null)
                Merge(parameters);
        }

        #endregion

        #region Implementation of INavigationContext

        /// <summary>
        ///     Gets the value of the mode parameter from the originating Navigate call.
        /// </summary>
        public NavigationMode NavigationMode
        {
            get { return _navigationMode; }
        }

        /// <summary>
        /// Gets the previously navigate view model.
        /// </summary>
        public IViewModel ViewModelFrom
        {
            get { return _viewModelFrom; }
        }

        /// <summary>
        ///     Gets the view model to navigate.
        /// </summary>
        public IViewModel ViewModelTo
        {
            get { return _viewModelTo; }
        }

        /// <summary>
        ///     Gets the navigation type.
        /// </summary>
        public NavigationType NavigationType
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the navigation provider that creates this context.
        /// </summary>
        public object NavigationProvider
        {
            get { return _navigationProvider; }
        }

        #endregion
    }
}