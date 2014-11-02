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
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class TouchBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly PlatformInfo _platform;
        private readonly UIWindow _window;
        private static readonly MethodInfo CreateLambdaGeneric;
        private INavigationService _navigationService;

        #endregion

        #region Constructors

        static TouchBootstrapperBase()
        {
            Expression body = null;
            IEnumerable<ParameterExpression> expressions = null;
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            CreateLambdaGeneric = GetMethod(() => CreateLambdaExpression<Delegate>(body, expressions)).GetGenericMethodDefinition();
            ExpressionReflectionManager.CreateLambdaExpressionByType = CreateLambdaExpressionByType;
            ExpressionReflectionManager.CreateLambdaExpression = CreateLambdaExpression;

            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchBootstrapperBase" /> class.
        /// </summary>
        protected TouchBootstrapperBase([NotNull] UIWindow window)
        {
            Should.NotBeNull(window, "window");
            _window = window;
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
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            return new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies());
        }

        /// <summary>
        ///     Initializes the current bootstraper.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _navigationService = CreateNavigationService(_window);
            if (_navigationService != null)
                IocContainer.BindToConstant(_navigationService);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start(IDataContext context = null)
        {
            if (context == null)
                context = DataContext.Empty;
            Initialize(false);
            
            Type mainViewModelType = GetMainViewModelType();
            IViewModel viewModel = CreateMainViewModel(mainViewModelType, context);
            viewModel.ShowAsync((model, result) => model.Dispose(), null, context);
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

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            return new NavigationService(window);
        }

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
                        throw BindingExtensions.DuplicateLambdaParameter(parameterExpression.ToString());
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
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || typeof(ITabView).IsAssignableFrom(mappingItem.ViewType) ||
                   !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}