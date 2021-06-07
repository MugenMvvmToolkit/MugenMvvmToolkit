using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingHolderComponent : IBindingHolderComponent, IHasPriority
    {
        public Func<IBindingManager, object, string?, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBinding>>? TryGetBindings { get; set; }

        public Func<IBindingManager, object?, IBinding, IReadOnlyMetadataContext?, bool>? TryRegister { get; set; }

        public Func<IBindingManager, object?, IBinding, IReadOnlyMetadataContext?, bool>? TryUnregister { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IBinding> IBindingHolderComponent.TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata) =>
            TryGetBindings?.Invoke(bindingManager, target, path, metadata) ?? default;

        bool IBindingHolderComponent.TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata) =>
            TryRegister?.Invoke(bindingManager, target, binding, metadata) ?? default;

        bool IBindingHolderComponent.TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata) =>
            TryUnregister?.Invoke(bindingManager, target, binding, metadata) ?? default;
    }
}