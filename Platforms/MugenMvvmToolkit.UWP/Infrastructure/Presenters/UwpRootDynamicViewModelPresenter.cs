#region Copyright

// ****************************************************************************
// <copyright file="UwpRootDynamicViewModelPresenter.cs">
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
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.UWP.Infrastructure.Presenters
{
    public class UwpRootDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public UwpRootDynamicViewModelPresenter(IViewModelProvider viewModelProvider, IViewManager viewManager, INavigationDispatcher navigationDispatcher,
            IOperationCallbackManager operationCallbackManager, IThreadManager threadManager)
        {
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(operationCallbackManager, nameof(operationCallbackManager));
            Should.NotBeNull(threadManager, nameof(threadManager));
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
            OperationCallbackManager = operationCallbackManager;
            ThreadManager = threadManager;
            ViewModelProvider = viewModelProvider;
        }

        #endregion

        #region Properties

        protected IViewManager ViewManager { get; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected IOperationCallbackManager OperationCallbackManager { get; }

        protected IThreadManager ThreadManager { get; }

        protected IViewModelProvider ViewModelProvider { get; }

        public int Priority => int.MaxValue;

        public Func<Frame> FrameFactory { get; set; }

        public bool IgnoreFailedNavigation { get; set; }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual Frame CreateRootFrame()
        {
            if (FrameFactory != null)
                return FrameFactory();
            return new Frame();
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(Frame frame)
        {
            if (frame == null)
                return null;
            return new FrameNavigationService(frame, ViewModelProvider);
        }

        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void TryBindRootFrame(Frame frame)
        {
            if (frame == null)
                return;
            var service = CreateNavigationService(frame);
            if (service != null)
                ServiceProvider.IocContainer.BindToConstant(service);
        }

        #endregion

        #region Implementation of interfaces

        public IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            if (Window.Current == null || Window.Current.Content != null)
            {
                TryBindRootFrame(Window.Current?.Content as Frame);
                return null;
            }

            var rootFrame = CreateRootFrame();
            if (rootFrame == null)
            {
                var viewModel = context.GetData(NavigationConstants.ViewModel);
                if (viewModel == null)
                    return null;
                var operation = new AsyncOperation<object>();
                OperationCallbackManager.Register(OperationType.PageNavigation, viewModel, operation.ToOperationCallback(), context);
                ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (presenter, vm, ctx) =>
                {
                    Window.Current.Content = (UIElement) presenter.ViewManager.GetOrCreateView(vm, null, ctx);
                    presenter.NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.New, null, vm, this, ctx));
                });
                return operation;
            }

            TryBindRootFrame(rootFrame);

            //Associate the frame with a SuspensionManager key                                
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
            if (!IgnoreFailedNavigation)
                rootFrame.NavigationFailed += OnNavigationFailed;

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;

            return parentPresenter.ShowAsync(context);
        }

        public Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion
    }
}