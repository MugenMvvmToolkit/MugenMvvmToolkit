using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Silverlight.Infrastructure;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.WinRT.Infrastructure;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace MugenMvvmToolkit.Test
{
    [TestClass]
    public abstract class TestBase
    {
        #region Nested types

        private sealed class UnitTestApp : MvvmApplication
        {
            #region Fields

            private readonly TestBase _test;

            #endregion

            #region Constructors

            public UnitTestApp(TestBase test)
                : base(LoadMode.UnitTest)
            {
                _test = test;
            }

            #endregion

            #region Methods

            public override IViewModelSettings ViewModelSettings
            {
                get { return _test.Settings; }
            }

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

        protected ViewModelSettingsMock Settings { get; set; }

        protected ThreadManagerMock ThreadManager { get; set; }

        protected DisplayNameProviderMock DisplayNameProvider { get; set; }

        protected IocContainerMock IocContainer { get; set; }

        protected ViewModelProvider ViewModelProvider { get; set; }

        protected VisualStateManagerMock VisualStateManager { get; set; }

        protected OperationCallbackManagerMock OperationCallbackManager { get; set; }

        #endregion

        #region Methods

        [TestInitialize]
        public void SetUp()
        {
            ServiceProvider.DesignTimeManager = DesignTimeManagerImpl.Instance;
            ServiceProvider.AttachedValueProvider = new AttachedValueProvider();
            CanBeResolvedTypes = new List<Type>
            {
                typeof (IThreadManager),
                typeof (IViewModelSettings),
                typeof (IViewManager),
                typeof (IDisplayNameProvider),
                typeof(IViewModelProvider),
                typeof(IVisualStateManager),
                typeof(OperationCallbackManagerMock)
            };
            OperationCallbackManager = new OperationCallbackManagerMock();
            ViewManager = new ViewManagerMock();
            ThreadManager = new ThreadManagerMock();
            ServiceProvider.ThreadManager = ThreadManager;
            Settings = new ViewModelSettingsMock();
            DisplayNameProvider = new DisplayNameProviderMock();
            IocContainer = new IocContainerMock
            {
                GetFunc = GetFunc,
                CanResolveDelegate = CanResolve
            };
            ServiceProvider.Tracer = new ConsoleTracer();
            ViewModelProvider = new ViewModelProvider(IocContainer);
            OnInit();
            var app = new UnitTestApp(this);
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
                    throw new InvalidOperationException(string.Format("The exception is wrong {0}.", exception));
                Tracer.Info("Error : " + exception);
                return;
            }
            throw new InvalidOperationException(string.Format("The exception {0} was not thrown.", typeof(T)));
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
            if (type == typeof(IViewModelSettings))
                return Settings;
            if (type == typeof(IViewManager))
                return ViewManager;
            if (type == typeof(IDisplayNameProvider))
                return DisplayNameProvider;
            if (type == typeof(IVisualStateManager))
                return VisualStateManager;
            if (type == typeof(IViewModelProvider))
                return ViewModelProvider;
            if (type == typeof(IOperationCallbackManager))
                return OperationCallbackManager;
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