﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICompositeCommand : IComponentOwner<ICompositeCommand>, IMetadataOwner<IMetadataContext>, ICommand, IHasDisposeState, ISuspendable //todo generic?
    {
        bool IsExecuting(IReadOnlyMetadataContext? metadata = null);

        bool CanExecute(object? parameter, IReadOnlyMetadataContext? metadata);

        void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null);

        void Execute(object? parameter = null, IReadOnlyMetadataContext? metadata = null);

        Task<bool?> ExecuteAsync(object? parameter = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        Task WaitAsync(IReadOnlyMetadataContext? metadata = null);
    }
}