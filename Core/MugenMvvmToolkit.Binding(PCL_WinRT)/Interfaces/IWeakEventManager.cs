#region Copyright
// ****************************************************************************
// <copyright file="IWeakEventManager.cs">
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
using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the interface that allows to subscribe to events using weak references.
    /// </summary>
    public interface IWeakEventManager
    {
        /// <summary>
        ///     Attempts to subscribe to the event.
        /// </summary>
        [CanBeNull]
        IDisposable TrySubscribe([NotNull] object target, [NotNull] EventInfo eventInfo, [NotNull] IEventListener listener, IDataContext context = null);

        /// <summary>
        ///     Subscribes to the property changed event.
        /// </summary>
        [NotNull]
        IDisposable Subscribe([NotNull] INotifyPropertyChanged propertyChanged, [NotNull] string propertyName,
            [NotNull] IEventListener listener, IDataContext context = null);
    }
}