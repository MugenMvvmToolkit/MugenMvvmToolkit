#region Copyright

// ****************************************************************************
// <copyright file="TestBase.cs">
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
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Silverlight.Infrastructure;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.UWP.Infrastructure;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace MugenMvvmToolkit.Test
{
    [TestClass]
    public abstract class TestBase
    {
        #region Nested types

        private sealed class UnitTestApp : MvvmApplication
        {
            #region Constructors

            public UnitTestApp()
                : base(null, LoadMode.UnitTest)
            {
            }

            #endregion

            #region Methods

            public override Type GetStartViewModelType()
            {
                return typeof(IViewModel);
            }

            #endregion
        }

        #endregion

        #region Properties

        protected List<Type> CanBeResolvedTypes { get; private set; }

        protected ViewManagerMock ViewManager { get; set; }

        protected ThreadManagerMock ThreadManager { get; set; }

        protected DisplayNameProviderMock DisplayNameProvider { get; set; }

        protected IocContainerMock IocContainer { get; set; }

        protected ViewModelProvider ViewModelProvider { get; set; }

        protected OperationCallbackManagerMock OperationCallbackManager { get; set; }

        protected NavigationDispatcherMock NavigationDispatcher { get; set; }

        protected ViewModelPresenterMock ViewModelPresenter { get; set; }

        #endregion

        #region Methods

        [TestInitialize]
        public void SetUp()
        {
            ServiceProvider.AttachedValueProvider = new AttachedValueProvider();
            ServiceProvider.ViewModelSettingsFactory = model => new DefaultViewModelSettings();
            ServiceProvider.ReflectionManager = new ExpressionReflectionManager();
            CanBeResolvedTypes = new List<Type>
            {
                typeof (IThreadManager),
                typeof (IViewManager),
                typeof (IDisplayNameProvider),
                typeof (IViewModelProvider),
                typeof (OperationCallbackManagerMock),
                typeof(INavigationDispatcher),
                typeof(IViewModelPresenter)
            };
            OperationCallbackManager = new OperationCallbackManagerMock();
            NavigationDispatcher = new NavigationDispatcherMock();
            ViewManager = new ViewManagerMock();
            ThreadManager = new ThreadManagerMock();
            ViewModelPresenter = new ViewModelPresenterMock();
            ServiceProvider.ThreadManager = ThreadManager;
            DisplayNameProvider = new DisplayNameProviderMock();
            IocContainer = new IocContainerMock
            {
                GetFunc = GetFunc,
                CanResolveDelegate = CanResolve
            };
            Tracer.TraceInformation = true;
            ServiceProvider.Tracer = new ConsoleTracer();
            ViewModelProvider = new ViewModelProvider(IocContainer);
            OnInit();
            var app = new UnitTestApp();
            app.Initialize(PlatformInfo.UnitTest, IocContainer, Empty.Array<Assembly>(), DataContext.Empty);
        }

        protected T GetViewModel<T>() where T : IViewModel, new()
        {
            var vm = new T();
            InitializeViewModel(vm, IocContainer);
            return vm;
        }

        protected static void ShouldThrow<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                    exception = GetOriginalException(aggregateException);
                if (!(exception is T))
                    throw new InvalidOperationException($"The exception is wrong {exception}.");
                Tracer.Info("Error : " + exception);
                return;
            }
            throw new InvalidOperationException($"The exception {typeof(T)} was not thrown.");
        }

        protected void ShouldThrow(Action action)
        {
            ShouldThrow<Exception>(action);
        }

        protected static void InitializeViewModel(IViewModel viewModel, IIocContainer iocContainer, IViewModel parentViewModel = null, IDataContext context = null)
        {
            if (viewModel.IsInitialized)
                return;

            var dataContext = context.ToNonReadOnly();
            if (parentViewModel != null)
                dataContext.Add(InitializationConstants.ParentViewModel, parentViewModel);
            if (iocContainer != null)
                dataContext.Add(InitializationConstants.IocContainer, iocContainer);

            viewModel.InitializeViewModel(dataContext);
        }

        protected virtual object GetFunc(Type type, string s, IIocParameter[] arg3)
        {
            if (type == typeof(IThreadManager))
                return ThreadManager;
            if (type == typeof(IViewManager))
                return ViewManager;
            if (type == typeof(IDisplayNameProvider))
                return DisplayNameProvider;
            if (type == typeof(IViewModelProvider))
                return ViewModelProvider;
            if (type == typeof(IOperationCallbackManager))
                return OperationCallbackManager;
            if (type == typeof(INavigationDispatcher))
                return NavigationDispatcher;
            if (type == typeof(IViewModelPresenter))
                return ViewModelPresenter;
            return Activator.CreateInstance(type);
        }

        protected static Exception GetOriginalException(AggregateException aggregateException)
        {
            Exception exception = aggregateException;
            while (aggregateException != null)
            {
                exception = aggregateException.InnerException;
                aggregateException = exception as AggregateException;
            }
            return exception;
        }

        private bool CanResolve(Type type)
        {
            return CanBeResolvedTypes.Contains(type);
        }

        protected virtual void OnInit()
        {
        }

        #endregion
    }
}
