using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingErrorProviderMock : IBindingErrorProvider
    {
        #region Properties

        public Action<object, string, IList<object>, IDataContext> SetErrors { get; set; }

        #endregion

        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="senderKey">The source of the errors.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        void IBindingErrorProvider.SetErrors(object target, string senderKey, IList<object> errors, IDataContext context)
        {
            if (SetErrors != null)
                SetErrors(target, senderKey, errors, context);
        }

        #endregion
    }
}