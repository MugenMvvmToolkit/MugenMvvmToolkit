#region Copyright

// ****************************************************************************
// <copyright file="IVisualTreeManager.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IVisualTreeManager
    {
        [CanBeNull]
        IBindingMemberInfo GetRootMember([NotNull] Type type);

        [CanBeNull]
        IBindingMemberInfo GetParentMember([NotNull] Type type);

        [CanBeNull]
        object GetParent([NotNull] object target);

        [CanBeNull]
        object FindByName([NotNull] object target, [NotNull] string elementName);

        [CanBeNull]
        object FindRelativeSource([NotNull] object target, [NotNull] string typeName, uint level);
    }
}
