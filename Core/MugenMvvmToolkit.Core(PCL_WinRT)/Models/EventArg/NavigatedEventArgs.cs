#region Copyright
// ****************************************************************************
// <copyright file="NavigatedEventArgs.cs">
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
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class NavigatedEventArgs : EventArgs
    {
        #region Fields

        private readonly INavigationContext _context;
        private readonly IViewModel _viewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes the <see cref="NavigatedEventArgs" />.
        /// </summary>
        public NavigatedEventArgs([NotNull]INavigationContext context, [NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(context, "context");
            Should.NotBeNull(viewModel, "viewModel");
            _context = context;
            _viewModel = viewModel;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="INavigationContext" />.
        /// </summary>
        [NotNull]
        public INavigationContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Gets the navigated <see cref="IViewModel" />.
        /// </summary>
        [NotNull]
        public IViewModel ViewModel
        {
            get { return _viewModel; }
        }

        #endregion
    }
}