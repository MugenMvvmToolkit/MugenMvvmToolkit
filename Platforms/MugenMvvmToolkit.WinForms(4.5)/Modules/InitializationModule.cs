#region Copyright

// ****************************************************************************
// <copyright file="InitializationModule.cs">
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
using MugenMvvmToolkit.WinForms.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WinForms.Modules
{
    /// <summary>
    ///     Represents the class that is used to initialize the IOC adapter.
    /// </summary>
    public class InitializationModule : InitializationModuleBase
    {
        #region Constructors

        static InitializationModule()
        {
            ServiceProvider.ItemsSourceDecorator = new BindingListItemsSourceDecorator();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModule" /> class.
        /// </summary>
        public InitializationModule()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModule" /> class.
        /// </summary>
        protected InitializationModule(LoadMode mode = LoadMode.All, int priority = InitializationModulePriority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that module should use the <see cref="SynchronousThreadManager"/> as thread manager dependency.
        /// </summary>
        public bool UseSimpleThreadManager { get; set; }

        #endregion

        #region Overrides of InitializationModuleBase

        /// <summary>
        ///     Gets the <see cref="IMessagePresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IMessagePresenter" />.</returns>
        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            if (Context.Platform.Platform == PlatformType.WinForms)
                return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
            return BindingInfo<IMessagePresenter>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            if (Context.Platform.Platform == PlatformType.WinForms)
                return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
            return BindingInfo<IToastPresenter>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IThreadManager" /> which will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IThreadManager" />.</returns>
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

        /// <summary>
        ///     Gets the <see cref="IReflectionManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IReflectionManager" />.</returns>
        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelPresenter" />.</returns>
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
            BootstrapperBase.Initialized += (sender, args) =>
            {
                var container = ServiceProvider.IocContainer;
                IViewModelPresenter presenter;
                if (container.TryGet(out presenter))
                    presenter.DynamicPresenters.Add(
                            new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                                container.Get<IViewManager>(), container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                                container.Get<IOperationCallbackManager>()));
            };
            return BindingInfo<IViewModelPresenter>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="ITracer" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="ITracer" />.</returns>
        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<TracerEx>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}