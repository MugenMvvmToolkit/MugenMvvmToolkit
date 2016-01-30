#region Copyright

// ****************************************************************************
// <copyright file="IOrientationChangeListener.cs">
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

#if XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Interfaces
#else
namespace MugenMvvmToolkit.iOS.Interfaces
#endif
{
    public interface IOrientationChangeListener
    {
        void OnOrientationChanged();
    }
}
