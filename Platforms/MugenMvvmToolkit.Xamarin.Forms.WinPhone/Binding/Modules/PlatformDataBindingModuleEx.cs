#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleEx.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Modules;

#if XAMARIN_FORMS && ANDROID
using MugenMvvmToolkit.Xamarin.Forms.Android.Binding.Infrastructure;
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Binding.Modules
#elif TOUCH
using MugenMvvmToolkit.Xamarin.Forms.iOS.Binding.Infrastructure;
namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Binding.Modules
#elif WINDOWS_PHONE
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Binding.Infrastructure;
namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Binding.Modules
#elif WINDOWSCOMMON
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding.Infrastructure;
namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding.Modules
#endif
{
    public class PlatformDataBindingModuleEx : PlatformDataBindingModule
    {
        #region Overrides of DataBindingModule

        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}
