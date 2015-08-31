#region Copyright

// ****************************************************************************
// <copyright file="AndroidBootstrapperBase.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Android.App;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class AndroidBootstrapperBase : BootstrapperBase
    {
        #region Nested types

        private sealed class AssemblyNameComparer : IComparer<string>
        {
            #region Fields

            public static readonly AssemblyNameComparer Comparer;

            #endregion

            #region Constructors

            static AssemblyNameComparer()
            {
                Comparer = new AssemblyNameComparer();
            }

            #endregion

            #region Implementation of IComparer<in string>

            public int Compare(string x, string y)
            {
                if (string.Equals(x, y, StringComparison.Ordinal))
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

        #endregion

        #region Fields

        private const int EmptyState = 0;
        private const int InitializedStateGlobal = 1;
        private const int InitializedStateLocal = 2;

        private static int _appStateGlobal;
        private ICollection<Assembly> _assemblies;

        #endregion

        #region Constructors

        static AndroidBootstrapperBase()
        {
            LinkerInclude.Initialize();
            ViewManager.AlwaysCreateNewView = true;
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AndroidBootstrapperBase" /> class.
        /// </summary>
        protected AndroidBootstrapperBase(PlatformInfo platform = null)
            : base(platform ?? PlatformExtensions.GetPlatformInfo())
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the collection of view assemblies.
        /// </summary>
        public static IList<Assembly> ViewAssemblies { get; private set; }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        public override void Initialize()
        {
            if (Interlocked.Exchange(ref _appStateGlobal, InitializedStateLocal) != InitializedStateLocal)
                base.Initialize();
        }

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void OnInitialize()
        {
            //NOTE: to improve startup performance saving the collection of assemblies to use it later.
            ViewAssemblies = GetAndroidViewAssemblies();
            TypeCache<View>.Initialize(null);
            base.OnInitialize();
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            if (_assemblies == null)
                InitalizeAssemblies();
            return _assemblies;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            CreateMainViewModel(GetMainViewModelType())
                .ShowAsync((model, result) => model.Dispose(), null, new DataContext(InitializationContext));
        }

        /// <summary>
        ///     Makes sure that the application is initialized.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _appStateGlobal, InitializedStateGlobal, EmptyState) != EmptyState)
                return;

            var attributes = new List<BootstrapperAttribute>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies())
            {
                attributes.AddRange(assembly
                    .GetCustomAttributes(typeof(BootstrapperAttribute), false)
                    .OfType<BootstrapperAttribute>());
            }
            var bootstrapperAttribute = attributes
                .OrderByDescending(attribute => attribute.Priority)
                .FirstOrDefault();
            if (bootstrapperAttribute == null)
                throw new InvalidOperationException(@"The BootstrapperAttribute was not found. 
You must specify the type of application bootstrapper using BootstrapperAttribute, for example [assembly:Bootstrapper(typeof(MyBootstrapperType))]");
            var instance = (BootstrapperBase)Activator.CreateInstance(bootstrapperAttribute.BootstrapperType);
            instance.Initialize();
        }

        /// <summary>
        ///     Gets the android view assemblies.
        /// </summary>
        protected virtual IList<Assembly> GetAndroidViewAssemblies()
        {
            return InitalizeAssemblies();
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, new DataContext(InitializationContext));
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        private IList<Assembly> InitalizeAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            var viewAssemblies = new HashSet<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsToolkitAssembly())
                {
                    assemblies.Add(assembly);
                    viewAssemblies.Add(assembly);
                }
                else if (!assembly.IsDynamic && !assembly.IsMicrosoftAssembly())
                    viewAssemblies.Add(assembly);
            }
            _assemblies = assemblies;
            return viewAssemblies.OrderBy(assembly => assembly.FullName, AssemblyNameComparer.Comparer).ToArray();
        }

        #endregion
    }
}