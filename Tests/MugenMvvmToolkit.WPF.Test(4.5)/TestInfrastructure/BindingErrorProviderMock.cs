#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProviderMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

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

        IList<object> IBindingErrorProvider.GetErrors(object target, string key, IDataContext context)
        {
            if (GetErrors == null)
                return Empty.Array<object>();
            return GetErrors(target, key, context);
        }

        void IBindingErrorProvider.SetErrors(object target, string senderKey, IList<object> errors, IDataContext context)
        {
            if (SetErrors != null)
                SetErrors(target, senderKey, errors, context);
        }

        #endregion
    }
}
