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

        protected RelayCommandBase(Action execute, Func<bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? context)
            : this(execute, canExecute, notifiers, contextBase: context)
        {
        }

        protected RelayCommandBase(Action<T> execute, Func<T, bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? context)
            : this(execute, canExecute, notifiers, contextBase: context)
        {
        }

        protected RelayCommandBase(Func<Task> execute, Func<bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? context)
            : this(execute, canExecute, notifiers, contextBase: context)
        {
        }

        protected RelayCommandBase(Func<T, Task> execute, Func<T, bool>? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? context)
            : this(execute, canExecute, notifiers, contextBase: context)
        {
        }

        protected RelayCommandBase(Delegate execute, Delegate? canExecute, IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? contextBase)
        {
            Mediator = Singleton<IRelayCommandDispatcher>.Instance.GetMediator(execute, canExecute, notifiers, contextBase ?? Default.MetadataContext);
        }

        #endregion

        #region Properties

        public IRelayCommandMediator Mediator { get; }

        WeakReference IHasWeakReference.WeakReference
        {
            get
            {
                if (_ref == null)
                    MugenExtensions.LazyInitialize(ref _ref, MugenExtensions.GetWeakReference(this, true));
                return _ref!;
            }
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => Mediator.AddCanExecuteChanged(value);
            remove => Mediator.RemoveCanExecuteChanged(value);
        }

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}