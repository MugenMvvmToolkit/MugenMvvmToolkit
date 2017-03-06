#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPresenterMock.cs">
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
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewModelPresenterMock : IViewModelPresenter
    {
        #region Properties

        public ICollection<IDynamicViewModelPresenter> DynamicPresenters { get; set; }

        public Func<IViewModel, IDataContext, IAsyncOperation> ShowAsync { get; set; }

        public Func<IViewModel, IDataContext, Task<bool>> CloseAsync { get; set; }

        public Action<IViewModel, IDataContext> Restore { get; set; }

        #endregion

        #region Implementation of interfaces

        IAsyncOperation IViewModelPresenter.ShowAsync(IViewModel viewModel, IDataContext context)
        {
            return ShowAsync?.Invoke(viewModel, context);
        }

        Task<bool> IViewModelPresenter.CloseAsync(IViewModel viewModel, IDataContext context)
        {
            return CloseAsync?.Invoke(viewModel, context);
        }

        void IViewModelPresenter.Restore(IViewModel viewModel, IDataContext context)
        {
            Restore?.Invoke(viewModel, context);
        }

        #endregion
    }
}