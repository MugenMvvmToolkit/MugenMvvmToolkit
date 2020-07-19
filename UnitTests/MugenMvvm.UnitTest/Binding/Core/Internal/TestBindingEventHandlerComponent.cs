using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingEventHandlerComponent : IBindingEventHandlerComponent, IHasPriority
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;

        #endregion

        #region Constructors

        public TestBindingEventHandlerComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnBeginEvent { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? OnEndEvent { get; set; }

        public Action<Exception, object?, object?, IReadOnlyMetadataContext?>? OnEventError { get; set; }

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}