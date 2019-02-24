using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommandDispatcher : HasListenersBase<IRelayCommandDispatcherListener>, IRelayCommandDispatcher
    {
        #region Fields

        private IExecutorRelayCommandMediatorFactory _executorMediatorFactory;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public RelayCommandDispatcher()
        {
            MediatorFactories = new List<IRelayCommandMediatorFactory>();
        }

        #endregion

        #region Properties

        protected List<IRelayCommandMediatorFactory> MediatorFactories { get; }

        public IExecutorRelayCommandMediatorFactory ExecutorMediatorFactory
        {
            get => GetExecutorMediatorFactoryInternal();
            set
            {
                Should.NotBeNull(value, nameof(ExecutorMediatorFactory));
                SetExecutorMediatorFactoryInternal(value);
            }
        }

        #endregion

        #region Implementation of interfaces

        public void AddMediatorFactory(IRelayCommandMediatorFactory factory)
        {
            Should.NotBeNull(factory, nameof(factory));
            AddMediatorFactoryInternal(factory);
        }

        public void RemoveMediatorFactory(IRelayCommandMediatorFactory factory)
        {
            Should.NotBeNull(factory, nameof(factory));
            RemoveMediatorFactoryInternal(factory);
        }

        public IReadOnlyList<IRelayCommandMediatorFactory> GetMediatorFactories()
        {
            return GetMediatorFactoriesInternal();
        }

        public IExecutorRelayCommandMediator GetExecutorMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(relayCommand, nameof(relayCommand));
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetExecutorMediatorInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
        }

        #endregion

        #region Methods

        protected virtual IExecutorRelayCommandMediatorFactory GetExecutorMediatorFactoryInternal()
        {
            return _executorMediatorFactory;
        }

        protected virtual void SetExecutorMediatorFactoryInternal(IExecutorRelayCommandMediatorFactory factory)
        {
            _executorMediatorFactory = factory;
        }

        protected virtual void AddMediatorFactoryInternal(IRelayCommandMediatorFactory factory)
        {
            lock (MediatorFactories)
            {
                MediatorFactories.Add(factory);
            }
        }

        protected virtual void RemoveMediatorFactoryInternal(IRelayCommandMediatorFactory factory)
        {
            lock (MediatorFactories)
            {
                MediatorFactories.Remove(factory);
            }
        }

        protected virtual IReadOnlyList<IRelayCommandMediatorFactory> GetMediatorFactoriesInternal()
        {
            lock (MediatorFactories)
            {
                return MediatorFactories.ToArray();
            }
        }

        protected virtual IExecutorRelayCommandMediator GetExecutorMediatorInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            var mediatorFactory = ExecutorMediatorFactory;
            Should.NotBeNull(mediatorFactory, nameof(ExecutorMediatorFactory));
            var mediators = GetMediatorsInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
            var mediator = mediatorFactory.GetExecutorMediator<TParameter>(this, relayCommand, mediators, execute, canExecute, notifiers, metadata);

            var listeners = GetListenersInternal();
            for (int i = 0; i < listeners.Length; i++) 
                listeners[i]?.OnMediatorCreated(this, relayCommand, execute, canExecute, notifiers, metadata, mediator);
            return mediator;
        }

        protected virtual IReadOnlyList<IRelayCommandMediator> GetMediatorsInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            List<IRelayCommandMediator>? result = null;
            lock (MediatorFactories)
            {
                for (var i = 0; i < MediatorFactories.Count; i++)
                {
                    var mediator = MediatorFactories[i].TryGetMediator<TParameter>(this, relayCommand, execute, canExecute, notifiers, metadata);
                    if (mediator == null)
                        continue;
                    if (result == null)
                        result = new List<IRelayCommandMediator>();
                    result.Add(mediator);
                }
            }

            if (result == null)
                return Default.EmptyArray<IRelayCommandMediator>();
            return result;
        }

        #endregion
    }
}