#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelWindowPresenter.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public class DynamicViewModelWindowPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        private readonly List<Func<IViewModel, Type, IDataContext, IWindowViewMediator>> _mediatorRegistrations;

        private readonly IOperationCallbackManager _callbackManager;
        private readonly IWrapperManager _wrapperManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private Task _currentTask;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DynamicViewModelWindowPresenter([NotNull] IViewMappingProvider viewMappingProvider, [NotNull] IOperationCallbackManager callbackManager, [NotNull] IWrapperManager wrapperManager)
        {
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            _viewMappingProvider = viewMappingProvider;
            _callbackManager = callbackManager;
            _wrapperManager = wrapperManager;
            _mediatorRegistrations = new List<Func<IViewModel, Type, IDataContext, IWindowViewMediator>>();
        }

        #endregion

        #region Properties

        protected IViewMappingProvider ViewMappingProvider => _viewMappingProvider;

        protected IOperationCallbackManager CallbackManager => _callbackManager;

        protected IWrapperManager WrapperManager => _wrapperManager;

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public virtual int Priority => ViewModelPresenter.DefaultWindowPresenterPriority;

        public IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;

            var viewMediator = TryCreateMediator(viewModel, context);
            if (viewMediator == null)
                return null;
            var tcs = new TaskCompletionSource<object>();
            var operation = new AsyncOperation<object>();
            operation.Context.AddOrUpdate(NavigationConstants.NavigationCompletedTask, tcs.Task);

            if (_currentTask == null)
                Show(viewMediator, operation, context, tcs);
            else
                _currentTask.TryExecuteSynchronously(_ => Show(viewMediator, operation, context, tcs));
            return operation;
        }

        public Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            var mediator = viewModel?.Settings.Metadata.GetData(WindowPresenterConstants.WindowViewMediator);
            return mediator?.CloseAsync(context);
        }

        public bool Restore(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var view = context.GetData(WindowPresenterConstants.RestoredView);
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (view == null || viewModel == null)
                return false;
            var mediator = TryCreateMediator(viewModel, context);
            if (mediator == null)
                return false;
            mediator.UpdateView(view, context.GetData(WindowPresenterConstants.IsViewOpened), context);
            return true;
        }

        #endregion

        #region Methods

        public void RegisterMediatorFactory<TMediator, TView>(bool viewExactlyEqual = false)
            where TMediator : WindowViewMediatorBase<TView>
            where TView : class
        {
            RegisterMediatorFactory(typeof(TMediator), typeof(TView), viewExactlyEqual);
        }

        public void RegisterMediatorFactory(Type mediatorType, Type viewType, bool viewExactlyEqual)
        {
            if (viewExactlyEqual)
            {
                RegisterMediatorFactory((vm, type, arg3) =>
                {
                    if (type == viewType)
                        return (IWindowViewMediator)vm.GetIocContainer(true).Get(mediatorType);
                    return null;
                });
            }
            else
            {
                RegisterMediatorFactory((vm, type, arg3) =>
                {
                    if (viewType.IsAssignableFrom(type) || WrapperManager.CanWrap(type, viewType, arg3))
                        return (IWindowViewMediator)vm.GetIocContainer(true).Get(mediatorType);
                    return null;
                });
            }
        }

        public void RegisterMediatorFactory([NotNull] Func<IViewModel, Type, IDataContext, IWindowViewMediator> mediatorFactory)
        {
            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
            lock (_mediatorRegistrations)
                _mediatorRegistrations.Add(mediatorFactory);
        }

        [CanBeNull]
        protected IWindowViewMediator TryCreateMediatorFromFactory([NotNull] IViewModel viewModel, Type viewType, [NotNull] IDataContext context)
        {
            lock (_mediatorRegistrations)
            {
                for (int i = 0; i < _mediatorRegistrations.Count; i++)
                {
                    var mediator = _mediatorRegistrations[i].Invoke(viewModel, viewType, context);
                    if (mediator != null)
                        return mediator;
                }
            }
            return null;
        }

        [CanBeNull]
        protected virtual IWindowViewMediator CreateWindowViewMediator([NotNull] IViewModel viewModel, Type viewType, [NotNull] IDataContext context)
        {
            return TryCreateMediatorFromFactory(viewModel, viewType, context);
        }

        private void Show(IWindowViewMediator viewMediator, IAsyncOperation operation, IDataContext context, TaskCompletionSource<object> tcs)
        {
            try
            {
                CallbackManager.Register(OperationType.WindowNavigation, viewMediator.ViewModel, operation.ToOperationCallback(), context);
                var task = viewMediator.ShowAsync(context);
                _currentTask = task;
                tcs.TrySetFromTask(task);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                throw;
            }
        }

        private IWindowViewMediator TryCreateMediator(IViewModel viewModel, IDataContext context)
        {
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressWindowNavigation, out data) && data)
                return null;

            var viewName = viewModel.GetViewName(context);
            IViewMappingItem mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            if (mappingItem == null)
                return null;

            IWindowViewMediator viewMediator;
            if (!viewModel.Settings.Metadata.TryGetData(WindowPresenterConstants.WindowViewMediator, out viewMediator))
            {
                viewMediator = CreateWindowViewMediator(viewModel, mappingItem.ViewType, context);
                if (viewMediator != null)
                {
                    viewMediator.Initialize(viewModel, context);
                    viewModel.Settings.Metadata.AddOrUpdate(WindowPresenterConstants.WindowViewMediator, viewMediator);
                }
            }
            return viewMediator;
        }

        #endregion
    }
}
