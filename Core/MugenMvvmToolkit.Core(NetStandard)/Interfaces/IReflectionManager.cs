#region Copyright

// ****************************************************************************
// <copyright file="IReflectionManager.cs">
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
using System.Reflection;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IReflectionManager
    {
        [CanBeNull]
        Delegate TryCreateDelegate([NotNull]Type delegateType, [CanBeNull] object target, [NotNull]MethodInfo method);

        [NotNull]
        Func<object[], object> GetActivatorDelegate([NotNull]ConstructorInfo constructor);

        [NotNull]
        Func<object, object[], object> GetMethodDelegate([NotNull]MethodInfo method);

        [NotNull]
        Delegate GetMethodDelegate([NotNull]Type delegateType, [NotNull] MethodInfo method);

        [NotNull]
        Func<object, TType> GetMemberGetter<TType>([NotNull]MemberInfo member);

        [NotNull]
        Action<object, TType> GetMemberSetter<TType>([NotNull]MemberInfo member);
    }
}
