using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingEventHandlerComponent : IComponent<IBindingManager>
    {
        void OnBeginEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata);

        void OnEndEvent(IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata);

        void OnEventError(IBindingManager bindingManager, Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata);
    }
}