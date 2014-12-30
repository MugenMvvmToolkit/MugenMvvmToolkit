#region Copyright

// ****************************************************************************
// <copyright file="IBindingBehavior.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the interface that allows to extend the <see cref="IDataBinding" /> by adding new features.
    /// </summary>
    public interface IBindingBehavior
    {
        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        bool Attach([NotNull] IDataBinding binding);

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        void Detach([NotNull] IDataBinding binding);

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        [NotNull]
        IBindingBehavior Clone();
    }
}