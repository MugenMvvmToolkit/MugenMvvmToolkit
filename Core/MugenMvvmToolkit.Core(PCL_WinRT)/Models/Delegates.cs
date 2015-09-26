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
    public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    public delegate void NotifyCollectionChangingEventHandler(object sender, NotifyCollectionChangingEventArgs e);

    public delegate TValue UpdateValueDelegate<in TItem, in TNewValue, TValue, in TState>(
        TItem item, TNewValue addValue, TValue currentValue, TState state);

    [NotNull]
    public delegate TViewModel GetViewModelDelegate<out TViewModel>([NotNull] IIocContainer iocContainer)
        where TViewModel : class, IViewModel;

    public delegate bool FilterDelegate<in T>(T item);
}
