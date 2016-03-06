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

using Android.App;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Infrastructure.Callbacks;
using MugenMvvmToolkit.Android.Infrastructure.Navigation;
using MugenMvvmToolkit.Android.Infrastructure.Presenters;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;

namespace MugenMvvmToolkit.Android.Modules
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

        protected override bool LoadInternal()
        {
            var iocContainer = Context.IocContainer;
            iocContainer.BindToBindingInfo(GetViewFactory());
            iocContainer.BindToBindingInfo(GetNavigationService());

            var result = base.LoadInternal();
            INavigationProvider provider;
            iocContainer.TryGet(out provider);
            return result;
        }

        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<TracerEx>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(Application.SynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }

        #endregion

        #region Methods

        protected virtual BindingInfo<INavigationService> GetNavigationService()
        {
            return BindingInfo<INavigationService>.FromType<NavigationService>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IViewFactory> GetViewFactory()
        {
            return BindingInfo<IViewFactory>.FromType<ViewFactory>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}
