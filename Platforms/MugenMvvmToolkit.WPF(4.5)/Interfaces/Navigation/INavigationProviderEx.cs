#region Copyright

// ****************************************************************************
// <copyright file="INavigationProviderEx.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;

#if WPF
namespace MugenMvvmToolkit.WPF.Interfaces.Navigation
#elif ANDROID
namespace MugenMvvmToolkit.Android.Interfaces.Navigation
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Interfaces.Navigation
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Interfaces.Navigation
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Interfaces.Navigation
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Interfaces.Navigation
#endif
{
    public interface INavigationProviderEx : INavigationProvider
    {
        [NotNull]
        INavigationService NavigationService { get; }
    }
}
