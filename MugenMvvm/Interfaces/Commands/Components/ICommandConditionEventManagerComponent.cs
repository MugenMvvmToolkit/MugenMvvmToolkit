using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandConditionEventManagerComponent : IComponent<ICompositeCommand>
    {
        void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata);

        void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata);

        void RaiseCanExecuteChanged(ICompositeCommand command, IReadOnlyMetadataContext? metadata);
    }
}