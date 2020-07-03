using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestConditionCommandComponent : IConditionCommandComponent
    {
        #region Properties

        public Func<ICompositeCommand, bool>? HasCanExecute { get; set; }

        public Func<ICompositeCommand, object?, bool>? CanExecute { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCommandComponent.HasCanExecute(ICompositeCommand command)
        {
            return HasCanExecute?.Invoke(command) ?? false;
        }

        bool IConditionCommandComponent.CanExecute(ICompositeCommand command, object? parameter)
        {
            return CanExecute?.Invoke(command, parameter) ?? true;
        }

        #endregion
    }
}