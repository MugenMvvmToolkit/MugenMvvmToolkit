using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class MediatorCommand : IMediatorCommand, IWeakReferenceHolder
    {
        #region Fields

        private readonly Func<ICommand, IReadOnlyMetadataContext, ICommandMediator> _getMediator;
        private IReadOnlyMetadataContext? _metadata;
        private ICommandMediator? _mediator;

        #endregion

        #region Constructors

        private MediatorCommand(IReadOnlyMetadataContext metadata, Func<ICommand, IReadOnlyMetadataContext, ICommandMediator> getMediator)
        {
            _metadata = metadata;
            _getMediator = getMediator;
        }

        #endregion

        #region Properties

        public bool HasCanExecute => Mediator.HasCanExecute();

        public ICommandMediator Mediator
        {
            get
            {
                if (_mediator == null)
                {
                    var metadata = _metadata;
                    _metadata = null;
                    if (metadata != null)
                        MugenExtensions.LazyInitializeDisposable(ref _mediator, _getMediator(this, metadata));
                }
                return _mediator!;
            }
        }

        IWeakReference? IWeakReferenceHolder.WeakReference { get; set; }

        public bool IsSuspended => Mediator.IsSuspended;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => Mediator.AddCanExecuteChanged(value);
            remove => Mediator.RemoveCanExecuteChanged(value);
        }

        #endregion

        #region Implementation of interfaces

        public void RaiseCanExecuteChanged()
        {
            Mediator.RaiseCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
            return Mediator.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Mediator.ExecuteAsync(parameter);
        }

        public void Dispose()
        {
            Mediator.Dispose();
        }

        public IDisposable Suspend()
        {
            return Mediator.Suspend();
        }

        #endregion

        #region Methods

        public static MediatorCommand Create(Action execute, IReadOnlyMetadataContext? metadata = null)
        {
            return Create(execute, null, null, null, metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return Create(execute, null, null, null, metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata,
            params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return CreateFromTask(execute, null, null, null, metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata,
            params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return CreateFromTask(execute, null, null, null, metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata,
            params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool? allowMultipleExecution, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            request.Set(MediatorCommandMetadata.Execute, execute);
            if (canExecute != null)
                request.Set(MediatorCommandMetadata.CanExecute, canExecute);
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return new MediatorCommand(request, DelegateInvoker<object>.Invoker);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool? allowMultipleExecution, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            request.Set(MediatorCommandMetadata.Execute, execute);
            if (canExecute != null)
                request.Set(MediatorCommandMetadata.CanExecute, canExecute);
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return new MediatorCommand(request, DelegateInvoker<T>.Invoker);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool? allowMultipleExecution, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            request.Set(MediatorCommandMetadata.Execute, execute);
            if (canExecute != null)
                request.Set(MediatorCommandMetadata.CanExecute, canExecute);
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return new MediatorCommand(request, DelegateInvoker<object>.Invoker);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool? allowMultipleExecution, IReadOnlyCollection<object>? notifiers,
            IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            request.Set(MediatorCommandMetadata.Execute, execute);
            if (canExecute != null)
                request.Set(MediatorCommandMetadata.CanExecute, canExecute);
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return new MediatorCommand(request, DelegateInvoker<T>.Invoker);
        }

        #endregion

        #region Nested types

        private static class DelegateInvoker<T>
        {
            #region Fields

            public static readonly Func<ICommand, IReadOnlyMetadataContext, ICommandMediator> Invoker = (command, context) => Service<ICommandMediatorProvider>.Instance.GetCommandMediator<T>(command, context);

            #endregion
        }

        #endregion
    }
}