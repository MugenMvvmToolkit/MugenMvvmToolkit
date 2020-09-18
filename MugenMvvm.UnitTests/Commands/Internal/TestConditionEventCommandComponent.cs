using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestConditionEventCommandComponent : IConditionEventCommandComponent
    {
        #region Properties

        public Action<ICompositeCommand, EventHandler>? AddCanExecuteChanged { get; set; }

        public Action<ICompositeCommand, EventHandler>? RemoveCanExecuteChanged { get; set; }

        public Action<ICompositeCommand>? RaiseCanExecuteChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IConditionEventCommandComponent.AddCanExecuteChanged(ICompositeCommand command, EventHandler handler, IReadOnlyMetadataContext? metadata) => AddCanExecuteChanged?.Invoke(command, handler);

        void IConditionEventCommandComponent.RemoveCanExecuteChanged(ICompositeCommand command, EventHandler handler, IReadOnlyMetadataContext? metadata) => RemoveCanExecuteChanged?.Invoke(command, handler);

        void IConditionEventCommandComponent.RaiseCanExecuteChanged(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => RaiseCanExecuteChanged?.Invoke(command);

        #endregion
    }
}