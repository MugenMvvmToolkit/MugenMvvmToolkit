using System;
using System.Windows.Input;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommand : ICommand
    {
        event EventHandler? ICommand.CanExecuteChanged
        {
            add => AddCanExecuteChanged?.Invoke(value!);
            remove => RemoveCanExecuteChanged?.Invoke(value!);
        }

        public Func<object?, bool>? CanExecute { get; set; }

        public Action<object?>? Execute { get; set; }

        public Action<EventHandler>? AddCanExecuteChanged { get; set; }

        public Action<EventHandler>? RemoveCanExecuteChanged { get; set; }

        bool ICommand.CanExecute(object? parameter) => CanExecute?.Invoke(parameter) ?? true;

        void ICommand.Execute(object? parameter) => Execute?.Invoke(parameter);
    }
}