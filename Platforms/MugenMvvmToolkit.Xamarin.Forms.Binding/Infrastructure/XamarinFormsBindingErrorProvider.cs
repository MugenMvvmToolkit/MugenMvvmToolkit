#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsBindingErrorProvider.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure
{
    public class XamarinFormsBindingErrorProvider : BindingErrorProviderBase
    {
        #region Methods

        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
        }

        #endregion
    }
}