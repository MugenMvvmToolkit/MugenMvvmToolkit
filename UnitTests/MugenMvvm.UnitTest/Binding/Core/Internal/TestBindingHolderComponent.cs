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

        public Func<object, string?, IReadOnlyMetadataContext?, ItemOrList<IBinding, IReadOnlyList<IBinding>>>? TryGetBindings { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryRegister { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryUnregister { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBinding, IReadOnlyList<IBinding>> IBindingHolderComponent.TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            return TryGetBindings?.Invoke(target, path, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryRegister(object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            return TryRegister?.Invoke(target, binding, metadata) ?? default;
        }

        bool IBindingHolderComponent.TryUnregister(object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            return TryUnregister?.Invoke(target, binding, metadata) ?? default;
        }

        #endregion
    }
}