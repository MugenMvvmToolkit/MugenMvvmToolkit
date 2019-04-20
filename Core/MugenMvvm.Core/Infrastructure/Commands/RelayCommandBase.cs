using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Commands
{
    public abstract class RelayCommandBase<T> : IRelayCommand, IHasWeakReference
    {
        #region Fields

        private WeakReference? _ref;

        #endregion

        #region Constructors

        protected RelayCommandBase(Action execute, Func<bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
            : this(execute, canExecute, notifiers, metadataBase: metadata)
        {
        }

        protected RelayCommandBase(Action<T> execute, Func<T, bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
            : this(execute, canExecute, notifiers, metadataBase: metadata)
        {
        }

        protected RelayCommandBase(Func<Task> execute, Func<bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
            : this(execute, canExecute, notifiers, metadataBase: metadata)
        {
        }

        protected RelayCommandBase(Func<T, Task> execute, Func<T, bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
            : this(execute, canExecute, notifiers, metadataBase: metadata)
        {
        }

        protected RelayCommandBase(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadataBase)
        {
            Mediator = Service<IRelayCommandMediatorProvider>.Instance.GetExecutorMediator<T>(this, execute, canExecute, notifiers, metadataBase ?? Default.MetadataContext);
        }

        #endregion

        #region Properties

        public bool HasCanExecute => Mediator.HasCanExecute();

        public IExecutorRelayCommandMediator Mediator { get; }

        WeakReference IHasWeakReference.WeakReference
        {
            get
            {
                if (_ref == null)
                    MugenExtensions.LazyInitialize(ref _ref, MugenExtensions.GetWeakReference(this, true));
                return _ref!;
            }
        }

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
    }
}