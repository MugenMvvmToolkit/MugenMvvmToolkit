using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingExpressionInitializerComponent : IBindingExpressionInitializerComponent, IHasPriority
    {
        public Action<IBindingManager, IBindingExpressionInitializerContext>? Initialize { get; set; }

        public int Priority { get; set; }

        void IBindingExpressionInitializerComponent.Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context) =>
            Initialize?.Invoke(bindingManager, context);
    }
}