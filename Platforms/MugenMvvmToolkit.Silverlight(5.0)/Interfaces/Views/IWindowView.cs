#region Copyright

// ****************************************************************************
// <copyright file="IWindowView.cs">
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

using System;
using System.ComponentModel;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Silverlight.Interfaces.Views
{
    public interface IWindowView : IView
    {
        void Show();

        void Close();

        void Activate();

        event EventHandler<CancelEventArgs> Closing;
    }
}
