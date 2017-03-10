#region Copyright

// ****************************************************************************
// <copyright file="ViewModelExtensions.cs">
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
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;

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

        public static string GetViewName([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (context == null)
                return viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
            return context.GetData(NavigationConstants.ViewName) ??
                   viewModel.Settings.Metadata.GetData(InitializationConstants.ViewName);
        }

        [CanBeNull]
        public static IViewModel GetParentViewModel([NotNull] this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return (IViewModel)viewModel.Settings.Metadata.GetData(ViewModelConstants.ParentViewModel)?.Target;
        }

        [CanBeNull]
        public static TView GetCurrentView<TView>([NotNull] this IViewModel viewModel, bool underlyingView = true)
            where TView : class
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var view = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            if (view == null || !underlyingView)
                return (TView)view;
            return ToolkitExtensions.GetUnderlyingView<TView>(view);
        }

        public static void ClearBusy([NotNull] this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var tokens = viewModel.GetBusyTokens();
            for (int i = 0; i < tokens.Count; i++)
                tokens[i].Dispose();
        }

        public static TTask WithBusyIndicator<TTask>([NotNull] this TTask task, [NotNull] IViewModel viewModel, object message = null)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (task.IsCompleted)
                return task;
            var token = viewModel.BeginBusy(message);
#if NET4
            task.TryExecuteSynchronously(t => token.Dispose());
#else
            task.ContinueWith((t, o) => ((IBusyToken)o).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
#endif            
            return task;
        }

        public static T Wrap<T>([NotNull] this IViewModel viewModel, [CanBeNull] IDataContext context)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
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

        public static IAsyncOperation ShowAsync([NotNull] this IViewModel viewModel, params DataConstantValue[] parameters)
        {
            return viewModel.ShowAsync(parameters == null ? null : new DataContext(parameters));
        }

        public static IAsyncOperation ShowAsync([NotNull] this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            return viewModel
                .GetIocContainer(true)
                .Get<IViewModelPresenter>()
                .ShowAsync(context);
        }

        public static IAsyncOperation<TResult> ShowAsync<TResult>([NotNull] this IHasResultViewModel<TResult> viewModel, params DataConstantValue[] parameters)
        {
            return viewModel.ShowAsync(parameters == null ? null : new DataContext(parameters));
        }

        public static IAsyncOperation<TResult> ShowAsync<TResult>([NotNull] this IHasResultViewModel<TResult> viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            return viewModel
                .GetIocContainer(true)
                .Get<IViewModelPresenter>()
                .ShowAsync(context)
                .ContinueWith<IHasResultViewModel<TResult>, TResult>((vm, result) => vm.Result);
        }

        public static IAsyncOperation ShowAsync([NotNull] this IViewModel viewModel, string viewName, IDataContext context = null)
        {
            return viewModel.ShowAsync(null, viewName, context);
        }

        public static IAsyncOperation ShowAsync<T>([NotNull] this T viewModel,
            Action<T, IOperationResult> completeCallback, string viewName = null, IDataContext context = null)
            where T : IViewModel
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
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

        public static Task<bool> CloseAsync([NotNull]this IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            return viewModel.GetIocContainer(true).Get<IViewModelPresenter>().CloseAsync(context);
        }

        public static Task<bool> CloseAsync([NotNull]this IViewModel viewModel, [CanBeNull] object parameter)
        {
            IDataContext context = null;
            if (parameter != null)
            {
                context = new DataContext();
                context.Add(NavigationConstants.CloseParameter, parameter);
            }
            return viewModel.CloseAsync(context);
        }

        public static void AddClosingHandler([NotNull]this IViewModel viewModel, EventHandler<IViewModel, ViewModelClosingEventArgs> handler)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var eventHandler = viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosingEvent) + handler;
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.ClosingEvent, eventHandler);
        }

        public static void RemoveClosingHandler([NotNull]this IViewModel viewModel, EventHandler<IViewModel, ViewModelClosingEventArgs> handler)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var eventHandler = viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosingEvent) - handler;
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.ClosingEvent, eventHandler);
        }

        public static void AddClosedHandler([NotNull]this IViewModel viewModel, EventHandler<IViewModel, ViewModelClosedEventArgs> handler)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var eventHandler = viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosedEvent) + handler;
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.ClosedEvent, eventHandler);
        }

        public static void RemoveClosedHandler([NotNull]this IViewModel viewModel, EventHandler<IViewModel, ViewModelClosedEventArgs> handler)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var eventHandler = viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosedEvent) - handler;
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.ClosedEvent, eventHandler);
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
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(getViewModelGeneric, nameof(getViewModelGeneric));
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
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(viewModelType, nameof(viewModelType));
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

        public static IIocContainer GetIocContainer([NotNull] this IViewModel viewModel, bool useGlobalContainer, bool throwOnError = true)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            IIocContainer iocContainer = null;
            if (!viewModel.IsDisposed)
                iocContainer = viewModel.IocContainer;
            if (iocContainer == null && useGlobalContainer)
                iocContainer = ServiceProvider.IocContainer;
            if (iocContainer == null && throwOnError)
                throw ExceptionManager.ObjectNotInitialized("viewModel", viewModel);
            return iocContainer;
        }

        public static void InvalidateCommands(this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Publish(viewModel, StateChangedMessage.Empty);
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
            Should.NotBeNull(validatableViewModel, nameof(validatableViewModel));
            return validatableViewModel.ValidateAsync(getMember.GetMemberName());
        }

        public static Task DisableValidationAsync<T>([NotNull] this T validatableViewModel, [NotNull] Func<Expression<Func<T, object>>> getMember)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, nameof(validatableViewModel));
            return validatableViewModel.DisableValidationAsync(getMember.GetMemberName());
        }

        public static Task DisableValidationAsync([NotNull] this IValidatorAggregator validatableViewModel,
            [NotNull] string propertyName)
        {
            Should.NotBeNull(validatableViewModel, nameof(validatableViewModel));
            Should.NotBeNull(propertyName, nameof(propertyName));
            validatableViewModel.ClearErrors(propertyName);
            validatableViewModel.IgnoreProperties.Add(propertyName);
            return validatableViewModel.ValidateAsync(propertyName);
        }

        public static Task EnableValidationAsync<T>([NotNull] this T validatableViewModel, [NotNull] Func<Expression<Func<T, object>>> getMember)
            where T : IValidatorAggregator
        {
            Should.NotBeNull(validatableViewModel, nameof(validatableViewModel));
            return validatableViewModel.EnableValidationAsync(getMember.GetMemberName());
        }

        public static Task EnableValidationAsync([NotNull] this IValidatorAggregator validatableViewModel,
            [NotNull] string propertyName)
        {
            Should.NotBeNull(validatableViewModel, nameof(validatableViewModel));
            Should.NotBeNull(propertyName, nameof(propertyName));
            validatableViewModel.IgnoreProperties.Remove(propertyName);
            return validatableViewModel.ValidateAsync(propertyName);
        }

        #endregion

        #region Grid view extensions

        public static void MoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, nameof(gridViewModel));
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveUpItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        public static void MoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, nameof(gridViewModel));
            object selectedItem = gridViewModel.SelectedItem;
            if (gridViewModel.OriginalItemsSource.MoveDownItem(selectedItem))
                gridViewModel.SelectedItem = selectedItem;
        }

        public static bool CanMoveUpSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, nameof(gridViewModel));
            return gridViewModel.OriginalItemsSource.CanMoveUpItem(gridViewModel.SelectedItem);
        }

        public static bool CanMoveDownSelectedItem([NotNull] this IGridViewModel gridViewModel)
        {
            Should.NotBeNull(gridViewModel, nameof(gridViewModel));
            return gridViewModel.OriginalItemsSource.CanMoveDownItem(gridViewModel.SelectedItem);
        }

        #endregion
    }
}
