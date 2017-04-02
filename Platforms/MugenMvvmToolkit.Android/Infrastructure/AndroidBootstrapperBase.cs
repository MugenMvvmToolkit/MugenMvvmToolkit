#region Copyright

// ****************************************************************************
// <copyright file="AndroidBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using Android.App;
using Android.OS;
using Android.Views;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public abstract class AndroidBootstrapperBase : BootstrapperBase, IComparer<string>
    {
        #region Nested types

        protected sealed class DefaultApp : MvvmApplication
        {
            #region Fields

            private readonly Type _startViewModelType;

            #endregion

            #region Constructors

            public DefaultApp(Type startViewModelType, Action<IModuleContext> loadModulesDelegate = null, LoadMode mode = LoadMode.Runtime)
                : base(loadModulesDelegate, mode)
            {
                Should.NotBeNull(startViewModelType, nameof(startViewModelType));
                Should.BeOfType<IViewModel>(startViewModelType, "startViewModelType");
                _startViewModelType = startViewModelType;
            }

            #endregion

            #region Methods

            public override Type GetStartViewModelType()
            {
                return _startViewModelType;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly PlatformInfo _platform;
        internal const string BootTypeKey = "BootTypeKey";
        private static string _bootstrapperType;
        private static readonly object Locker;

        #endregion

        #region Constructors

        static AndroidBootstrapperBase()
        {
            ServiceProvider.WeakReferenceFactory = AndroidToolkitExtensions.CreateWeakReference;
            ApplicationSettings.ViewManagerAlwaysCreateNewView = true;
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            Locker = new object();
        }

        protected AndroidBootstrapperBase(bool isDesignMode = false, PlatformInfo platform = null)
            : base(isDesignMode)
        {
            _platform = platform ?? AndroidToolkitExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        public static IList<Assembly> ViewAssemblies { get; protected set; }

        public new static AndroidBootstrapperBase Current { get; private set; }

        internal static string BootstrapperType
        {
            get
            {
                if (_bootstrapperType == null && Current != null)
                    _bootstrapperType = Current.GetType().AssemblyQualifiedName;
                return _bootstrapperType;
            }
        }

        protected override PlatformInfo Platform => _platform;

        #endregion

        #region Methods

        public static AndroidBootstrapperBase GetOrCreateBootstrapper(Func<AndroidBootstrapperBase> factory)
        {
            lock (Locker)
            {
                if (Current == null)
                    Current = factory();
                return Current;
            }
        }

        public static void EnsureInitialized(object sender = null, Bundle bundle = null, Func<AndroidBootstrapperBase> factory = null)
        {
            if (Current == null)
            {
                lock (Locker)
                {
                    if (Current == null)
                    {
                        if (factory == null)
                        {
                            Type type;
                            var typeString = bundle?.GetString(BootTypeKey);
                            if (string.IsNullOrEmpty(typeString))
                            {
                                BootstrapperAttribute bootstrapperAttribute = null;
                                if (sender != null)
                                    bootstrapperAttribute = sender.GetType().Assembly.GetCustomAttribute<BootstrapperAttribute>();
                                if (bootstrapperAttribute == null)
                                {
                                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                                    {
                                        bootstrapperAttribute = (BootstrapperAttribute)assembly
                                            .GetCustomAttributes(typeof(BootstrapperAttribute), false)
                                            .FirstOrDefault();
                                        if (bootstrapperAttribute != null)
                                            break;
                                    }
                                    if (bootstrapperAttribute == null)
                                        throw new InvalidOperationException(@"The BootstrapperAttribute was not found.
You must specify the type of application bootstrapper using BootstrapperAttribute, for example [assembly:Bootstrapper(typeof(MyBootstrapperType))]");
                                }
                                type = bootstrapperAttribute.BootstrapperType;
                            }
                            else
                                type = Type.GetType(typeString, true);
                            Current = (AndroidBootstrapperBase)Activator.CreateInstance(type);
                        }
                        else
                            Current = factory();
                    }
                }
            }
            Current.Initialize();
        }

        protected override void InitializeInternal()
        {
            TypeCache<View>.Initialize(null);
            base.InitializeInternal();
            //Activating navigation provider if need
            INavigationProvider provider;
            IocContainer.TryGet(out provider);
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).ToList();
            assemblies.AddRange(domainAssemblies);
            if (ViewAssemblies == null)
            {
                //NOTE order the assemblies to keep the support libraries at the end of array.
                ViewAssemblies = domainAssemblies
                    .OrderBy(assembly => assembly.FullName, this)
                    .ToArray();
            }
        }

        public virtual void Start()
        {
            Initialize();
            MvvmApplication.Start();
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion

        #region Implementation of interfaces

        int IComparer<string>.Compare(string x, string y)
        {
            if (string.Equals(x, y))
                return 0;
            var xSupport = x.IndexOf(".Android.Support.", StringComparison.Ordinal) >= 0;
            var ySupport = y.IndexOf(".Android.Support.", StringComparison.Ordinal) >= 0;
            if (xSupport == ySupport)
                return string.Compare(x, y, StringComparison.Ordinal);
            if (xSupport)
                return 1;
            return -1;
        }

        #endregion
    }
}