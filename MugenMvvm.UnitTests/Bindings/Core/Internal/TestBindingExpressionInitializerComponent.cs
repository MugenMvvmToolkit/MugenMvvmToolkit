using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingExpressionInitializerComponent : IBindingExpressionInitializerComponent, IHasPriority
    {
        private readonly IBindingManager? _bindingManager;

        public TestBindingExpressionInitializerComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        public Action<IBindingExpressionInitializerContext>? Initialize { get; set; }

        public int Priority { get; set; }

        void IBindingExpressionInitializerComponent.Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            Initialize?.Invoke(context);
        }
    }
}