#region Copyright

// ****************************************************************************
// <copyright file="Delegates.cs">
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    /// Represents a method that handles general events.
    /// </summary>
    public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    /// <summary>
    ///     Represents the method that handles the CollectionChanging event.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Information about the event.</param>
    public delegate void NotifyCollectionChangingEventHandler(object sender, NotifyCollectionChangingEventArgs e);

    /// <summary>
    ///     Reprsents the attached value update delegate.
    /// </summary>
    public delegate TValue UpdateValueDelegate<in TItem, in TNewValue, TValue, in TState>(
        TItem item, TNewValue addValue, TValue currentValue, TState state);

    /// <summary>
    ///     Represents the delegate to create view model.
    /// </summary>
    /// <typeparam name="TViewModel">The type of view-model.</typeparam>
    /// <param name="iocContainer">The specified <see cref="IIocContainer" />.</param>
    /// <returns>An instance of <see cref="IViewModel" />.</returns>
    [NotNull]
    public delegate TViewModel GetViewModelDelegate<out TViewModel>([NotNull] IIocContainer iocContainer)
        where TViewModel : class, IViewModel;

    /// <summary>
    ///     Represents the method that used as a filter
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    /// <param name="item">The item to filter.</param>
    /// <returns>The result of filter.</returns>
    public delegate bool FilterDelegate<in T>(T item);
}