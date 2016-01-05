#region Copyright

// ****************************************************************************
// <copyright file="ViewModelRestoredEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelRestoredEventArgs : ViewModelRestoringEventArgs
    {
        #region Fields

        private readonly IViewModel _viewModel;

        #endregion

        #region Constructors

        public ViewModelRestoredEventArgs([NotNull]IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _viewModel = viewModel;
        }

        #endregion

        #region Properties

        [NotNull]
        public IViewModel ViewModel => _viewModel;

        #endregion
    }
}