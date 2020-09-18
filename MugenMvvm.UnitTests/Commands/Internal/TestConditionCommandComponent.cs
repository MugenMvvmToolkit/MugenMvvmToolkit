using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestConditionCommandComponent : IConditionCommandComponent
    {
        #region Properties

        public Func<ICompositeCommand, bool>? HasCanExecute { get; set; }

        public Func<ICompositeCommand, object?, bool>? CanExecute { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCommandComponent.HasCanExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => HasCanExecute?.Invoke(command) ?? false;

        bool IConditionCommandComponent.CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) => CanExecute?.Invoke(command, parameter) ?? true;

        #endregion
    }
}