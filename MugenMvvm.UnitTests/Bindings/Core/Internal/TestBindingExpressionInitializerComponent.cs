using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingExpressionInitializerComponent : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;

        #endregion

        #region Constructors

        public TestBindingExpressionInitializerComponent(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<IBindingExpressionInitializerContext>? Initialize { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingExpressionInitializerComponent.Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            Initialize?.Invoke(context);
        }

        #endregion
    }
}