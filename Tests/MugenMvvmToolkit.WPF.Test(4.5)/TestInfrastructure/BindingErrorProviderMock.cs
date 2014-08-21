using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingErrorProviderMock : IBindingErrorProvider
    {
        #region Properties

        public Action<object, IList<object>> SetErrors { get; set; }

        #endregion

        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Sets errors for target.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="errors">The collection of errors</param>
        void IBindingErrorProvider.SetErrors(object target, IList<object> errors)
        {
            if (SetErrors != null)
                SetErrors(target, errors);
        }

        #endregion
    }
}