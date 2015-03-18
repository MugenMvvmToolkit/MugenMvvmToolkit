#region Copyright

// ****************************************************************************
// <copyright file="IViewModelSettings.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the view-model settings.
    /// </summary>
    public interface IViewModelSettings
    {
        /// <summary>
        ///     Gets or sets property, that is responsible for broadcast all messages through all view models in chain.
        /// </summary>
        bool BroadcastAllMessages { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose container when the view model disposing.
        /// </summary>
        bool DisposeIocContainer { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose all command when the view model disposing.
        /// </summary>
        bool DisposeCommands { get; set; }

        /// <summary>
        ///     Gets or sets the value that is responsible for listen busy messages.
        /// </summary>
        HandleMode HandleBusyMessageMode { get; set; }

        /// <summary>
        ///     Gets or sets value that will be displayed when the BeginIsBusy method will be invoked without a message.
        /// </summary>
        object DefaultBusyMessage { get; set; }

        /// <summary>
        ///     Specifies the execution mode for invoke events (<c>ErrorsChanged</c>, <c>SelectedItemChanged</c>, etc).
        /// </summary>
        ExecutionMode EventExecutionMode { get; set; }

        /// <summary>
        ///     Gets the metadata context of current view model.
        /// </summary>
        [NotNull]
        IDataContext Metadata { get; }

        /// <summary>
        ///     Gets the serializable state of view model.
        /// </summary>
        [NotNull]
        IDataContext State { get; }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        [NotNull]
        IViewModelSettings Clone();
    }
}