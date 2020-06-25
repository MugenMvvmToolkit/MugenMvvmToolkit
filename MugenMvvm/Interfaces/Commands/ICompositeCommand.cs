using System;
using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICompositeCommand : IComponentOwner<ICompositeCommand>, IMetadataOwner<IMetadataContext>, ICommand, IDisposable, ISuspendable
    {
        bool HasCanExecute { get; }

        void RaiseCanExecuteChanged();
    }
}