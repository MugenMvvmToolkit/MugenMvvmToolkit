using System;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.UnitTest.Commands
{
    public class TestConditionEventCommandComponent : IConditionEventCommandComponent
    {
        #region Properties

        public Action<EventHandler>? AddCanExecuteChanged { get; set; }

        public Action<EventHandler>? RemoveCanExecuteChanged { get; set; }

        public Action? RaiseCanExecuteChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IConditionEventCommandComponent.AddCanExecuteChanged(EventHandler handler)
        {
            AddCanExecuteChanged?.Invoke(handler);
        }

        void IConditionEventCommandComponent.RemoveCanExecuteChanged(EventHandler handler)
        {
            RemoveCanExecuteChanged?.Invoke(handler);
        }

        void IConditionEventCommandComponent.RaiseCanExecuteChanged()
        {
            RaiseCanExecuteChanged?.Invoke();
        }

        #endregion
    }
}