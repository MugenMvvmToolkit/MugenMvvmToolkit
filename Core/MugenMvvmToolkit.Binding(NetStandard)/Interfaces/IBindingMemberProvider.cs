#region Copyright

// ****************************************************************************
// <copyright file="IBindingMemberProvider.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingMemberProvider
    {
        IBindingMemberInfo GetBindingMember([NotNull] Type sourceType, [NotNull] string path, bool ignoreAttachedMembers, bool throwOnError);

        void Register([NotNull] Type type, [NotNull] IBindingMemberInfo member, bool rewrite);

        void Register([NotNull] Type type, string path, [NotNull] IBindingMemberInfo member, bool rewrite);

        bool Unregister([NotNull] Type type, [NotNull] string path);

        bool Unregister([NotNull] Type type);

        ICollection<KeyValuePair<string, IBindingMemberInfo>> GetAttachedMembers([NotNull] Type type);
    }
}
