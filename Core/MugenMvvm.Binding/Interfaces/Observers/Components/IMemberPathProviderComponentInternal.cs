using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    internal interface IMemberPathProviderComponentInternal<TPath>
    {
        IMemberPath? TryGetMemberPath(in TPath path, IReadOnlyMetadataContext? metadata);
    }
}