using System;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    public class BindableRelayCommandMediator : RelayCommandMediatorWrapperBase, IBindableRelayCommandMediator, IThreadDispatcherHandler
    {
        #region Fields

        private readonly ThreadExecutionMode _eventExecutionMode;
        private readonly Func<string>? _getDisplayName;

        private readonly IThreadDispatcher _threadDispatcher;
        private bool _isNotificationsDirty;
        private object? _lastParameter;
        private int _suspendCount;

        #endregion

        #region Constructors

        public BindableRelayCommandMediator(IRelayCommandMediator mediator, IThreadDispatcher threadDispatcher, ThreadExecutionMode eventExecutionMode, Func<string>? getDisplayName)
            : base(mediator)
        {
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            _getDisplayName = getDisplayName;
        }

        #endregion

        #region Properties

        public string DisplayName => _getDisplayName?.Invoke() ?? string.Empty;

        public bool IsCanExecuteNullParameter => CanExecute(null);

        public bool IsCanExecuteLastParameter => CanExecute(_lastParameter);

        public override bool IsNotificationsSuspended => _suspendCount != 0 || base.IsNotificationsSuspended;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public void InvalidateProperties()
        {
            OnPropertyChanged(Default.EmptyPropertyChangedArgs);
        }

        public override IDisposable SuspendNotifications()
        {
            Interlocked.Increment(ref _suspendCount);
            var baseToken = base.SuspendNotifications();
            if (ReferenceEquals(Default.Disposable, baseToken))
                return WeakActionToken.Create(this, @this => @this.EndSuspendNotifications());
            return WeakActionToken.Create(this, baseToken, (@this, t) =>
            {
                t.Dispose();
                @this.EndSuspendNotifications();
            });
        }

        void IThreadDispatcherHandler.Execute(object? state)
        {
            PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)state);
        }

        #endregion

        #region Methods

        public override bool CanExecute(object parameter)
        {
            _lastParameter = parameter;
            return base.CanExecute(parameter);
        }

        public override void RaiseCanExecuteChanged()
        {
            OnPropertyChanged(Default.IsCanExecuteLastChangedArgs);
            OnPropertyChanged(Default.IsCanExecuteNullChangedArgs);
            base.RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            _lastParameter = null;
            base.Dispose();
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged == null)
                return;
            if (_suspendCount != 0)
            {
                _isNotificationsDirty = true;
                return;
            }

            _threadDispatcher.Execute(this, _eventExecutionMode, null);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            if (_isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        #endregion
    }
}