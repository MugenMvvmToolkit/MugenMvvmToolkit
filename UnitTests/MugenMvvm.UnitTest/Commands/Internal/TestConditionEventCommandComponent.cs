using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestConditionEventCommandComponent : IConditionEventCommandComponent
    {
        #region Properties

        public Action<ICompositeCommand, EventHandler>? AddCanExecuteChanged { get; set; }

        public Action<ICompositeCommand, EventHandler>? RemoveCanExecuteChanged { get; set; }

        public Action<ICompositeCommand>? RaiseCanExecuteChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IConditionEventCommandComponent.AddCanExecuteChanged(ICompositeCommand command, EventHandler handler)
        {
            AddCanExecuteChanged?.Invoke(command, handler);
        }

        void IConditionEventCommandComponent.RemoveCanExecuteChanged(ICompositeCommand command, EventHandler handler)
        {
            RemoveCanExecuteChanged?.Invoke(command, handler);
        }

        void IConditionEventCommandComponent.RaiseCanExecuteChanged(ICompositeCommand command)
        {
            RaiseCanExecuteChanged?.Invoke(command);
        }

        #endregion
    }
}