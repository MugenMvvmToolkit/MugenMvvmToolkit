using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IChildObserverProvider : IHasPriority
    {
        IBindingMemberObserver? TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata);
    }
}