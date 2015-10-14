#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPreservingEventArgs.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelPreservingEventArgs : EventArgs
    {
        #region Fields

        private readonly IViewModel _viewModel;

        #endregion

        #region Constructors

        public ViewModelPreservingEventArgs([NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, "viewModel");
            _viewModel = viewModel;
        }

        #endregion

        #region Properties

        [NotNull]
        public IViewModel ViewModel
        {
            get { return _viewModel; }
        }

        public IDataContext Context { get; set; }

        #endregion
    }
}