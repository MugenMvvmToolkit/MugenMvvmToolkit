using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation.Components
{
    public abstract class SuspendableNavigationListenerBase : AttachableComponentBase<INavigationDispatcher>, INavigationListener, INavigationErrorListener, ISuspendable
    {
        #region Fields

        private readonly List<(INavigationDispatcher, INavigationContext, object?, CancellationToken?)> _suspendedEvents;
        private int _suspendCount;

        #endregion

        #region Constructors

        protected SuspendableNavigationListenerBase()
        {
            _suspendedEvents = new List<(INavigationDispatcher, INavigationContext, object?, CancellationToken?)>();
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

        void INavigationErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception) =>
            AddEvent((navigationDispatcher, navigationContext, exception, null));

        void INavigationErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken) =>
            AddEvent((navigationDispatcher, navigationContext, null, cancellationToken));

        void INavigationListener.OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) => AddEvent((navigationDispatcher, navigationContext, this, null));

        void INavigationListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) => AddEvent((navigationDispatcher, navigationContext, null, null));

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
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

        private void AddEvent(in (INavigationDispatcher, INavigationContext, object?, CancellationToken?) e)
        {
            lock (_suspendedEvents)
            {
                if (_suspendCount != 0)
                {
                    _suspendedEvents.Add(e);
                    return;
                }
            }

            InvokeEvent(e);
        }

        private void InvokeEvent(in (INavigationDispatcher dispatcher, INavigationContext context, object? exceptionOrNavigatingFlag, CancellationToken? cancellationToken) eventInfo)
        {
            if (eventInfo.exceptionOrNavigatingFlag is Exception e)
                OnNavigationFailed(eventInfo.dispatcher, eventInfo.context, e);
            else if (eventInfo.cancellationToken != null)
                OnNavigationCanceled(eventInfo.dispatcher, eventInfo.context, eventInfo.cancellationToken.Value);
            else if (eventInfo.exceptionOrNavigatingFlag != null)
                OnNavigating(eventInfo.dispatcher, eventInfo.context);
            else
                OnNavigated(eventInfo.dispatcher, eventInfo.context);
        }

        protected abstract void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception);

        protected abstract void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        protected abstract void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        protected virtual void OnBeginSuspend()
        {
        }

        protected virtual void OnEndSuspend()
        {
        }

        private void EndSuspend()
        {
            ItemOrArray<(INavigationDispatcher dispatcher, INavigationContext context, object? exceptionOrNavigatingFlag, CancellationToken? cancellationToken)> events;
            lock (_suspendedEvents)
            {
                if (--_suspendCount != 0)
                    return;
                events = _suspendedEvents.Count == 1 ? _suspendedEvents[0] : _suspendedEvents.ToArray();
                _suspendedEvents.Clear();
            }

            OnEndSuspend();

            foreach (var t in events)
                InvokeEvent(t);
        }

        #endregion
    }
}