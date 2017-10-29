#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsInitializationModule.cs">
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

using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Callbacks;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Mediators;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;

namespace MugenMvvmToolkit.Xamarin.Forms.Modules
{
    public class XamarinFormsInitializationModule : InitializationModuleBase
    {
        #region Methods

        protected override void BindOperationCallbackFactory(IModuleContext context, IIocContainer container)
        {
            container.Bind<IOperationCallbackFactory, SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindViewModelPresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<RestorableViewModelPresenter, RestorableViewModelPresenter>(DependencyLifecycle.SingleInstance);
            container.BindToMethod((iocContainer, list) =>
            {
                IViewModelPresenter presenter = container.Get<RestorableViewModelPresenter>();
                var windowPresenter = iocContainer.Get<DynamicViewModelWindowPresenter>();
                windowPresenter.RegisterMediatorFactory<ModalViewMediator, IModalView>();
                presenter.DynamicPresenters.Add(windowPresenter);
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override void BindViewMappingProvider(IModuleContext context, IIocContainer container)
        {
            IViewMappingProvider viewMappingProvider = new ViewMappingProviderEx(context.Assemblies) { IsSupportedUriNavigation = false };
            container.BindToConstant(viewMappingProvider);
        }

        protected override void BindThreadManager(IModuleContext context, IIocContainer container)
        {
            container.BindToMethod<IThreadManager>((iocContainer, list) => new ThreadManager(ToolkitServiceProvider.UiSynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        protected override void BindNavigationProvider(IModuleContext context, IIocContainer container)
        {
            container.Bind<INavigationProvider, NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindAttachedValueProvider(IModuleContext context, IIocContainer container)
        {
            IAttachedValueProvider attachedValueProvider = new AttachedValueProvider();
            ToolkitServiceProvider.AttachedValueProvider = attachedValueProvider;
            container.BindToConstant(attachedValueProvider);
        }

        #endregion
    }
}