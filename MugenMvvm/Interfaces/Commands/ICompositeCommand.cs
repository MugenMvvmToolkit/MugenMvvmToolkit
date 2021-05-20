using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICompositeCommand : IComponentOwner<ICompositeCommand>, IMetadataOwner<IMetadataContext>, ICommand, IDisposable, ISuspendable
    {
        bool HasCanExecute(IReadOnlyMetadataContext? metadata = null);

        bool CanExecute(object? parameter, IReadOnlyMetadataContext? metadata);

        void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null);

        ValueTask<bool> ExecuteAsync(object? parameter, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}