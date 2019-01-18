using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    internal sealed class DisableMultipleExecutionRelayCommandMediator : RelayCommandMediatorWrapperBase
    {
        #region Fields

        private int _state;

        #endregion

        #region Constructors

        public DisableMultipleExecutionRelayCommandMediator(IRelayCommandMediator mediator)
            : base(mediator)
        {
        }

        #endregion

        #region Methods

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _state == 0;
        }

        public override Task ExecuteAsync(object parameter)
        {
            if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) != 0)
                return Default.CompletedTask;
            try
            {
                var task = base.ExecuteAsync(parameter);
                if (task.IsCompleted)
                {
                    Interlocked.Exchange(ref _state, 0);
                    return task;
                }

                RaiseCanExecuteChanged();
                return task.ContinueWith((t, o) =>
                {
                    var wrapper = (DisableMultipleExecutionRelayCommandMediator) o;
                    Interlocked.Exchange(ref wrapper._state, 0);
                    wrapper.RaiseCanExecuteChanged();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
            }
            catch
            {
                _state = 0;
                throw;
            }
        }

        #endregion
    }
}