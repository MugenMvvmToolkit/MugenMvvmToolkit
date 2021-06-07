using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandConditionComponent : ICommandConditionComponent
    {
        public Func<ICompositeCommand, object?, bool>? CanExecute { get; set; }

        bool ICommandConditionComponent.CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            CanExecute?.Invoke(command, parameter) ?? true;
    }
}