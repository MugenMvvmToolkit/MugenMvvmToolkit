#region Copyright

// ****************************************************************************
// <copyright file="ModalViewMediator.cs">
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
using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Mediators
{
    public class ModalViewMediator : WindowViewMediatorBase<IModalView>
    {
        #region Fields

        private readonly EventHandler<ModalPoppedEventArgs> _closedHandler;
        private readonly EventHandler<Page, CancelEventArgs> _backButtonHandler;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ModalViewMediator([NotNull] IThreadManager threadManager, [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager,
            [NotNull] INavigationDispatcher navigationDispatcher, [NotNull] IEventAggregator eventAggregator)
            : base(threadManager, viewManager, wrapperManager, navigationDispatcher, eventAggregator)
        {
            _backButtonHandler = ReflectionExtensions
                .CreateWeakDelegate<ModalViewMediator, CancelEventArgs, EventHandler<Page, CancelEventArgs>>(this,
                    (service, o, arg3) => service.OnBackButtonPressed((Page)o, arg3),
                    (o, handler) => XamarinFormsToolkitExtensions.BackButtonPressed -= handler, handler => handler.Handle);
            _closedHandler = ReflectionExtensions
                .CreateWeakDelegate<ModalViewMediator, ModalPoppedEventArgs, EventHandler<ModalPoppedEventArgs>>(this,
                    (mediator, o, arg3) => mediator.OnModalClosed(arg3), (o, handler) => Application.Current.ModalPopped -= handler, handler => handler.Handle);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        #endregion

        #region Methods

        private void OnBackButtonPressed(Page page, CancelEventArgs args)
        {
            if (View.GetUnderlyingView<object>() == page)
                OnViewClosing(page, args);
        }

        private void OnModalClosed(ModalPoppedEventArgs args)
        {
            if (View.GetUnderlyingView<object>() == args.Modal)
                OnViewClosed(args.Modal, args);
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IModalView>

        protected override void ShowView(IModalView view, bool isDialog, IDataContext context)
        {
            var page = (Page)ViewModel
                .GetIocContainer(true)
                .Get<INavigationService>()
                .CurrentContent;
            bool animated;
            if (context.TryGetData(NavigationConstants.UseAnimations, out animated))
                ViewModel.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
            else
                animated = UseAnimations;
            page.Navigation.PushModalAsync(view.GetUnderlyingView<Page>(), animated);
        }

        protected override bool ActivateView(IModalView view, IDataContext context)
        {
            var supportActivationModalView = view as ISupportActivationModalView;
            if (supportActivationModalView == null)
                return false;
            return supportActivationModalView.Activate();
        }

        protected override void InitializeView(IModalView view, IDataContext context)
        {
            XamarinFormsToolkitExtensions.BackButtonPressed += _backButtonHandler;
            Application.Current.ModalPopped += _closedHandler;
        }

        protected override void CleanupView(IModalView view)
        {
            XamarinFormsToolkitExtensions.BackButtonPressed -= _backButtonHandler;
            Application.Current.ModalPopped -= _closedHandler;
        }

        protected override void CloseView(IModalView view)
        {
            var page = view.GetUnderlyingView<Page>();
            bool animated;
            if (ViewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out animated))
                ViewModel.Settings.State.Remove(NavigationConstants.UseAnimations);
            else
                animated = UseAnimations;
            page.Navigation.PopModalAsync(animated);
        }

        #endregion
    }
}
