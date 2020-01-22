using System;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.UnitTest.Commands
{
    public class TestConditionCommandComponent : IConditionCommandComponent
    {
        #region Properties

        public Func<bool>? HasCanExecute { get; set; }

        public Func<object?, bool>? CanExecute { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCommandComponent.HasCanExecute()
        {
            return HasCanExecute?.Invoke() ?? false;
        }

        bool IConditionCommandComponent.CanExecute(object? parameter)
        {
            return CanExecute?.Invoke(parameter) ?? true;
        }

        #endregion
    }
}