using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommandMediatorProvider : IRelayCommandMediatorProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IRelayCommandMediatorProviderListener>? _listeners;
        private IComponentCollection<IRelayCommandMediatorFactory>? _mediatorFactories;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public RelayCommandMediatorProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IRelayCommandMediatorFactory> MediatorFactories
        {
            get
            {
                if (_mediatorFactories == null)
                    _componentCollectionProvider.LazyInitialize(ref _mediatorFactories, this);
                return _mediatorFactories;
            }
        }

        public IComponentCollection<IRelayCommandMediatorProviderListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _componentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(relayCommand, nameof(relayCommand));
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetExecutorMediatorInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IExecutorRelayCommandMediatorFactory).Name);

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMediatorCreated(this, relayCommand, execute, canExecute, notifiers, metadata, result);

            return result;
        }

        #endregion

        #region Methods

        protected virtual IExecutorRelayCommandMediator GetExecutorMediatorInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            var mediators = GetMediatorsInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
            IExecutorRelayCommandMediator? result = null;
            var mediatorFactories = MediatorFactories.GetItems();
            for (var i = 0; i < mediatorFactories.Length; i++)
            {
                if (mediatorFactories[i] is IExecutorRelayCommandMediatorFactory executorFactory)
                {
                    result = executorFactory.TryGetExecutorMediator<TParameter>(this, relayCommand, mediators, execute, canExecute, notifiers, metadata);
                    if (result != null)
                        break;
                }
            }

            return result;
        }

        protected virtual IReadOnlyList<IRelayCommandMediator> GetMediatorsInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            List<IRelayCommandMediator>? result = null;
            var mediatorFactories = MediatorFactories.GetItems();
            for (var i = 0; i < mediatorFactories.Length; i++)
            {
                var mediators = mediatorFactories[i].GetMediators<TParameter>(this, relayCommand, execute, canExecute, notifiers, metadata);
                if (mediators == null || mediators.Count == 0)
                    continue;
                if (result == null)
                    result = new List<IRelayCommandMediator>();
                result.AddRange(mediators);
            }

            if (result == null)
                return Default.EmptyArray<IRelayCommandMediator>();
            result.Sort(HasPriorityComparer.Instance);
            return result;
        }

        protected IRelayCommandMediatorProviderListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}