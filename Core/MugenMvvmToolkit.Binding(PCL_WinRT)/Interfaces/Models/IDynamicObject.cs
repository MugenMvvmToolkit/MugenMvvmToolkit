#region Copyright

// ****************************************************************************
// <copyright file="IDynamicObject.cs">
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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Provides a simple ineface that can be inherited from to create an object with dynamic behavior at runtime.
    /// </summary>
    public interface IDynamicObject
    {
        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        [CanBeNull]
        IDisposable TryObserve(string member, [NotNull] IEventListener listener);

        /// <summary>
        ///     Provides the implementation of getting a member.
        /// </summary>
        /// <returns>The result of the get operation.</returns>
        [CanBeNull]
        object GetMember([NotNull] string member, IList<object> args);

        /// <summary>
        ///     Provides the implementation of setting a member.
        /// </summary>
        void SetMember([NotNull] string member, IList<object> args);

        /// <summary>
        ///     Provides the implementation of calling a member.
        /// </summary>
        [CanBeNull]
        object InvokeMember([NotNull] string member, IList<object> args, IList<Type> typeArgs, IDataContext context);

        /// <summary>
        ///     Provides the implementation of performing a get index operation.
        /// </summary>
        [CanBeNull]
        object GetIndex(IList<object> indexes, IDataContext context);

        /// <summary>
        ///     Provides the implementation of performing a set index operation.
        /// </summary>
        void SetIndex(IList<object> indexes, IDataContext context);
    }
}