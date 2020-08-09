using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestExecutorCommandComponent : IExecutorCommandComponent
    {
        #region Properties

        public Func<ICompositeCommand, object?, Task>? ExecuteAsync { get; set; }

        #endregion

        #region Implementation of interfaces

        Task IExecutorCommandComponent.ExecuteAsync(ICompositeCommand command, object? parameter) => ExecuteAsync?.Invoke(command, parameter) ?? Default.CompletedTask;

        #endregion
    }
}