#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingModuleEx.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding.Modules
{
    public class PlatformDataBindingModuleEx : PlatformDataBindingModule
    {
        #region Overrides of DataBindingModule

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}