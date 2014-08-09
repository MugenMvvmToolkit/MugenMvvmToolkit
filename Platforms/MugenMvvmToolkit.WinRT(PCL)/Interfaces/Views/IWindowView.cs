#region Copyright
// ****************************************************************************
// <copyright file="IWindowView.cs">
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
using System.ComponentModel;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for dialog view.
    /// </summary>
    [BaseView(Priority = 1, ViewTypeName = "Windows.UI.Xaml.Controls.ContentDialog")]
    public interface IWindowView : IWindowViewBase
    {
        /// <summary>
        ///     Shows dialog as a window.
        /// </summary>
        void Show();

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        void Close();

        /// <summary>
        ///     Occurred on closing.
        /// </summary>
        event EventHandler<object, CancelEventArgs> Closing;
    }
}