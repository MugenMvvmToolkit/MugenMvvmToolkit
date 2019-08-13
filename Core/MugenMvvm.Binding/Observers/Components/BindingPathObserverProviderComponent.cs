using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class BindingPathObserverProviderComponent : IBindingPathObserverProviderComponent, IHasPriority //todo static members?
    {
        #region Properties

        public int Priority { get; set; } = 5;

        #endregion

        #region Implementation of interfaces

        public IBindingPathObserver? TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}