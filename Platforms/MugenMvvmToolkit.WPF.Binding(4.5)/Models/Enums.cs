#region Copyright

// ****************************************************************************
// <copyright file="Enums.cs">
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

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Models
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Models
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Models
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Binding.Models
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Models
#endif
{
    public enum BindingModeCore
    {
        Default = 0,
        TwoWay = 1,
        OneWay = 2,
        OneTime = 3,
        OneWayToSource = 4,
        None = 5
    }

    public enum UpdateSourceTriggerCore
    {
        Default = 0,
        PropertyChanged = 1,
        LostFocus = 2,
    }
}
