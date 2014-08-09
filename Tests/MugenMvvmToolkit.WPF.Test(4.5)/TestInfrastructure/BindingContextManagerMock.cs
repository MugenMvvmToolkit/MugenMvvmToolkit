using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingContextManagerMock : IBindingContextManager
    {
        #region Properties

        public Func<object, IBindingContext> GetBindingContext { get; set; }

        #endregion

        #region Implementation of IBindingContextManager

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        IBindingContext IBindingContextManager.GetBindingContext(object item)
        {
            return GetBindingContext(item);
        }

        #endregion
    }
}