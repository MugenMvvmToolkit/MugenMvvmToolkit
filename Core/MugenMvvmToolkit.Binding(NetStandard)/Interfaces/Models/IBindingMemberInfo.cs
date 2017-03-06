#region Copyright

// ****************************************************************************
// <copyright file="IBindingMemberInfo.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IBindingMemberInfo
    {
        [NotNull]
        string Path { get; }

        [NotNull]
        Type Type { get; }

        [CanBeNull]
        object Member { get; }

        [NotNull]
        BindingMemberType MemberType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanObserve { get; }

        object GetValue(object source, [CanBeNull] object[] args);

        object SetValue(object source, object[] args);

        object SetSingleValue(object source, object value);

        [CanBeNull]
        IDisposable TryObserve(object source, [NotNull]IEventListener listener);
    }
}
