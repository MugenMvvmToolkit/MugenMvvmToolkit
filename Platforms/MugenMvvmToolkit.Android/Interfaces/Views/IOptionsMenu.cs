#region Copyright

// ****************************************************************************
// <copyright file="IOptionsMenu.cs">
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

using Android.App;
using Android.Views;

namespace MugenMvvmToolkit.Android.Interfaces.Views
{
    public interface IOptionsMenu
    {
        void Inflate(Activity activity, IMenu menu);
    }
}