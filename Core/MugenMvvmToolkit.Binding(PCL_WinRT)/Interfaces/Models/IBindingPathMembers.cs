#region Copyright

// ****************************************************************************
// <copyright file="IBindingPathMembers.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IBindingPathMembers
    {
        [NotNull]
        IBindingPath Path { get; }

        bool AllMembersAvailable { get; }

        [NotNull]
        IList<IBindingMemberInfo> Members { get; }

        [NotNull]
        IBindingMemberInfo LastMember { get; }

        [CanBeNull]
        object Source { get; }

        [CanBeNull]
        object PenultimateValue { get; }
    }
}
