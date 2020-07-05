using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingHolderComponent : IBindingHolderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IBindingManager, object, string?, IReadOnlyMetadataContext?, ItemOrList<IBinding, IReadOnlyList<IBinding>>>? TryGetBindings { get; set; }

        public Func<IBindingManager, object?, IBinding, IReadOnlyMetadataContext?, bool>? TryRegister { get; set; }

        public Func<IBindingManager, object?, IBinding, IReadOnlyMetadataContext?, bool>? TryUnregister { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBinding, IReadOnlyList<IBinding>> IBindingHolderComponent.TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            return TryGetBindings?.Invoke(bindingManager, target, path, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            return TryRegister?.Invoke(bindingManager, target, binding, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            return TryUnregister?.Invoke(bindingManager, target, binding, metadata) ?? default;
        }

        #endregion
    }
}