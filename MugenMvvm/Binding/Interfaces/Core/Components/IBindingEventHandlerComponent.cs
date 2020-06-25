using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingEventHandlerComponent : IComponent<IBindingManager>
    {
        void OnBeginEvent<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata);

        void OnEndEvent<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata);

        void OnEventError<T>(Exception exception, object? sender, in T message, IReadOnlyMetadataContext? metadata);
    }
}