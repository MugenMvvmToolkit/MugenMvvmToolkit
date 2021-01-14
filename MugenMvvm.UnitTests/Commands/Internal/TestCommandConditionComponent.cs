using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommandConditionComponent : ICommandConditionComponent
    {
        public Func<ICompositeCommand, bool>? HasCanExecute { get; set; }

        public Func<ICompositeCommand, object?, bool>? CanExecute { get; set; }

        bool ICommandConditionComponent.HasCanExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => HasCanExecute?.Invoke(command) ?? false;

        bool ICommandConditionComponent.CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) =>
            CanExecute?.Invoke(command, parameter) ?? true;
    }
}