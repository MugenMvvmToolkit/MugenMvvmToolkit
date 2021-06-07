using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandEventHandlerComponent : ICommandEventHandlerComponent
    {
        public Action<ICompositeCommand, EventHandler>? AddCanExecuteChanged { get; set; }

        public Action<ICompositeCommand, EventHandler>? RemoveCanExecuteChanged { get; set; }

        public Action<ICompositeCommand>? RaiseCanExecuteChanged { get; set; }

        void ICommandEventHandlerComponent.AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) =>
            AddCanExecuteChanged?.Invoke(command, handler!);

        void ICommandEventHandlerComponent.RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) =>
            RemoveCanExecuteChanged?.Invoke(command, handler!);

        void ICommandEventHandlerComponent.RaiseCanExecuteChanged(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => RaiseCanExecuteChanged?.Invoke(command);
    }
}