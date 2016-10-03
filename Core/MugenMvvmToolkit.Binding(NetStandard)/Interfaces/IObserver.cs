#region Copyright

// ****************************************************************************
// <copyright file="IObserver.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IObserver : IDisposable
    {
        bool IsAlive { get; }

        [NotNull]
        IBindingPath Path { get; }

        [CanBeNull]
        object Source { get; }

        void Update();

        bool Validate(bool throwOnError);

        [CanBeNull]
        object GetActualSource(bool throwOnError);

        [NotNull]
        IBindingPathMembers GetPathMembers(bool throwOnError);

        event EventHandler<IObserver, ValueChangedEventArgs> ValueChanged;
    }
}
