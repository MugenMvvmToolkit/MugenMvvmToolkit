﻿using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommandExecutorComponent : ICommandExecutorComponent
    {
        #region Properties

        public Func<ICompositeCommand, object?, Task>? ExecuteAsync { get; set; }

        #endregion

        #region Implementation of interfaces

        Task ICommandExecutorComponent.ExecuteAsync(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata) => ExecuteAsync?.Invoke(command, parameter) ?? Default.CompletedTask;

        #endregion
    }
}