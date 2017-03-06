#region Copyright

// ****************************************************************************
// <copyright file="Enums.cs">
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

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Models
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Models
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Binding.Models
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
