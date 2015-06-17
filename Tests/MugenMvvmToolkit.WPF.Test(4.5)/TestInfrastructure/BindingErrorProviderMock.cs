using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingErrorProviderMock : IBindingErrorProvider
    {
        #region Properties

        public Func<object, string, IDataContext, IList<object>> GetErrors { get; set; }

        public Action<object, string, IList<object>, IDataContext> SetErrors { get; set; }

        #endregion

        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="key">
        ///     The name of the key to retrieve validation errors for; or null or
        ///     <see cref="F:System.String.Empty" />, to retrieve entity-level errors.
        /// </param>
        /// <param name="context">The specified context, if any.</param>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        IList<object> IBindingErrorProvider.GetErrors(object target, string key, IDataContext context)
        {
            if (GetErrors == null)
                return Empty.Array<object>();
            return GetErrors(target, key, context);
        }

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