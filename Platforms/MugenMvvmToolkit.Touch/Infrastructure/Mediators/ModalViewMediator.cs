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
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class ModalViewMediator : WindowViewMediatorBase<IModalView>
    {
        #region Fields

        private static readonly NSAction NodoAction;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static ModalViewMediator()
        {
            NodoAction = () => { };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModalViewMediator" /> class.
        /// </summary>
        public ModalViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager operationCallbackManager,
             [NotNull] IViewMappingProvider viewMappingProvider,
             [NotNull] IViewModelProvider viewModelProvider)
            : base(viewModel, threadManager, viewManager, wrapperManager, operationCallbackManager)
        {
            Should.NotBeNull(viewMappingProvider, "viewMappingProvider");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            _viewMappingProvider = viewMappingProvider;
            _viewModelProvider = viewModelProvider;
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        #endregion

        #region Methods

        protected virtual INavigationProvider CreateNavigationProvider(INavigationService service)
        {
            return new NavigationProvider(service, ThreadManager, _viewMappingProvider, ViewManager, _viewModelProvider,
                OperationCallbackManager);
        }

        private void BindProvider(UINavigationController controller)
        {
            var navigationService = new NavigationService(controller);
            ViewModel.IocContainer.Unbind<INavigationService>();
            ViewModel.IocContainer.BindToConstant(navigationService);

            INavigationProvider service = CreateNavigationProvider(navigationService);
            ViewModel.IocContainer.Unbind<INavigationProvider>();
            ViewModel.IocContainer.BindToConstant(service);
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IModalView>

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected override void ShowView(IModalView view, bool isDialog, IDataContext context)
        {
            var parentController = (UIViewController)ViewModel
                .GetIocContainer(true)
                .Get<INavigationService>()
                .CurrentContent;
            var toShow = ToolkitExtensions.GetUnderlyingView<UIViewController>(view);
            if (view is IModalNavSupportView)
            {
                var nav = new MvvmNavigationController();
                nav.PushViewController(toShow, UseAnimations);
                toShow = nav;
                BindProvider(nav);
            }
            parentController.PresentViewController(toShow, UseAnimations, NodoAction);
            BindingExtensions.AttachedParentMember.Raise(toShow, EventArgs.Empty);
        }

        /// <summary>
        ///     Initializes the specified view.
        /// </summary>
        protected override void InitializeView(IModalView view, IDataContext context)
        {
        }

        /// <summary>
        ///     Clears the event subscribtions from the specified view.
        /// </summary>
        /// <param name="view">The specified window-view to dispose.</param>
        protected override void CleanupView(IModalView view)
        {
        }

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected override void CloseView(IModalView view)
        {
            var controller = ToolkitExtensions.GetUnderlyingView<UIViewController>(view);
            UIViewController presentedController = controller.PresentingViewController ??
                                                   controller.PresentedViewController;
            if (presentedController != null)
                presentedController.DismissViewController(UseAnimations, () =>
                {
                    OnViewClosed(view, EventArgs.Empty);
                    BindingExtensions.AttachedParentMember.Raise(controller, EventArgs.Empty);
                });
        }

        /// <summary>
        ///     Occured on update the current view using the <see cref="IWindowViewMediator.UpdateView" /> method.
        /// </summary>
        protected override void OnViewUpdated(IModalView view, IDataContext context)
        {
            if (!(view is IModalNavSupportView))
                return;
            var controller = ToolkitExtensions
                .GetUnderlyingView<UIViewController>(view)
                .NavigationController;
            if (controller != null)
                BindProvider(controller);
        }

        #endregion
    }
}