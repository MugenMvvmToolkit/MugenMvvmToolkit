using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class DisableMultipleExecutionCommandComponent : DecoratorComponentBase<ICompositeCommand, IExecutorCommandComponent>, IExecutorCommandComponent, IConditionCommandComponent, IHasPriority
    {
        #region Fields

        private int _state;

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public bool HasCanExecute()
        {
            return true;
        }

        public bool CanExecute(object? parameter)
        {
            return _state == 0;
        }

        public Task ExecuteAsync(object? parameter)
        {
            if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) != 0)
                return Default.CompletedTask;

            try
            {
                var executionTask = Components.ExecuteAsync(parameter);
                if (executionTask.IsCompleted)
                {
                    _state = 0;
                    return executionTask;
                }

                Owner.RaiseCanExecuteChanged();
                executionTask.ContinueWith((t, o) =>
                {
                    var component = (DisableMultipleExecutionCommandComponent) o;
                    component._state = 0;
                    component.Owner.RaiseCanExecuteChanged();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
                return executionTask;
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