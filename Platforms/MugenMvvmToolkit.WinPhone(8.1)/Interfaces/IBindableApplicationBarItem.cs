#region Copyright

// ****************************************************************************
// <copyright file="IBindableApplicationBarItem.cs">
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

using System.Windows.Input;
using Microsoft.Phone.Shell;

namespace MugenMvvmToolkit.WinPhone.Interfaces
{
    public interface IBindableApplicationBarItem : IApplicationBarMenuItem
    {
        object DataContext { get; set; }

        IApplicationBarMenuItem ApplicationBarItem { get; }

        bool IsVisible { get; set; }

        ICommand Command { get; set; }

        object CommandParameter { get; set; }

        void Attach(IBindableApplicationBar applicationBar, int position);

        void Detach();
    }
}
