#region Copyright

// ****************************************************************************
// <copyright file="WinFormsInitializationModule.cs">
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
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WinForms.Infrastructure;
using MugenMvvmToolkit.WinForms.Infrastructure.Callbacks;
using MugenMvvmToolkit.WinForms.Infrastructure.Mediators;
using MugenMvvmToolkit.WinForms.Infrastructure.Presenters;
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Modules
{
    public class WinFormsInitializationModule : InitializationModuleBase
    {
        #region Methods

        protected override void BindViewManager(IModuleContext context, IIocContainer container)
        {
            container.BindToMethod<IViewManager>((iocContainer, list) =>
            {
                var viewManager = iocContainer.Get<ViewManager>();
                viewManager.ViewCleared += OnViewCleared;
                return viewManager;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override void BindOperationCallbackFactory(IModuleContext context, IIocContainer container)
        {
            container.Bind<IOperationCallbackFactory, SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindItemsSourceDecorator(IModuleContext context, IIocContainer container)
        {
            container.Bind<IItemsSourceDecorator, BindingListItemsSourceDecorator>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindMessagePresenter(IModuleContext context, IIocContainer container)
        {
            if (context.PlatformInfo.Platform == PlatformType.WinForms)
                container.Bind<IMessagePresenter, MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindToastPresenter(IModuleContext context, IIocContainer container)
        {
            if (context.PlatformInfo.Platform == PlatformType.WinForms)
                container.Bind<IToastPresenter, ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindThreadManager(IModuleContext context, IIocContainer container)
        {
            if (context.PlatformInfo.Platform != PlatformType.WinForms)
                return;
            container.BindToMethod<IThreadManager>((iocContainer, list) =>
            {
                var syncContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
                if (syncContext == null)
                {
                    syncContext = new WindowsFormsSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(syncContext);
                    WindowsFormsSynchronizationContext.AutoInstall = false;
                }
                return new ThreadManager(syncContext);
            }, DependencyLifecycle.SingleInstance);
        }

        protected override void BindReflectionManager(IModuleContext context, IIocContainer container)
        {
            IReflectionManager reflectionManager = new ExpressionReflectionManagerEx();
            ServiceProvider.ReflectionManager = reflectionManager;
            container.BindToConstant(reflectionManager);
        }

        protected override void BindViewModelPresenter(IModuleContext context, IIocContainer container)
        {
            if (context.PlatformInfo.Platform == PlatformType.WinForms)
            {

                container.BindToMethod((iocContainer, list) =>
                {
                    IViewModelPresenter presenter = iocContainer.Get<ViewModelPresenter>();
                    var windowPresenter = iocContainer.Get<DynamicViewModelWindowPresenter>();
                    windowPresenter.RegisterMediatorFactory<WindowViewMediator, IWindowView>();
                    presenter.DynamicPresenters.Add(windowPresenter);
                    return presenter;
                }, DependencyLifecycle.SingleInstance);
            }
            else
            {
                ServiceProvider.Initialized += MvvmApplicationOnInitialized;
            }
        }

        protected override void BindTracer(IModuleContext context, IIocContainer container)
        {
            ITracer tracer = new TracerEx();
            ServiceProvider.Tracer = tracer;
            container.BindToConstant(tracer);
        }

        protected override void BindAttachedValueProvider(IModuleContext context, IIocContainer container)
        {
            IAttachedValueProvider attachedValueProvider = new AttachedValueProvider();
            ServiceProvider.AttachedValueProvider = attachedValueProvider;
            container.BindToConstant(attachedValueProvider);
        }

        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            ServiceProvider.Initialized -= MvvmApplicationOnInitialized;
            IViewModelPresenter presenter;
            if (ServiceProvider.TryGet(out presenter))
            {
                var windowPresenter = presenter.DynamicPresenters.OfType<DynamicViewModelWindowPresenter>().FirstOrDefault();
                if (windowPresenter == null)
                {
                    windowPresenter = ServiceProvider.Get<DynamicViewModelWindowPresenter>();
                    presenter.DynamicPresenters.Add(windowPresenter);
                }
                windowPresenter.RegisterMediatorFactory<WindowViewMediator, IWindowView>();
            }
        }

        private static void OnViewCleared(IViewManager viewManager, ViewClearedEventArgs args)
        {
            try
            {
                var control = args.View as Control;
                if (control != null)
                    ClearBindingsRecursively(control.Controls);
                (args.View as IDisposable)?.Dispose();
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten());
            }
        }

        private static void ClearBindingsRecursively(Control.ControlCollection collection)
        {
            if (collection == null)
                return;
            foreach (var item in collection.OfType<Control>())
            {
                ClearBindingsRecursively(item.Controls);
                item.ClearBindings(true, true);
            }
        }

        #endregion
    }
}