#region Copyright

// ****************************************************************************
// <copyright file="InitializationModule.cs">
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
using System.Threading;
using System.Windows.Forms;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WinForms.Infrastructure;
using MugenMvvmToolkit.WinForms.Infrastructure.Callbacks;
using MugenMvvmToolkit.WinForms.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WinForms.Modules
{
    public class InitializationModule : InitializationModuleBase
    {
        #region Constructors

        public InitializationModule()
        {
        }

        protected InitializationModule(LoadMode mode, int priority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Properties

        public bool UseSimpleThreadManager { get; set; }

        #endregion

        #region Overrides of InitializationModuleBase

        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IItemsSourceDecorator> GetItemsSourceDecorator()
        {
            return BindingInfo<IItemsSourceDecorator>.FromType<BindingListItemsSourceDecorator>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            if (Context.Platform.Platform == PlatformType.WinForms)
                return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
            return BindingInfo<IMessagePresenter>.Empty;
        }

        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            if (Context.Platform.Platform == PlatformType.WinForms)
                return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
            return BindingInfo<IToastPresenter>.Empty;
        }

        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            if (Context.Platform.Platform != PlatformType.WinForms)
                return BindingInfo<IThreadManager>.Empty;
            if (UseSimpleThreadManager)
                return BindingInfo<IThreadManager>.FromType<SynchronousThreadManager>(DependencyLifecycle.SingleInstance);
            return BindingInfo<IThreadManager>.FromMethod((container, list) =>
            {
                var context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
                if (context == null)
                {
                    context = new WindowsFormsSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(context);
                    WindowsFormsSynchronizationContext.AutoInstall = false;
                }
                return new ThreadManager(context);
            }, DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            if (Context.Platform.Platform == PlatformType.WinForms)
                return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
                {
                    var presenter = new ViewModelPresenter();
                    presenter.DynamicPresenters.Add(
                        new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                            container.Get<IViewManager>(), container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                            container.Get<IOperationCallbackManager>()));
                    return presenter;
                }, DependencyLifecycle.SingleInstance);
            MvvmApplication.Initialized += MvvmApplicationOnInitialized;
            return BindingInfo<IViewModelPresenter>.Empty;
        }

        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<TracerEx>(DependencyLifecycle.SingleInstance);
        }

        #endregion

        #region Methods

        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            MvvmApplication.Initialized -= MvvmApplicationOnInitialized;
            IViewModelPresenter presenter;
            if (ServiceProvider.TryGet(out presenter))
                presenter.DynamicPresenters.Add(ServiceProvider.Get<DynamicViewModelWindowPresenter>());
        }

        #endregion
    }
}
