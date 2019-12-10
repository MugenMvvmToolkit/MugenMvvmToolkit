using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Commands
{
    public sealed class CommandProvider : ComponentOwnerBase<ICommandProvider>, ICommandProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CommandProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ICompositeCommand GetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetComponents<ICommandProviderComponent>(metadata).TryGetCommand(request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            GetComponents<ICommandProviderListener>(metadata).OnCommandCreated(this, request, result, metadata);
            return result;
        }

        #endregion
    }
}