#region Copyright

// ****************************************************************************
// <copyright file="IToastPresenter.cs">
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

using System.Threading.Tasks;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    /// <summary>
    ///     Provides functionality to present a timed message.
    /// </summary>
    public interface IToastPresenter
    {
        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        [SuppressTaskBusyHandler]
        Task ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom,
            IDataContext context = null);
    }
}