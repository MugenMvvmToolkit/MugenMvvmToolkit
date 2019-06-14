using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class ObserverChildBindingObserverProvider : IBindingPathChildBindingObserverProvider //todo static members?
    {
        #region Properties

        public int Priority { get; set; } = 5;

        #endregion

        #region Implementation of interfaces

        public bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            observer = default;
            return false;
        }

        public IBindingPath TryGetBindingPath(object path, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        public IBindingPathObserver TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}