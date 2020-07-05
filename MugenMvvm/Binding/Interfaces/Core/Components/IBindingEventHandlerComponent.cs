using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingEventHandlerComponent : IComponent<IBindingManager>
    {
        void OnBeginEvent<T>(IBindingManager bindingManager, object? sender, in T message, IReadOnlyMetadataContext? metadata);

        void OnEndEvent<T>(IBindingManager bindingManager, object? sender, in T message, IReadOnlyMetadataContext? metadata);

        void OnEventError<T>(IBindingManager bindingManager, Exception exception, object? sender, in T message, IReadOnlyMetadataContext? metadata);
    }
}