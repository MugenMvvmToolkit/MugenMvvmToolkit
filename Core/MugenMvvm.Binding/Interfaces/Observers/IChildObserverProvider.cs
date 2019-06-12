using System;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IChildObserverProvider : IHasPriority
    {
        bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer);
    }
}