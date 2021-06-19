﻿using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingCleaner : IBindingLifecycleListener, IHasPriority
    {
        public int Priority { get; set; } = BindingComponentPriority.LifecyclePostInitializer;

        public void OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState != BindingLifecycleState.Disposed)
                return;
            InternalComponentExtensions.Dispose(binding.GetComponents().GetRawValue());
            binding.Target.Dispose();
            BindingMugenExtensions.DisposeBindingSource(binding.Source.GetRawValue());
            binding.Components.Clear();
        }
    }
}