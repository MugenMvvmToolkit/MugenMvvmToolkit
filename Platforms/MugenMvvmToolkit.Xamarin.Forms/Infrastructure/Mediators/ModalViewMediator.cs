#region Copyright
// ****************************************************************************
// <copyright file="ModalViewMediator.cs">
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
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.ViewModels;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class ModalViewMediator : WindowViewMediatorBase<IModalView>
    {
        #region Fields

        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModalViewMediator" /> class.
        /// </summary>
        public ModalViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IOperationCallbackManager operationCallbackManager,
            [NotNull] IViewMappingProvider viewMappingProvider,
            [NotNull] IViewModelProvider viewModelProvider)
            : base(viewModel, threadManager, viewManager, operationCallbackManager)
        {
            Should.NotBeNull(viewMappingProvider, "viewMappingProvider");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            _viewMappingProvider = viewMappingProvider;
            _viewModelProvider = viewModelProvider;
        }

        #endregion

        #region Methods

        protected virtual INavigationProvider CreateNavigationProvider(INavigationService service)
        {
            return new NavigationProvider(service, ThreadManager, _viewMappingProvider, ViewManager, _viewModelProvider,
                OperationCallbackManager);
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IModalView>

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected override void ShowView(IModalView view, bool isDialog, IDataContext context)
        {
            var page = (Page)ViewModel
                .GetIocContainer(true)
                .Get<INavigationService>()
                .CurrentContent;
            page.Navigation.PushModalAsync((Page)view);
        }

        /// <summary>
        ///     Initializes the specified view.
        /// </summary>
        protected override void InitializeView(IModalView view, IDataContext context)
        {
            ((Page)view).Disappearing += OnViewClosed;
        }

        /// <summary>
        ///     Clears the event subscribtions from the specified view.
        /// </summary>
        /// <param name="view">The specified window-view to dispose.</param>
        protected override void CleanupView(IModalView view)
        {
            ((Page)view).Disappearing -= OnViewClosed;
        }

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected override void CloseView(IModalView view)
        {
            var page = (Page)view.GetUnderlyingView();
            page.Navigation.PopModalAsync();
        }

        #endregion
    }
}