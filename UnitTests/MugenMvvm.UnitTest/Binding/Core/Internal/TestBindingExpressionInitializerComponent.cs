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

        public Action<IBindingExpressionInitializerContext>? Initialize { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingExpressionInitializerComponent.Initialize(IBindingExpressionInitializerContext context)
        {
            Initialize?.Invoke(context);
        }

        #endregion
    }
}