using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class CommandMediatorProvider : ComponentOwnerBase<ICommandMediatorProvider>, ICommandMediatorProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CommandMediatorProvider(IComponentCollectionProvider componentCollectionProvider) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ICommandMediator GetCommandMediator<TParameter>(ICommand command, Delegate execute, Delegate canExecute, IReadOnlyCollection<object> notifiers,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(execute, nameof(execute));
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetExecutorMediatorInternal<TParameter>(command, execute, canExecute, notifiers, metadata);

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(ICommandMediatorProviderComponent).Name);

            OnMediatorCreated<TParameter>(result, command, execute, canExecute, notifiers, metadata);

            return result;
        }

        #endregion

        #region Methods

        protected virtual ICommandMediator GetExecutorMediatorInternal<TParameter>(ICommand command, Delegate execute, Delegate? canExecute,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            ICommandMediator? result = null;
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ICommandMediatorProviderComponent executorFactory)
                {
                    result = executorFactory.TryGetCommandMediator<TParameter>(command, execute, canExecute, notifiers, metadata);
                    if (result != null)
                        break;
                }
            }

            return result;
        }

        protected virtual void OnMediatorCreated<TParameter>(ICommandMediator mediator, ICommand command, Delegate execute,
            Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ICommandMediatorProviderListener listener)
                    listener.OnCommandMediatorCreated<TParameter>(this, mediator, command, execute, canExecute, notifiers, metadata);
            }
        }

        #endregion
    }
}