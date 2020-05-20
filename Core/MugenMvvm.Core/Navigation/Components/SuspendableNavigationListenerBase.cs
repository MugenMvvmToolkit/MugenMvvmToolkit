using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public abstract class SuspendableNavigationListenerBase : AttachableComponentBase<INavigationDispatcher>, INavigationDispatcherNavigatedListener, INavigationDispatcherErrorListener, ISuspendable
    {
        #region Fields

        private readonly List<(INavigationDispatcher, INavigationContext, Exception?, CancellationToken?)> _suspendedEvents;
        private int _suspendCount;

        #endregion

        #region Constructors

        protected SuspendableNavigationListenerBase()
        {
            _suspendedEvents = new List<(INavigationDispatcher, INavigationContext, Exception?, CancellationToken?)>();
        }

        #endregion

        #region Properties

        public bool IsSuspended
        {
            get
            {
                lock (_suspendedEvents)
                {
                    return _suspendCount != 0;
                }
            }
        }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            lock (_suspendedEvents)
            {
                if (_suspendCount != 0)
                {
                    _suspendedEvents.Add((navigationDispatcher, navigationContext, exception, null));
                    return;
                }
            }

            OnNavigationFailed(navigationDispatcher, navigationContext, exception);
        }

        void INavigationDispatcherErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            lock (_suspendedEvents)
            {
                if (_suspendCount != 0)
                {
                    _suspendedEvents.Add((navigationDispatcher, navigationContext, null, cancellationToken));
                    return;
                }
            }

            OnNavigationCanceled(navigationDispatcher, navigationContext, cancellationToken);
        }

        void INavigationDispatcherNavigatedListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            lock (_suspendedEvents)
            {
                if (_suspendCount != 0)
                {
                    _suspendedEvents.Add((navigationDispatcher, navigationContext, null, null));
                    return;
                }
            }

            OnNavigated(navigationDispatcher, navigationContext);
        }

        public ActionToken Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            bool begin;
            lock (_suspendedEvents)
            {
                begin = ++_suspendCount == 1;
            }

            if (begin)
                OnBeginSuspend();
            return new ActionToken((o, _) => ((SuspendableNavigationListenerBase) o!).EndSuspend(), this);
        }

        #endregion

        #region Methods

        protected abstract void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception);

        protected abstract void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        protected virtual void OnBeginSuspend()
        {
        }

        protected virtual void OnEndSuspend()
        {
        }

        private void EndSuspend()
        {
            (INavigationDispatcher dispatcher, INavigationContext context, Exception? exception, CancellationToken? cancellationToken)[] events;
            lock (_suspendedEvents)
            {
                if (--_suspendCount != 0)
                    return;
                events = _suspendedEvents.ToArray();
                _suspendedEvents.Clear();
            }

            OnEndSuspend();

            for (var i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                if (eventInfo.exception != null)
                    OnNavigationFailed(eventInfo.dispatcher, eventInfo.context, eventInfo.exception);
                else if (eventInfo.cancellationToken != null)
                    OnNavigationCanceled(eventInfo.dispatcher, eventInfo.context, eventInfo.cancellationToken.Value);
                else
                    OnNavigated(eventInfo.dispatcher, eventInfo.context);
            }
        }

        #endregion
    }
}