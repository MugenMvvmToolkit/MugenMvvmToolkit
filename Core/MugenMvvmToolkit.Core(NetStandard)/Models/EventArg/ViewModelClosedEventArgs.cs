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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelClosedEventArgs : EventArgs
    {
        #region Fields

        private readonly IDataContext _context;
        private readonly IViewModel _viewModel;

        #endregion

        #region Constructors

        public ViewModelClosedEventArgs([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _viewModel = viewModel;
            _context = context;
        }

        #endregion

        #region Properties

        public IViewModel ViewModel => _viewModel;

        [CanBeNull]
        public IDataContext Context => _context;

        #endregion
    }
}
