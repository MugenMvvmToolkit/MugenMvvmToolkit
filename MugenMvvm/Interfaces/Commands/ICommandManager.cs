using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICommandManager : IComponentOwner<ICommandManager>
    {
        ICompositeCommand? TryGetCommand<TParameter>(object request, IReadOnlyMetadataContext? metadata = null);
    }
}