#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManagerMock.cs">
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
