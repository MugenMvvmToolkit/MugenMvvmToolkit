#region Copyright

// ****************************************************************************
// <copyright file="ISubscriber.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface ISubscriber : IEquatable<ISubscriber>
    {
        bool IsAlive { get; }

        bool AllowDuplicate { get; }

        object Target { get; }

        HandlerResult Handle(object sender, object message);
    }
}
