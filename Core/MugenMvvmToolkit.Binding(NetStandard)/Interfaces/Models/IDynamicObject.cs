#region Copyright

// ****************************************************************************
// <copyright file="IDynamicObject.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    [Preserve(AllMembers = true)]
    public interface IDynamicObject
    {
        [CanBeNull]
        IDisposable TryObserve(string member, [NotNull] IEventListener listener);

        [CanBeNull]
        object GetMember([NotNull] string member, IList<object> args);

        void SetMember([NotNull] string member, IList<object> args);

        [CanBeNull]
        object InvokeMember([NotNull] string member, IList<object> args, IList<Type> typeArgs, IDataContext context);

        [CanBeNull]
        object GetIndex(IList<object> indexes, IDataContext context);

        void SetIndex(IList<object> indexes, IDataContext context);
    }
}
