#region Copyright

// ****************************************************************************
// <copyright file="ViewModelProviderMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewModelProviderMock : IViewModelProvider
    {
        #region Proeprties

        public Func<GetViewModelDelegate<IViewModel>, IDataContext, IViewModel> GetViewModel { get; set; }

        public Func<Type, IDataContext, IViewModel> GetViewModelType { get; set; }

        public Action<IViewModel, IDataContext> InitializeViewModel { get; set; }

        #endregion

        #region Implementation of IViewModelProvider

        IViewModel IViewModelProvider.GetViewModel(GetViewModelDelegate<IViewModel> getViewModel,
            IDataContext dataContext)
        {
            return GetViewModel(getViewModel, dataContext);
        }

        IViewModel IViewModelProvider.GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            return GetViewModelType(viewModelType, dataContext);
        }

        void IViewModelProvider.InitializeViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            InitializeViewModel(viewModel, dataContext);
        }

        public IDataContext PreserveViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            throw new NotImplementedException();
        }

        public IViewModel RestoreViewModel(IDataContext viewModelState, IDataContext dataContext, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        public IViewModel TryGetViewModelById(Guid idViewModel)
        {
            throw new NotImplementedException();
        }

        public IList<IViewModel> GetCreatedViewModels(IDataContext dataContext = null)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initializing;
        public event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initialized;
        public event EventHandler<IViewModelProvider, ViewModelPreservingEventArgs> Preserving;
        public event EventHandler<IViewModelProvider, ViewModelPreservedEventArgs> Preserved;
        public event EventHandler<IViewModelProvider, ViewModelRestoringEventArgs> Restoring;
        public event EventHandler<IViewModelProvider, ViewModelRestoredEventArgs> Restored;

        #endregion
    }
}
