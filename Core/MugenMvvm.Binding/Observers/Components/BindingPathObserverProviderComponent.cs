using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class BindingPathObserverProviderComponent : IBindingPathObserverProviderComponent //todo static members?
    {
        #region Properties

        public int Priority { get; set; } = 5;

        #endregion

        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        public IBindingPathObserver? TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}