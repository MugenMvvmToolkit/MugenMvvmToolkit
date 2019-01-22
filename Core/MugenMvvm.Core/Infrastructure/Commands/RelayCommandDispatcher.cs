using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Commands.Mediators;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommandDispatcher : IRelayCommandDispatcher
    {
        #region Fields

        private static readonly Dictionary<Type, Func<object, ICommand>[]> TypesToCommandsProperties;

        #endregion

        #region Constructors

        static RelayCommandDispatcher()
        {
            TypesToCommandsProperties = new Dictionary<Type, Func<object, ICommand>[]>(MemberInfoComparer.Instance);
        }

        [Preserve(Conditional = true)]
        public RelayCommandDispatcher(IThreadDispatcher threadDispatcher, IReflectionManager reflectionManager)
        {
            ReflectionManager = reflectionManager;
            ThreadDispatcher = threadDispatcher;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
            MediatorFactories = new List<Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?>>
            {
                DefaultConditionEventFactory
            };
        }

        #endregion

        #region Properties

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        protected IReflectionManager ReflectionManager { get; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected List<Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?>> MediatorFactories { get; }

        #endregion

        #region Implementation of interfaces

        public IExecutorRelayCommandMediator GetMediator<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(relayCommand, nameof(relayCommand));
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetExecutorMediatorInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
        }

        public void AddMediatorFactory(Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory)
        {
            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
            AddMediatorFactoryInternal(mediatorFactory);
        }

        public void RemoveMediatorFactory(Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory)
        {
            Should.NotBeNull(mediatorFactory, nameof(mediatorFactory));
            RemoveMediatorFactoryInternal(mediatorFactory);
        }

        public void ClearMediatorFactories()
        {
            ClearMediatorFactoriesInternal();
        }

        public void CleanupCommands(object target, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(metadata, nameof(metadata));
            CleanupCommandsInternal(target, metadata);
        }

        #endregion

        #region Methods

        protected virtual IExecutorRelayCommandMediator GetExecutorMediatorInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            var mediators = GetMediatorsInternal<TParameter>(relayCommand, execute, canExecute, notifiers, metadata);
            return new ExecutorRelayCommandMediator<TParameter>(execute, canExecute, metadata.Get(RelayCommandMetadata.ExecutionMode, CommandExecutionMode),
                metadata.Get(RelayCommandMetadata.AllowMultipleExecution, AllowMultipleExecution), mediators);
        }

        protected virtual IReadOnlyList<IRelayCommandMediator> GetMediatorsInternal<TParameter>(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            List<IRelayCommandMediator>? result = null;
            lock (MediatorFactories)
            {
                for (var i = 0; i < MediatorFactories.Count; i++)
                {
                    var mediator = MediatorFactories[i].Invoke(relayCommand, execute, canExecute, notifiers, metadata);
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

        protected virtual void AddMediatorFactoryInternal(
            Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory)
        {
            MediatorFactories.Add(mediatorFactory);
        }

        protected virtual void RemoveMediatorFactoryInternal(
            Func<IRelayCommand, Delegate, Delegate, IReadOnlyCollection<object>?, IReadOnlyMetadataContext, IRelayCommandMediator?> mediatorFactory)
        {
            MediatorFactories.Remove(mediatorFactory);
        }

        protected virtual void ClearMediatorFactoriesInternal()
        {
            MediatorFactories.Clear();
        }

        protected virtual void CleanupCommandsInternal(object target, IReadOnlyMetadataContext metadata)
        {
            Func<object, ICommand>[] list;
            var type = target.GetType();
            lock (TypesToCommandsProperties)
            {
                if (!TypesToCommandsProperties.TryGetValue(type, out list))
                {
                    List<Func<object, ICommand>> items = null;
                    foreach (var p in type.GetPropertiesUnified(MemberFlags.InstanceOnly))
                    {
                        if (typeof(ICommand).IsAssignableFromUnified(p.PropertyType) && p.CanRead && p.GetIndexParameters().Length == 0)
                        {
                            var func = ReflectionManager.GetMemberGetter<ICommand>(p);
                            if (items == null)
                                items = new List<Func<object, ICommand>>();
                            items.Add(func);
                        }
                    }

                    list = items == null ? Default.EmptyArray<Func<object, ICommand>>() : items.ToArray();
                    TypesToCommandsProperties[type] = list;
                }
            }

            if (list.Length == 0)
                return;
            for (var index = 0; index < list.Length; index++)
            {
                try
                {
                    (list[index].Invoke(target) as IDisposable)?.Dispose();
                }
                catch (Exception)
                {
                    //To avoid method access exception.
                }
            }
        }

        private IRelayCommandMediator DefaultConditionEventFactory(IRelayCommand relayCommand, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            if (notifiers == null || notifiers.Count == 0)
                return null;
            return new ConditionEventRelayCommandMediator(ThreadDispatcher, notifiers, metadata.Get(RelayCommandMetadata.IgnoreProperties),
                metadata.Get(RelayCommandMetadata.EventThreadMode, EventThreadMode), relayCommand);
        }

        #endregion
    }
}