using System;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingEventHandlerComponent : IBindingEventHandlerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnBeginEvent { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnEndEvent { get; set; }

        public Action<Exception, object?, object?, IReadOnlyMetadataContext?>? OnEventError { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingEventHandlerComponent.OnBeginEvent(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnBeginEvent?.Invoke(sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEndEvent(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnEndEvent?.Invoke(sender, message, metadata);
        }

        void IBindingEventHandlerComponent.OnEventError(Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnEventError?.Invoke(exception, sender, message, metadata);
        }

        #endregion
    }
}