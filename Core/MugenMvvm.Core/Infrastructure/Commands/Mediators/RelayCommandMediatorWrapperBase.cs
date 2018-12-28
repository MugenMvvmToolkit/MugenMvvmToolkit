using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    public abstract class RelayCommandMediatorWrapperBase : IRelayCommandMediator
    {
        #region Fields

        protected readonly IRelayCommandMediator Mediator;

        #endregion

        #region Constructors

        protected RelayCommandMediatorWrapperBase(IRelayCommandMediator mediator)
        {
            Mediator = mediator;
        }

        #endregion

        #region Properties

        public virtual bool IsNotificationsSuspended => Mediator.IsNotificationsSuspended;

        public virtual bool HasCanExecute => Mediator.HasCanExecute;

        #endregion

        #region Implementation of interfaces

        public virtual IDisposable SuspendNotifications()
        {
            return Mediator.SuspendNotifications();
        }

        public virtual void Dispose()
        {
            Mediator.Dispose();
        }

        public virtual TMediator GetMediator<TMediator>() where TMediator : class?
        {
            if (this is TMediator result)
                return result;
            return Mediator.GetMediator<TMediator>();
        }

        public virtual void AddCanExecuteChanged(EventHandler handler)
        {
            Mediator.AddCanExecuteChanged(handler);
        }

        public virtual void RemoveCanExecuteChanged(EventHandler handler)
        {
            Mediator.RemoveCanExecuteChanged(handler);
        }

        public virtual bool CanExecute(object parameter)
        {
            return Mediator.CanExecute(parameter);
        }

        public virtual Task ExecuteAsync(object parameter)
        {
            return Mediator.ExecuteAsync(parameter);
        }

        public virtual void RaiseCanExecuteChanged()
        {
            Mediator.RaiseCanExecuteChanged();
        }

        #endregion
    }
}