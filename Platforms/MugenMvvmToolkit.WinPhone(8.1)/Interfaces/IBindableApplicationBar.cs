#region Copyright

// ****************************************************************************
// <copyright file="IBindableApplicationBar.cs">
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

using System.Collections;
using System.Windows;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.WinPhone.AppBar;

namespace MugenMvvmToolkit.WinPhone.Interfaces
{
    public interface IBindableApplicationBar : IApplicationBar
    {
        object DataContext { get; set; }

        DataTemplate ButtonItemTemplate { get; set; }

        DataTemplate MenuItemTemplate { get; set; }

        IApplicationBar OriginalApplicationBar { get; }

        IEnumerable ButtonItemsSource { get; set; }

        IEnumerable MenuItemsSource { get; set; }

        void Attach(object target);

        void Detach();
    }
}
