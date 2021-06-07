using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingEventHandlerComponent : IBindingEventHandlerComponent, IHasPriority
    {
        public Action<IBindingManager, object?, object?, IReadOnlyMetadataContext?>? OnBeginEvent { get; set; }

        public Action<IBindingManager, object?, object?, IReadOnlyMetadataContext?>? OnEndEvent { get; set; }

        public Action<IBindingManager, Exception, object?, object?, IReadOnlyMetadataContext?>? OnEventError { get; set; }

        public int Priority { get; set; }

        void IBindingEventHandlerComponent.OnBeginEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata) =>
            OnBeginEvent?.Invoke(bindingManager, sender, message, metadata);

        void IBindingEventHandlerComponent.OnEndEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata) =>
            OnEndEvent?.Invoke(bindingManager, sender, message, metadata);

        void IBindingEventHandlerComponent.OnEventError(IBindingManager bindingManager, Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata) =>
            OnEventError?.Invoke(bindingManager, exception, sender, message, metadata);
    }
}