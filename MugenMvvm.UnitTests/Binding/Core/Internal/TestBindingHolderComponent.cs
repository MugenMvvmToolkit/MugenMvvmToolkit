﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Binding.Core.Internal
{
    public class TestBindingHolderComponent : IBindingHolderComponent, IHasPriority
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;

        #endregion

        #region Constructors

        public TestBindingHolderComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, string?, IReadOnlyMetadataContext?, ItemOrList<IBinding, IReadOnlyList<IBinding>>>? TryGetBindings { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryRegister { get; set; }

        public Func<object?, IBinding, IReadOnlyMetadataContext?, bool>? TryUnregister { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IBinding, IReadOnlyList<IBinding>> IBindingHolderComponent.TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata)
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

        #endregion
    }
}