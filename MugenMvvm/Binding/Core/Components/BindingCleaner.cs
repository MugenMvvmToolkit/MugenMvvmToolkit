using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingCleaner : IBindingLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState != BindingLifecycleState.Disposed)
                return;
            DefaultComponentExtensions.Dispose(binding.GetComponents().GetRawValue());
            binding.Target.Dispose();
            MugenBindingExtensions.DisposeBindingSource(binding.Source.GetRawValue());
            binding.Components.Clear();
        }

        #endregion
    }
}