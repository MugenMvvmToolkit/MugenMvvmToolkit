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
using System.Windows.Forms;
using MugenMvvmToolkit.Annotations;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for a window view.
    /// </summary>
    [BaseView(Priority = 1, ViewTypeName = "System.Windows.Forms.Form")]
    public interface IWindowView : IView
    {
        /// <summary>
        ///     Shows window.
        /// </summary>
        void Show();

        /// <summary>
        ///     Shows window as dialog.
        /// </summary>
        /// <returns></returns>
        DialogResult ShowDialog();

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        void Close();

        /// <summary>
        ///     Occurred on closing window.
        /// </summary>
        event CancelEventHandler Closing;
    }
}