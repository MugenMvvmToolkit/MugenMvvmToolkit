using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandConditionComponent : ICommandConditionComponent, IHasPriority
    {
        public Func<ICompositeCommand, object?, IReadOnlyMetadataContext?, bool>? CanExecute { get; set; }

        public int Priority { get; set; }

        bool ICommandConditionComponent.CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            CanExecute?.Invoke(command, parameter, metadata) ?? true;
    }
}