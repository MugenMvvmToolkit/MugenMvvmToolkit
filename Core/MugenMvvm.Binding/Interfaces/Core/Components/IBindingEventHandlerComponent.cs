using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingEventHandlerComponent : IComponent<IBindingManager>
    {
        void OnBeginEvent(object? sender, object? message, IReadOnlyMetadataContext? metadata);

        void OnEndEvent(object? sender, object? message, IReadOnlyMetadataContext? metadata);

        void OnEventError(Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata);
    }
}