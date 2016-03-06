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

using System.Threading;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Callbacks;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.Modules
{
    public class InitializationModule : InitializationModuleBase
    {
        #region Cosntructors

        public InitializationModule()
        {
        }

        protected InitializationModule(LoadMode mode, int priority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Overrides of InitializationModuleBase

        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
                presenter.DynamicPresenters.Add(
                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                        container.Get<IViewManager>(),
                        container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                        container.Get<IOperationCallbackManager>()));
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IViewMappingProvider> GetViewMappingProvider()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<IViewMappingProvider>.FromMethod((adapter, list) => new ViewMappingProviderEx(assemblies) { IsSupportedUriNavigation = false }, DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(ServiceProvider.UiSynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}