using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingEventHandlerComponent : IBindingEventHandlerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBindingManager, object?, object?, IReadOnlyMetadataContext?>? OnBeginEvent { get; set; }

        public Action<IBindingManager, object?, object?, IReadOnlyMetadataContext?>? OnEndEvent { get; set; }

        public Action<IBindingManager, Exception, object?, object?, IReadOnlyMetadataContext?>? OnEventError { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingEventHandlerComponent.OnBeginEvent<T>(IBindingManager bindingManager, object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            OnBeginEvent?.Invoke(bindingManager, sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEndEvent<T>(IBindingManager bindingManager, object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            OnEndEvent?.Invoke(bindingManager, sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEventError<T>(IBindingManager bindingManager, Exception exception, object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            OnEventError?.Invoke(bindingManager, exception, sender, message, metadata);
        }

        #endregion
    }
}