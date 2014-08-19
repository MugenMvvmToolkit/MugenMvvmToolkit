#region Copyright
// ****************************************************************************
// <copyright file="AndroidBootstrapperBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Android.App;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class AndroidBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private const int EmptyState = 0;
        private const int InitializedStateGlobal = 1;
        private const int InitializedStateLocal = 2;

        private static int _appStateGlobal;

        private static readonly MethodInfo CreateLambdaGeneric;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static AndroidBootstrapperBase()
        {
            ViewManager.AlwaysCreateNewView = true;
            Expression body = null;
            IEnumerable<ParameterExpression> expressions = null;
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            CreateLambdaGeneric = GetMethod(() => CreateLambdaExpression<Delegate>(body, expressions)).GetGenericMethodDefinition();
            ExpressionReflectionManager.CreateLambdaExpressionByType = CreateLambdaExpressionByType;
            ExpressionReflectionManager.CreateLambdaExpression = CreateLambdaExpression;
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AndroidBootstrapperBase" /> class.
        /// </summary>
        protected AndroidBootstrapperBase()
        {
            _platform = PlatformExtensions.GetPlatformInfo();
        }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public override PlatformInfo Platform
        {
            get { return _platform; }
        }

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        public override void Initialize(bool throwIfInitialized = false)
        {
            if (Interlocked.Exchange(ref _appStateGlobal, InitializedStateLocal) != InitializedStateLocal)
                base.Initialize(throwIfInitialized);
        }

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        protected override void OnInitialize()
        {
            //NOTE: to improve startup performance adding types manually
            var viewCache = TypeCache<View>.Instance;
#if !API8
            var actionProviderCache = TypeCache<ActionProvider>.Instance;
            var fragmentCache = TypeCache<Fragment>.Instance;
#endif
            IList<Assembly> assemblies = GetAndroidViewAssemblies();
            for (int i = 0; i < assemblies.Count; i++)
            {
                var types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    viewCache.Add(type);
#if !API8
                    actionProviderCache.Add(type);
                    fragmentCache.Add(type);
#endif
                }
            }
            base.OnInitialize();
            //To activate navigation provider.
            var provider = IocContainer.Get<INavigationProvider>();
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            return new HashSet<Assembly>(MvvmUtils.SkipFrameworkAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start(IDataContext context = null)
        {
            var mainViewModelType = GetMainViewModelType();
            if (context == null)
                context = DataContext.Empty;
            Initialize(false);
            CreateMainViewModel(mainViewModelType, context)
                .ShowAsync((model, result) => model.Dispose(), null, context);
        }

        /// <summary>
        ///     Makes sure that the application is initialized.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _appStateGlobal, InitializedStateGlobal, EmptyState) != EmptyState)
                return;

            var attributes = new List<BootstrapperAttribute>();
            foreach (var assembly in MvvmUtils.SkipFrameworkAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
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
You must specify the type of application bootstraper using BootstrapperAttribute, for example [assembly:Bootstrapper(typeof(MyBootstrapperType))]");
            var instance = (BootstrapperBase)Activator.CreateInstance(bootstrapperAttribute.BootstrapperType);
            instance.Initialize(false);
        }

        /// <summary>
        ///     Gets the android view assemblies.
        /// </summary>
        protected virtual IList<Assembly> GetAndroidViewAssemblies()
        {
            return new[]
            {
                typeof (View).Assembly,
                typeof (ListItemView).Assembly,
                GetType().Assembly,
#if API8SUPPORT
               typeof(SearchView).Assembly
#endif
            };
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType, [NotNull] IDataContext context)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, context);
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        private static LambdaExpression CreateLambdaExpressionByType(Type type, Expression expression,
            IEnumerable<ParameterExpression> arg3)
        {
            return (LambdaExpression)CreateLambdaGeneric
                .MakeGenericMethod(type)
                .Invoke(null, new object[] { expression, arg3 });
        }

        private static LambdaExpression CreateLambdaExpression(Expression body,
            ParameterExpression[] parameterExpressions)
        {
            var types = new Type[parameterExpressions.Length + 1];
            if (parameterExpressions.Length > 0)
            {
                var set = new HashSet<ParameterExpression>();
                for (int index = 0; index < parameterExpressions.Length; ++index)
                {
                    ParameterExpression parameterExpression = parameterExpressions[index];
                    types[index] = !parameterExpression.IsByRef
                        ? parameterExpression.Type
                        : parameterExpression.Type.MakeByRefType();
                    if (set.Contains(parameterExpression))
                        throw BindingExceptionManager.DuplicateLambdaParameter(parameterExpression.ToString());
                    set.Add(parameterExpression);
                }
            }
            types[parameterExpressions.Length] = body.Type;
            Type delegateType = Expression.GetDelegateType(types);
            return CreateLambdaExpressionByType(delegateType, body, parameterExpressions);
        }

        private static LambdaExpression CreateLambdaExpression<T>(Expression body,
            IEnumerable<ParameterExpression> expressions)
        {
            return Expression.Lambda<T>(body, expressions);
        }

        private static MethodInfo GetMethod(Expression<Action> action)
        {
            var callExpression = action.Body as MethodCallExpression;
            Should.BeSupported(callExpression != null, "The {0} method was not found", action);
            return callExpression.Method;
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = MvvmUtils.GetIocContainer(viewModel, true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = MvvmUtils.GetIocContainer(viewModel, true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Activity).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}