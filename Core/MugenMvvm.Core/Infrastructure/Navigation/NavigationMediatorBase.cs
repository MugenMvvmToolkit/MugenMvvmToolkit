using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Navigation
{
//    public abstract class NavigationMediatorBase<TView> : INavigationMediator, IApplicationStateAwareNavigationProvider
//        where TView : class
//    {
//        #region Fields
//
//        private CancelEventArgs _cancelArgs;
//        private INavigationContext? _closingContext;
//        private INavigationContext? _showingContext;
//        private bool _shouldClose;
//
//        #endregion
//
//        #region Properties
//
//        protected IViewManager ViewManager { get; }
//
//        protected INavigationDispatcher NavigationDispatcher { get; }
//
//        protected IThreadDispatcher ThreadDispatcher { get; }
//
//        protected IWrapperManager WrapperManager { get; }
//
//        public virtual NavigationType NavigationType => NavigationType.Generic;
//
//        public bool IsOpen { get; private set; }
//
//        object? INavigationMediator.View => View;
//
//        public TView View { get; private set; }
//
//        public IViewModelBase ViewModel { get; private set; }
//
//        protected bool IsClosing => _closingContext != null;
//
//        #endregion
//    }
}