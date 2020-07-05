using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingExpressionInitializerComponent : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBindingManager, IBindingExpressionInitializerContext>? Initialize { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingExpressionInitializerComponent.Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            Initialize?.Invoke(bindingManager, context);
        }

        #endregion
    }
}