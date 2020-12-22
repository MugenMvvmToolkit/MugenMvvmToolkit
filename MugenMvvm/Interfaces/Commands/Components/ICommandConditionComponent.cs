using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandConditionComponent : IComponent<ICompositeCommand>
    {
        bool HasCanExecute(ICompositeCommand command, IReadOnlyMetadataContext? metadata);

        bool CanExecute(ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata);
    }
}