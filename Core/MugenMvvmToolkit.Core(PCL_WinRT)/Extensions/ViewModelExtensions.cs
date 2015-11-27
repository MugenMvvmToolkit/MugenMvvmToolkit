#region Copyright

// ****************************************************************************
// <copyright file="ViewModelExtensions.cs">
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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit.ViewModels
{
    public static class ViewModelExtensions
    {
        #region View models extension

        public static Guid GetViewModelId(this IViewModel viewModel)
        {
            return ViewModelProvider.GetOrAddViewModelId(viewModel);
        }

        public static bool? GetOperationResult(IViewModel viewModel, bool? defaultValue = null)
        {
            var hasOperationResult = viewModel as IHasOperationResult;
            if (hasOperationResult == null)
                return defaultValue;
            return hasOperationResult.OperationResult;
        }

        public static string GetViewName([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                return viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
            return context.GetData(NavigationConstants.ViewName) ??
                   viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
        }

        public static void ClearBusy([NotNull] this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var tokens = viewModel.GetBusyTokens();
            for (int i = 0; i < tokens.Count; i++)
                tokens[i].Dispose();
        }

        public static TTask WithBusyIndicator<TTask>([NotNull] this TTask task, [NotNull] IViewModel viewModel,
            bool handleException, object message = null)
            where TTask : Task
        {
            return task.WithBusyIndicator(viewModel, message, handleException);
        }

        public static TTask WithBusyIndicator<TTask>([NotNull] this TTask task, [NotNull] IViewModel viewModel,
            object message = null, bool? handleException = null)
            where TTask : Task
        {
            Should.NotBeNull(task, "task");
            Should.NotBeNull(viewModel, "viewModel");
            if (handleException == null)
                handleException = ApplicationSettings.HandleTaskExceptionBusyIndicator;
            if (task.IsCompleted)
            {
                if (handleException.Value)
                    ToolkitExtensions.TryHandleTaskException(task, viewModel, viewModel.GetIocContainer(true));
                return task;
            }
            var token = viewModel.BeginBusy(message);
            task.TryExecuteSynchronously(t =>
            {
                token.Dispose();
                if (handleException.Value)
                    ToolkitExtensions.TryHandleTaskException(t, viewModel, viewModel.GetIocContainer(true));
            });
            return task;
        }

        public static T Wrap<T>([NotNull] this IViewModel viewModel, [CanBeNull] IDataContext context)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            return (T)viewModel
                .GetIocContainer(true)
                .Get<IWrapperManager>()
                .Wrap(viewModel, typeof(T), context);
        }

        public static T Wrap<T>([NotNull] this IViewModel viewModel, params DataConstantValue[] parameters)
            where T : IViewModel
        {
            if (parameters == null || parameters.Length == 0)
                return Wrap<T>(viewModel, DataContext.Empty);
            return Wrap<T>(viewModel, new DataContext(parameters));
        }

        public static INavigationOperation ShowAsync([NotNull] this IViewModel viewModel,
            params DataConstantValue[] parameters)
        {
            return viewModel.ShowAsync(parameters == null ? null : new DataContext(parameters));
        }

        public static INavigationOperation ShowAsync([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            return viewModel
                .GetIocContainer(true)
                .Get<IViewModelPresenter>()
                .ShowAsync(viewModel, context);
        }

        public static INavigationOperation ShowAsync([NotNull] this IViewModel viewModel, string viewName, IDataContext context = null)
        {
            return viewModel.ShowAsync(null, viewName, context);
        }

        public static INavigationOperation ShowAsync<T>([NotNull] this T viewModel,
            Action<T, IOperationResult<bool>> completeCallback, string viewName = null, IDataContext context = null)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (viewName != null)
            {
                context = context.ToNonReadOnly();
                context.AddOrUpdate(NavigationConstants.ViewName, viewName);
            }
            var operation = viewModel.ShowAsync(context);
            if (completeCallback != null)
                operation.ContinueWith(completeCallback);
            return operation;
        }

        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<IViewModel> getViewModel, params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, getViewModelGeneric: getViewModel, parameters: parameters);
        }

        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<IViewModel> getViewModel, IViewModel parentViewModel = null,
            ObservationMode? observationMode = null, params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, getViewModel, MergeParameters(parentViewModel, observationMode, parameters));
        }

        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<T> getViewModelGeneric, params DataConstantValue[] parameters)
            where T : class, IViewModel
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(getViewModelGeneric, "getViewModelGeneric");
            return (T)viewModelProvider.GetViewModel(getViewModelGeneric, new DataContext(parameters));
        }

        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<T> getViewModelGeneric, IViewModel parentViewModel = null,
            ObservationMode? observationMode = null, params DataConstantValue[] parameters) where T : class, IViewModel
        {
            return GetViewModel(viewModelProvider, getViewModelGeneric, MergeParameters(parentViewModel, observationMode, parameters));
        }

        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] Type viewModelType, params DataConstantValue[] parameters)
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(viewModelType, "viewModelType");
            return viewModelProvider.GetViewModel(viewModelType, new DataContext(parameters));
        }

        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider, [NotNull] Type viewModelType, IViewModel parentViewModel = null,
            ObservationMode? observationMode = null, params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, viewModelType, MergeParameters(parentViewModel, observationMode, parameters));
        }

        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            params DataConstantValue[] parameters) where T : IViewModel
        {
            return (T)GetViewModel(viewModelProvider, typeof(T), parameters);
        }

        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null, params DataConstantValue[] parameters) where T : IViewModel
        {
            return GetViewModel<T>(viewModelProvider, MergeParameters(parentViewModel, observationMode, parameters));
        }

        [CanBeNull]
        public static IViewModel GetParentViewModel(this IViewModel viewModel)
        {
            WeakReference reference = viewModel.Settings.Metadata.GetData(ViewModelConstants.ParentViewModel);
            if (reference == null)
                return null;
            return (IViewModel)reference.Target;
        }

        public static Task<bool> TryCloseAsync([NotNull] this IViewModel viewModel, [CanBeNull] object parameter,
            [CanBeNull] INavigationContext context, NavigationType type = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = parameter as INavigationContext ??
                          new NavigationContext(type ?? NavigationType.Undefined, NavigationMode.Back, viewModel, viewModel.GetParentViewModel(), null);
            if (parameter == null)
                parameter = context;
            //NOTE: Close view model only on back navigation.
            ICloseableViewModel closeableViewModel = context.NavigationMode == NavigationMode.Back
                ? viewModel as ICloseableViewModel
                : null;
            var navigableViewModel = viewModel as INavigableViewModel;
            if (closeableViewModel == null && navigableViewModel == null)
                return Empty.TrueTask;
            if (closeableViewModel != null && navigableViewModel != null)
            {
                Task<bool> navigatingTask = navigableViewModel.OnNavigatingFrom(context);
                if (navigatingTask.IsCompleted)
                {
                    if (navigatingTask.Result)
                        return closeableViewModel.CloseAsync(parameter);
                    return Empty.FalseTask;
                }
                return navigatingTask
                    .TryExecuteSynchronously(task =>
                    {
                        if (task.Result)
                            return closeableViewModel.CloseAsync(parameter);
                        return Empty.FalseTask;
                    }).Unwrap();
            }
            if (closeableViewModel == null)
                return navigableViewModel.OnNavigatingFrom(context);
            return closeableViewModel.CloseAsync(parameter);
        }

        public static IIocContainer GetIocContainer([NotNull] this IViewModel viewModel, bool useGlobalContainer, bool throwOnError = true)
        {
            Should.NotBeNull(viewModel, "viewModel");
            IIocContainer iocContainer = null;
            if (!viewModel.IsDisposed)
                iocContainer = viewModel.IocContainer;
            if (iocContainer == null && useGlobalContainer)
                iocContainer = ServiceProvider.IocContainer;
            if (iocContainer == null && throwOnError)
                throw ExceptionManager.ObjectNotInitialized("viewModel", viewModel);
            return iocContainer;
        }

        private static DataConstantValue[] MergeParameters(IViewModel parentViewModel, ObservationMode? observationMode, DataConstantValue[] parameters)
        {
            if (observationMode == null && parentViewModel == null)
                return parameters;

            var values = new List<DataConstantValue>();
            if (observationMode.HasValue)
                values.Add(InitializationConstants.ObservationMode.ToValue(observationMode.Value));

            if (parentViewModel != null)
                values.Add(InitializationConstants.ParentViewModel.ToValue(parentViewModel));
            if (parameters != null && parameters.Length != 0)
            {
                for (int index = 0; index < parameters.Length; index++)
                {
                    DataConstantValue parameter = parameters[index];
                    if (values.Any(value => value.DataConstant.Equals(parameter.DataConstant)))
                        continue;
                    values.Add(parameter);
                }
            }
            return values.ToArray();
        }

        #endregion

        #region Validatable view model extensions

        public static Task ValidateAsync<T>([NotNull] this T validatableViewModel, [NotNull] Func<Expression<Func<T, object>>> getMember)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.ValidateAsync(getMember.GetMemberName());
        }

        public static Task DisableValidationAsync<T>([NotNull] this T validatableViewModel, [NotNull] Func<Expression<Func<T, object>>> getMember)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.DisableValidationAsync(getMember.GetMemberName());
        }

        public static Task DisableValidationAsync([NotNull] this IValidatorAggregator validatableViewModel,
            [NotNull] string propertyName)
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            Should.NotBeNull(propertyName, "propertyName");
            validatableViewModel.ClearErrors(propertyName);
            validatableViewModel.IgnoreProperties.Add(propertyName);
            return validatableViewModel.ValidateAsync(propertyName);
        }

        public static Task EnableValidationAsync<T>([NotNull] this T validatableViewModel, [NotNull] Func<Expression<Func<T, object>>> getMember)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.EnableValidationAsync(getMember.GetMemberName());
        }

        public static Task EnableValidationAsync([NotNull] this IValidatorAggregator validatableViewModel,
            [NotNull] string propertyName)
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            Should.NotBeNull(propertyName, "propertyName");
            validatableViewModel.IgnoreProperties.Remove(propertyName);
            return validatableViewModel.ValidateAsync(propertyName);
        }

        #endregion

        #region Grid view extensions

        public static void MoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveUpItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        public static void MoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveDownItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        public static bool CanMoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            return gridViewModel.OriginalItemsSource.CanMoveUpItem(gridViewModel.SelectedItem);
        }

        public static bool CanMoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            return gridViewModel.OriginalItemsSource.CanMoveDownItem(gridViewModel.SelectedItem);
        }

        #endregion
    }
}
