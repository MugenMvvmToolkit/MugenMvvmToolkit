using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class ExecutionModeCommandComponent : DecoratorComponentBase<ICompositeCommand, IExecutorCommandComponent>, IExecutorCommandComponent, IHasPriority
    {
        #region Fields

        private readonly CommandExecutionMode _executionMode;

        #endregion

        #region Constructors

        private ExecutionModeCommandComponent(CommandExecutionMode executionMode)
        {
            _executionMode = executionMode;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializerHigh;

        #endregion

        #region Implementation of interfaces

        public Task ExecuteAsync(object? parameter)
        {
            if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecute)
            {
                if (!Owner.CanExecute(parameter))
                {
                    Owner.RaiseCanExecuteChanged();
                    return Default.CompletedTask;
                }
            }
            else if (_executionMode == CommandExecutionMode.CanExecuteBeforeExecuteException)
            {
                if (!Owner.CanExecute(parameter))
                    ExceptionManager.ThrowCommandCannotBeExecuted();
            }

            return Components.ExecuteAsync(parameter);
        }

        #endregion

        #region Methods

        public static ExecutionModeCommandComponent? Get(CommandExecutionMode executionMode)
        {
            switch (executionMode)
            {
                case CommandExecutionMode.CanExecuteBeforeExecute:
                case CommandExecutionMode.CanExecuteBeforeExecuteException:
                    return new ExecutionModeCommandComponent(executionMode);
                default:
                    return null;
            }
        }

        #endregion
    }
}