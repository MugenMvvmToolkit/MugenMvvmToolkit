#region Copyright

// ****************************************************************************
// <copyright file="IEventAggregator.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IEventAggregator : IObservable, IEventPublisher
    {
        bool Contains([NotNull] ISubscriber subscriber);

        void UnsubscribeAll();

        [NotNull]
        IList<ISubscriber> GetSubscribers();
    }
}
