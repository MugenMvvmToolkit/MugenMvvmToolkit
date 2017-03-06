#region Copyright

// ****************************************************************************
// <copyright file="IAttachedBindingMemberInfo.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IAttachedBindingMemberInfo<in TTarget, TType> : IBindingMemberInfo
        where TTarget : class
    {
        TType GetValue(TTarget source, [CanBeNull] object[] args);

        object SetValue(TTarget source, TType value);

        object SetValue(TTarget source, object[] args);

        IDisposable TryObserve(TTarget source, IEventListener listener);
    }

    public interface INotifiableAttachedBindingMemberInfo : IBindingMemberInfo
    {
        bool Raise(object target, object message);
    }

    public interface INotifiableAttachedBindingMemberInfo<in TTarget, TType> :
        IAttachedBindingMemberInfo<TTarget, TType>, INotifiableAttachedBindingMemberInfo
        where TTarget : class
    {
        bool Raise(TTarget target, object message);
    }
}
