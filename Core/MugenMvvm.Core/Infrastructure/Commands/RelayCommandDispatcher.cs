using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Commands.Mediators;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommandDispatcher : IRelayCommandDispatcher
    {
        #region Fields

        private readonly IReflectionManager _reflectionManager;

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
            _reflectionManager = reflectionManager;
            ThreadDispatcher = threadDispatcher;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            EventThreadMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        protected IThreadDispatcher ThreadDispatcher { get; }

        public bool AllowMultipleExecution { get; set; }

        public CommandExecutionMode CommandExecutionMode { get; set; }

        public ThreadExecutionMode EventThreadMode { get; set; }

        #endregion

        #region Implementation of interfaces

        public IRelayCommandMediator GetMediator(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMediatorInternal<object>(execute, canExecute, notifiers, metadata);
        }

        public void CleanupCommands(object target, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(metadata, nameof(metadata));
            CleanupCommandsInternal(target, metadata);
        }

        #endregion

        #region Methods

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
                            var func = _reflectionManager.GetMemberGetter<ICommand>(p);
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

        protected virtual IRelayCommandMediator GetMediatorInternal<T>(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            var mediator = CreateExecutorMediator<T>(execute, canExecute, notifiers, metadata);
            mediator = WrapCanExecuteMediator<T>(mediator, canExecute, notifiers, metadata);
            return WrapMediator<T>(mediator, execute, canExecute, notifiers, metadata);
        }

        protected virtual IRelayCommandMediator CreateExecutorMediator<T>(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            return new ExecutorRelayCommandMediator<T>(execute, metadata.Get(RelayCommandMetadata.ExecutionMode, CommandExecutionMode));
        }

        protected virtual IRelayCommandMediator WrapCanExecuteMediator<T>(IRelayCommandMediator mediator, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            if (canExecute == null)
                return mediator;
            return new CanExecuteRelayCommandMediator<T>(mediator, ThreadDispatcher, metadata.Get(RelayCommandMetadata.EventThreadMode, EventThreadMode), canExecute,
                notifiers ?? Default.EmptyArray<object>(), metadata.Get(RelayCommandMetadata.IgnoreProperties));
        }

        protected virtual IRelayCommandMediator WrapMediator<T>(IRelayCommandMediator mediator, Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext metadata)
        {
            if (metadata.Get(RelayCommandMetadata.AllowMultipleExecution, AllowMultipleExecution))
                mediator = new DisableMultipleExecutionRelayCommandWrapper(mediator);
            var displayName = metadata.Get(RelayCommandMetadata.DisplayName);
            if (displayName != null || canExecute != null)
                mediator = new BindableRelayCommandMediator(mediator, ThreadDispatcher, metadata.Get(RelayCommandMetadata.EventThreadMode, EventThreadMode), displayName);
            return mediator;
        }

        #endregion
    }
}