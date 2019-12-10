using System;
using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICompositeCommand : IComponentOwner<ICompositeCommand>, ICommand, IDisposable, ISuspendable
    {
        bool HasCanExecute { get; }

        void RaiseCanExecuteChanged();
    }
}