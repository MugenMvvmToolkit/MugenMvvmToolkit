#region Copyright

// ****************************************************************************
// <copyright file="ModalViewMediator.cs">
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.Views;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Mediators
{
    public class ModalViewMediator : WindowViewMediatorBase<IModalView>
    {
        #region Fields

        private static readonly Action NodoAction;
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
            var toShow = view.GetUnderlyingView<UIViewController>();
            bool animated;
            if (context.TryGetData(NavigationConstants.UseAnimations, out animated))
                ViewModel.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
            else
                animated = UseAnimations;
            if (view is IModalNavSupportView)
            {
                var nav = new MvvmNavigationController();
                nav.PushViewController(toShow, animated);
                toShow = nav;
                BindProvider(nav);
            }
            parentController.PresentViewController(toShow, animated, NodoAction);
            toShow.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
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
            var controller = view.GetUnderlyingView<UIViewController>();
            UIViewController presentedController = controller.PresentingViewController ??
                                                   controller.PresentedViewController;
            bool animated;
            if (ViewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out animated))
                ViewModel.Settings.State.Remove(NavigationConstants.UseAnimations);
            else
                animated = UseAnimations;
            if (presentedController != null)
                presentedController.DismissViewController(animated, () =>
                {
                    OnViewClosed(view, EventArgs.Empty);
                    controller.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
                });
        }

        /// <summary>
        ///     Occured on update the current view using the <see cref="IWindowViewMediator.UpdateView" /> method.
        /// </summary>
        protected override void OnViewUpdated(IModalView view, IDataContext context)
        {
            if (!(view is IModalNavSupportView))
                return;
            var controller = view.GetUnderlyingView<UIViewController>().NavigationController;
            if (controller != null)
                BindProvider(controller);
        }

        #endregion
    }
}