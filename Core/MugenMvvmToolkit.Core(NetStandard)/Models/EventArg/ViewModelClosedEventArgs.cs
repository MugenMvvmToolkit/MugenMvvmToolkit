#region Copyright

// ****************************************************************************
// <copyright file="ViewModelClosedEventArgs.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

        public ViewModelClosedEventArgs([NotNull] IViewModel viewModel, [CanBeNull] object parameter)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _viewModel = viewModel;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        public IViewModel ViewModel => _viewModel;

        [CanBeNull]
        public object Parameter => _parameter;

        #endregion
    }
}
