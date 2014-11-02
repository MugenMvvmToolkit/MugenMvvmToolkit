#region Copyright

// ****************************************************************************
// <copyright file="ViewModelExtensions.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
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
    /// <summary>
    ///     Represents the extensions method for view models.
    /// </summary>
    public static class ViewModelExtensions
    {
        #region View models extension

        /// <summary>
        /// Gets the unique identifier for the view model.
        /// </summary>
        public static Guid GetViewModelId(this IViewModel viewModel)
        {
            return ViewModelProvider.GetOrAddViewModelId(viewModel);
        }

        /// <summary>
        ///     Returns the name of view, if any.
        /// </summary>
        public static string GetViewName([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                return viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
            return context.GetData(NavigationConstants.ViewName) ??
                   viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
        }

        /// <summary>
        ///     Invokes a task and wrap it to busy indicator.
        /// </summary>
        /// <param name="task">The specified <see cref="Task" />.</param>
        /// <param name="viewModel">The specified <see cref="IViewModel" />.</param>
        /// <param name="message">The specified message for the <c>BusyMessage</c> property.</param>
        /// <param name="handleException">
        ///     Responsible for handling errors if true errors will be processed using the
        ///     <see cref="ITaskExceptionHandler" /> interface; otherwise false
        /// </param>
        public static TTask WithBusyIndicator<TTask>([NotNull] this TTask task, [NotNull] IViewModel viewModel,
            bool handleException, object message = null)
            where TTask : Task
        {
            return task.WithBusyIndicator(viewModel, message, handleException);
        }

        /// <summary>
        ///     Invokes a task and wrap it to busy indicator.
        /// </summary>
        /// <param name="task">The specified <see cref="Task" />.</param>
        /// <param name="viewModel">The specified <see cref="IViewModel" />.</param>
        /// <param name="message">The specified message for the <c>BusyMessage</c> property.</param>
        /// <param name="handleException">
        ///     Responsible for handling errors if true errors will be processed using the
        ///     <see cref="ITaskExceptionHandler" /> interface; otherwise false
        /// </param>
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
            Guid beginBusy = viewModel.BeginBusy(message);
            task.TryExecuteSynchronously(t =>
            {
                viewModel.EndBusy(beginBusy);
                if (handleException.Value)
                    ToolkitExtensions.TryHandleTaskException(t, viewModel, viewModel.GetIocContainer(true));
            });
            return task;
        }

        /// <summary>
        ///     Wraps the specified view-model to a specified type.
        /// </summary>
        /// <param name="viewModel">The specified view-model.</param>
        /// <param name="context">The specified data context</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public static T Wrap<T>([NotNull] this IViewModel viewModel, [CanBeNull] IDataContext context)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            return (T)viewModel
                .GetIocContainer(true)
                .Get<IViewModelWrapperManager>()
                .Wrap(viewModel, typeof(T), context);
        }

        /// <summary>
        ///     Wraps the specified view-model to a specified type.
        /// </summary>
        /// <param name="viewModel">The specified view-model.</param>
        /// <param name="parameters">The specified parameters, if any.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public static T Wrap<T>([NotNull] this IViewModel viewModel, params DataConstantValue[] parameters)
            where T : IViewModel
        {
            if (parameters == null || parameters.Length == 0)
                return Wrap<T>(viewModel, DataContext.Empty);
            return Wrap<T>(viewModel, new DataContext(parameters));
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>The operation result task, this task returns the result of the operation.</returns>
        public static IAsyncOperation<bool> ShowAsync([NotNull] this IViewModel viewModel,
            params DataConstantValue[] parameters)
        {
            IDataContext context = parameters == null ? null : new DataContext(parameters);
            return viewModel.ShowAsync(context);
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>The operation result task, this task returns the result of the operation.</returns>
        public static IAsyncOperation<bool> ShowAsync([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            return viewModel
                .GetIocContainer(true)
                .Get<IViewModelPresenter>()
                .ShowAsync(viewModel, context)
                .ContinueWith(result => result.Result.GetValueOrDefault());
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="viewName">The name of view.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>The operation result task, this task returns the result of the operation.</returns>
        public static IAsyncOperation<bool> ShowAsync([NotNull] this IViewModel viewModel, string viewName,
            IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (viewName != null)
            {
                context = context.ToNonReadOnly();
                context.AddOrUpdate(NavigationConstants.ViewName, viewName);
            }
            return viewModel.ShowAsync(context);
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="completeCallback">The specified callback.</param>
        /// <param name="viewName">The name of view.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>The operation result task, this task returns the result of the operation.</returns>
        public static IAsyncOperation<bool> ShowAsync<T>([NotNull] this T viewModel,
            Action<T, IOperationResult<bool>> completeCallback, string viewName = null, IDataContext context = null)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (viewName != null)
            {
                context = context.ToNonReadOnly();
                context.AddOrUpdate(NavigationConstants.ViewName, viewName);
            }
            IAsyncOperation<bool> operation = viewModel.ShowAsync(context);
            operation.ContinueWith(completeCallback);
            return operation;
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<IViewModel> getViewModel, params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, getViewModelGeneric: getViewModel, parameters: parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<IViewModel> getViewModel, IViewModel parentViewModel = null,
            ObservationMode? observationMode = null, IocContainerCreationMode? containerCreationMode = null,
            params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, getViewModel,
                MergeParameters(parentViewModel, containerCreationMode, observationMode, parameters));
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="getViewModelGeneric">The specified delegate to create view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<T> getViewModelGeneric, params DataConstantValue[] parameters)
            where T : IViewModel
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(getViewModelGeneric, "getViewModelGeneric");
            return (T)viewModelProvider.GetViewModel(adapter => getViewModelGeneric(adapter), new DataContext(parameters));
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="getViewModelGeneric">The specified delegate to create view model.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull] GetViewModelDelegate<T> getViewModelGeneric, IViewModel parentViewModel = null,
            ObservationMode? observationMode = null, IocContainerCreationMode? containerCreationMode = null,
            params DataConstantValue[] parameters) where T : IViewModel
        {
            return GetViewModel(viewModelProvider, getViewModelGeneric,
                MergeParameters(parentViewModel, containerCreationMode, observationMode, parameters));
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull, ViewModelTypeRequired] Type viewModelType, params DataConstantValue[] parameters)
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(viewModelType, "viewModelType");
            return viewModelProvider.GetViewModel(viewModelType, new DataContext(parameters));
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static IViewModel GetViewModel([NotNull] this IViewModelProvider viewModelProvider,
            [NotNull, ViewModelTypeRequired] Type viewModelType,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
        {
            return GetViewModel(viewModelProvider, viewModelType,
                MergeParameters(parentViewModel, containerCreationMode, observationMode, parameters));
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <typeparam name="T">The type of view model.</typeparam>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            params DataConstantValue[] parameters) where T : IViewModel
        {
            return (T)GetViewModel(viewModelProvider, typeof(T), parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <typeparam name="T">The type of view model.</typeparam>
        /// <param name="viewModelProvider">The specified <see cref="IViewModelProvider" />.</param>
        /// <param name="parentViewModel">The parent view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        public static T GetViewModel<T>([NotNull] this IViewModelProvider viewModelProvider,
            IViewModel parentViewModel = null, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters) where T : IViewModel
        {
            return GetViewModel<T>(viewModelProvider,
                MergeParameters(parentViewModel, containerCreationMode, observationMode, parameters));
        }

        /// <summary>
        ///     Tries to get parent view model, the result value can be null.
        /// </summary>
        [CanBeNull]
        public static IViewModel GetParentViewModel(this IViewModel viewModel)
        {
            WeakReference reference = viewModel.Settings.Metadata.GetData(ViewModelConstants.ParentViewModel);
            if (reference == null)
                return null;
            return (IViewModel)reference.Target;
        }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        public static Task<bool> TryCloseAsync([NotNull] this IViewModel viewModel, [CanBeNull] object parameter,
            [CanBeNull] INavigationContext context)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = parameter as INavigationContext ??
                          new NavigationContext(NavigationMode.Back, viewModel, viewModel.GetParentViewModel(), null);
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

        /// <summary>
        ///     Tries to get an instance of <see cref="IIocContainer" /> from the <see cref="IViewModel" /> or from
        ///     <see cref="ServiceProvider" />.
        /// </summary>
        public static IIocContainer GetIocContainer([NotNull] this IViewModel viewModel, bool useGlobalContainer,
            bool throwOnError = true)
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

        private static DataConstantValue[] MergeParameters(IViewModel parentViewModel,
            IocContainerCreationMode? containerCreationMode, ObservationMode? observationMode, DataConstantValue[] parameters)
        {
            if (observationMode == null && containerCreationMode == null && parentViewModel == null)
                return parameters;

            var values = new List<DataConstantValue>();
            if (containerCreationMode.HasValue)
                values.Add(InitializationConstants.IocContainerCreationMode.ToValue(containerCreationMode.Value));
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

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static Task ValidateAsync<T, TValue>([NotNull] this T validatableViewModel,
            [NotNull] Expression<Func<T, TValue>> getProperty)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.ValidateAsync(ToolkitExtensions.GetMemberName(getProperty));
        }

        /// <summary>
        ///     Disable validation for the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static Task DisableValidationAsync<T, TValue>([NotNull] this T validatableViewModel,
            [NotNull] Expression<Func<T, TValue>> getProperty)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.DisableValidationAsync(ToolkitExtensions.GetMemberName(getProperty));
        }

        /// <summary>
        ///     Disable validation for the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static Task DisableValidationAsync([NotNull] this IValidatorAggregator validatableViewModel,
            [NotNull] string propertyName)
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            Should.NotBeNull(propertyName, "propertyName");
            validatableViewModel.ClearErrors(propertyName);
            validatableViewModel.IgnoreProperties.Add(propertyName);
            return validatableViewModel.ValidateAsync(propertyName);
        }

        /// <summary>
        ///     Enable validation for the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static Task EnableValidationAsync<T, TValue>([NotNull] this T validatableViewModel,
            [NotNull] Expression<Func<T, TValue>> getProperty)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, "validatableViewModel");
            return validatableViewModel.EnableValidationAsync(ToolkitExtensions.GetMemberName(getProperty));
        }

        /// <summary>
        ///     Enable validation for the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
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

        /// <summary>
        ///     Moves up the selected item in the specified <see cref="IGridViewModel" />.
        /// </summary>
        public static void MoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveUpItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        /// <summary>
        ///     Moves down the selected item in the specified <see cref="IGridViewModel" />.
        /// </summary>
        public static void MoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveDownItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        /// <summary>
        ///     Determines whether the view model can move up the selected item.
        /// </summary>
        public static bool CanMoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            return gridViewModel.OriginalItemsSource.CanMoveUpItem(gridViewModel.SelectedItem);
        }

        /// <summary>
        ///     Determines whether the view model can move down the selected item.
        /// </summary>
        public static bool CanMoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, "gridViewModel");
            return gridViewModel.OriginalItemsSource.CanMoveDownItem(gridViewModel.SelectedItem);
        }

        #endregion
    }
}