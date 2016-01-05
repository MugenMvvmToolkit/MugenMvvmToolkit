#region Copyright

// ****************************************************************************
// <copyright file="IWeakEventManager.cs">
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

using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IWeakEventManager
    {
        [CanBeNull]
        IDisposable TrySubscribe([NotNull] object target, [NotNull] EventInfo eventInfo, [NotNull] IEventListener listener, IDataContext context = null);

        [NotNull]
        IDisposable Subscribe([NotNull] INotifyPropertyChanged propertyChanged, [NotNull] string propertyName,
            [NotNull] IEventListener listener, IDataContext context = null);
    }
}
