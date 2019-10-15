using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    internal interface IMemberObserverProviderComponentInternal<TMember>
    {
        MemberObserver TryGetMemberObserver(Type type, in TMember member, IReadOnlyMetadataContext? metadata);
    }
}