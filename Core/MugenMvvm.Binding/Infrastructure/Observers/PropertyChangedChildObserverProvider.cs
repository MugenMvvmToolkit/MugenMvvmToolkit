using System;
using System.ComponentModel;
using MugenMvvm.Binding.Interfaces.Events;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public class PropertyChangedChildObserverProvider : IChildObserverProvider, IBindingMemberObserver
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IDisposable TryObserve(object source, object member, IBindingEventListener listener, IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        public IBindingMemberObserver? TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFromUnified(type) && member is string)
                return this;
            return null;
        }

        #endregion
    }
}