using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingHolderComponent : IBindingHolderComponent, IHasPriority
    {
        private readonly IBindingManager? _bindingManager;

        public TestBindingHolderComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        public Func<object, string?, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IBinding>>? TryGetBindings { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryRegister { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryUnregister { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IBinding> IBindingHolderComponent.TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            return TryGetBindings?.Invoke(target, path, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            return TryRegister?.Invoke(target, binding, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            return TryUnregister?.Invoke(target, binding, metadata) ?? default;
        }
    }
}