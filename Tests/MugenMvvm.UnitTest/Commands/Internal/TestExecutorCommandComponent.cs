using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestExecutorCommandComponent : IExecutorCommandComponent
    {
        #region Properties

        public Func<object?, Task>? ExecuteAsync { get; set; }

        #endregion

        #region Implementation of interfaces

        Task IExecutorCommandComponent.ExecuteAsync(object? parameter)
        {
            return ExecuteAsync?.Invoke(parameter) ?? Default.CompletedTask;
        }

        #endregion
    }
}