#region Copyright

// ****************************************************************************
// <copyright file="WinFormsInitializationModule.cs">
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
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WinForms.Infrastructure;
using MugenMvvmToolkit.WinForms.Infrastructure.Callbacks;
using MugenMvvmToolkit.WinForms.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WinForms.Modules
{
    public class WinFormsInitializationModule : InitializationModuleBase
    {
        #region Methods

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
                    IViewModelPresenter presenter = new ViewModelPresenter();
                    presenter.DynamicPresenters.Add(iocContainer.Get<DynamicViewModelWindowPresenter>());
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

        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            ServiceProvider.Initialized -= MvvmApplicationOnInitialized;
            IViewModelPresenter presenter;
            if (ServiceProvider.TryGet(out presenter))
                presenter.DynamicPresenters.Add(ServiceProvider.Get<DynamicViewModelWindowPresenter>());
        }

        #endregion
    }
}