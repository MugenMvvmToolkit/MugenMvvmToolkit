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

        public bool HasBindingContext(object item)
        {
            throw new NotImplementedException();
        }

        IBindingContext IBindingContextManager.GetBindingContext(object item)
        {
            return GetBindingContext(item);
        }

        #endregion
    }
}
