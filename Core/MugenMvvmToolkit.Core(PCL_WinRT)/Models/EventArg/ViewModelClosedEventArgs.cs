#region Copyright
// ****************************************************************************
// <copyright file="ViewModelClosedEventArgs.cs">
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
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelClosedEventArgs : EventArgs
    {
        #region Fields

        private readonly object _parameter;
        private readonly IViewModel _viewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelClosedEventArgs" /> class.
        /// </summary>
        public ViewModelClosedEventArgs([NotNull] IViewModel viewModel, [CanBeNull] object parameter)
        {
            Should.NotBeNull(viewModel, "viewModel");
            _viewModel = viewModel;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="IViewModel" />.
        /// </summary>
        public IViewModel ViewModel
        {
            get { return _viewModel; }
        }

        /// <summary>
        ///     The specified close parameter if any.
        /// </summary>
        [CanBeNull]
        public object Parameter
        {
            get { return _parameter; }
        }

        #endregion
    }
}