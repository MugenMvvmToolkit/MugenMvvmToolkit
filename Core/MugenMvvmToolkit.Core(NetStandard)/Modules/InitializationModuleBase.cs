﻿#region Copyright

// ****************************************************************************
// <copyright file="InitializationModuleBase.cs">
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

using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Infrastructure.Requests;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Requests;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Modules
{
    public abstract class InitializationModuleBase : IModule
    {
        #region Properties

        public int Priority { get; set; } = ApplicationSettings.ModulePriorityInitialization;

        #endregion

        #region Methods

        protected virtual void BindItemsSourceDecorator(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindOperationCallbackStateManager(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindAttachedValueProvider(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindReflectionManager(IModuleContext context, IIocContainer container)
        {
            IReflectionManager reflectionManager = new ExpressionReflectionManager();
            ToolkitServiceProvider.ReflectionManager = reflectionManager;
            container.BindToConstant(reflectionManager);
        }

        protected virtual void BindDisplayNameProvider(IModuleContext context, IIocContainer container)
        {
            container.Bind<IDisplayNameProvider, DisplayNameProvider>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindViewMappingProvider(IModuleContext context, IIocContainer container)
        {
            var platformType = context.PlatformInfo.Platform;
            IViewMappingProvider mappingProvider = new ViewMappingProvider(context.Assemblies) { IsSupportedUriNavigation = platformType == PlatformType.WPF };
            container.BindToConstant(mappingProvider);
        }

	    protected virtual void BindRequestHandlerProvider(IModuleContext context, IIocContainer container)
	    {
		    IRequestHandlerProvider requestHandlerProvider = new RequestHandlerProvider(container, context.Assemblies);
		    ToolkitServiceProvider.RequestHandlerProvider = requestHandlerProvider;
		    container.BindToConstant(requestHandlerProvider);
		}


		protected virtual void BindViewManager(IModuleContext context, IIocContainer container)
        {
            container.Bind<IViewManager, ViewManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindViewModelProvider(IModuleContext context, IIocContainer container)
        {
            IViewModelProvider viewModelProvider = new ViewModelProvider(container.GetRoot());
            ToolkitServiceProvider.ViewModelProvider = viewModelProvider;
            container.BindToConstant(viewModelProvider);
        }

        protected virtual void BindViewModelPresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<IViewModelPresenter, ViewModelPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindMessagePresenter(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindToastPresenter(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindWrapperManager(IModuleContext context, IIocContainer container)
        {
            container.Bind<IWrapperManager, WrapperManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindEventAggregator(IModuleContext context, IIocContainer container)
        {
            IEventAggregator eventAggregator = new EventAggregator();
            ToolkitServiceProvider.EventAggregator = eventAggregator;
            container.BindToConstant(eventAggregator);
        }

        protected virtual void BindEntityStateProvider(IModuleContext context, IIocContainer container)
        {
            container.Bind<IEntityStateManager, EntityStateManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindValidatorProvider(IModuleContext context, IIocContainer container)
        {
            IValidatorProvider validatorProvider = new ValidatorProvider();
            validatorProvider.Register(typeof(ValidatableViewModelValidator));
            validatorProvider.Register(typeof(DataAnnotationValidatior));
            ToolkitServiceProvider.ValidatorProvider = validatorProvider;
            container.BindToConstant(validatorProvider);
        }

        protected virtual void BindTracer(IModuleContext context, IIocContainer container)
        {
            container.BindToConstant<ITracer>(Tracer.Instance);
        }

        protected virtual void BindThreadManager(IModuleContext context, IIocContainer container)
        {
            IThreadManager threadManager = new SynchronousThreadManager();
            ToolkitServiceProvider.ThreadManager = threadManager;
            container.BindToConstant(threadManager);
        }

        protected virtual void BindNavigationProvider(IModuleContext context, IIocContainer container)
        {
        }

        protected virtual void BindSerializer(IModuleContext context, IIocContainer container)
        {
            var assemblies = context.Assemblies;
            container.BindToMethod<ISerializer>((iocContainer, list) => new Serializer(assemblies), DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindOperationCallbackManager(IModuleContext context, IIocContainer container)
        {
            container.Bind<IOperationCallbackManager, OperationCallbackManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindOperationCallbackFactory(IModuleContext context, IIocContainer container)
        {
            IOperationCallbackFactory callbackFactory = new DefaultOperationCallbackFactory();
            ToolkitServiceProvider.OperationCallbackFactory = callbackFactory;
            container.BindToConstant(callbackFactory);
        }

        protected virtual void BindNavigationDispatcher(IModuleContext context, IIocContainer container)
        {
            container.Bind<INavigationDispatcher, NavigationDispatcher>(DependencyLifecycle.SingleInstance);
        }

        #endregion

        #region Implementation of interfaces

        public virtual bool Load(IModuleContext context)
        {
            BindAttachedValueProvider(context, context.IocContainer);
            BindThreadManager(context, context.IocContainer);
            BindSerializer(context, context.IocContainer);
            BindOperationCallbackManager(context, context.IocContainer);
            BindNavigationDispatcher(context, context.IocContainer);
            BindOperationCallbackFactory(context, context.IocContainer);
            BindOperationCallbackStateManager(context, context.IocContainer);
            BindViewMappingProvider(context, context.IocContainer);
	        BindRequestHandlerProvider(context, context.IocContainer);
			BindViewManager(context, context.IocContainer);
            BindDisplayNameProvider(context, context.IocContainer);
            BindViewModelProvider(context, context.IocContainer);
            BindMessagePresenter(context, context.IocContainer);
            BindToastPresenter(context, context.IocContainer);
            BindViewModelPresenter(context, context.IocContainer);
            BindWrapperManager(context, context.IocContainer);
            BindEventAggregator(context, context.IocContainer);
            BindEntityStateProvider(context, context.IocContainer);
            BindValidatorProvider(context, context.IocContainer);
            BindTracer(context, context.IocContainer);
            BindReflectionManager(context, context.IocContainer);
            BindNavigationProvider(context, context.IocContainer);
            BindItemsSourceDecorator(context, context.IocContainer);
            return true;
        }

        public virtual void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}