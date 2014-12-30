#region Copyright

// ****************************************************************************
// <copyright file="IBindingSourceDecorator.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the target manager.
    /// </summary>
    public interface IBindingSourceDecorator
    {
        /// <summary>
        ///     Gets the priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Decorates the specified <see cref="IBindingSource" />.
        /// </summary>
        void Decorate([NotNull] ref IBindingSource source, bool isTarget, [CanBeNull] IDataContext context);
    }
}