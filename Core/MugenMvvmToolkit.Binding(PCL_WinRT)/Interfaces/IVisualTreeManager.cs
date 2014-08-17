#region Copyright
// ****************************************************************************
// <copyright file="IVisualTreeManager.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     Represents the visual tree manager.
    /// </summary>
    public interface IVisualTreeManager
    {
        /// <summary>
        ///     Gets the root member, if any.
        /// </summary>
        [CanBeNull]
        IBindingMemberInfo GetRootMember([NotNull] Type type);

        /// <summary>
        ///     Gets the parent member, if any.
        /// </summary>
        [CanBeNull]
        IBindingMemberInfo GetParentMember([NotNull] Type type);

        /// <summary>
        ///     Tries to find parent.
        /// </summary>
        [CanBeNull]
        object FindParent([NotNull] object target);

        /// <summary>
        ///     Tries to find element by it's name.
        /// </summary>
        [CanBeNull]
        object FindByName([NotNull] object target, [NotNull] string elementName);

        /// <summary>
        ///     Tries to find relative source.
        /// </summary>
        [CanBeNull]
        object FindRelativeSource([NotNull] object target, [NotNull] string typeName, uint level);
    }
}