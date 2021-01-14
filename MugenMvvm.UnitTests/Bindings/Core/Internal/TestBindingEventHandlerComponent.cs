using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingEventHandlerComponent : IBindingEventHandlerComponent, IHasPriority
    {
        private readonly IBindingManager? _bindingManager;

        public TestBindingEventHandlerComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnBeginEvent { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnEndEvent { get; set; }

        public Action<Exception, object?, object?, IReadOnlyMetadataContext?>? OnEventError { get; set; }

        public int Priority { get; set; }

        void IBindingEventHandlerComponent.OnBeginEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            OnBeginEvent?.Invoke(sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEndEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            OnEndEvent?.Invoke(sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEventError(IBindingManager bindingManager, Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            OnEventError?.Invoke(exception, sender, message, metadata);
        }
    }
}