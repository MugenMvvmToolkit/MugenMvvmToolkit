#region Copyright
// ****************************************************************************
// <copyright file="IDataBinding.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the interface that provides high-level access to the definition of a binding, which connects the
    ///     properties of binding target objects and any data source.
    /// </summary>
    public interface IDataBinding : IDisposable
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        ///     Gets the current <see cref="IDataContext" />.
        /// </summary>
        [NotNull]
        IDataContext Context { get; }

        /// <summary>
        ///     Gets the binding target accessor.
        /// </summary>
        [NotNull]
        ISingleBindingSourceAccessor TargetAccessor { get; }

        /// <summary>
        ///     Gets the binding source accessor.
        /// </summary>
        [NotNull]
        IBindingSourceAccessor SourceAccessor { get; }

        /// <summary>
        ///     Gets the binding behaviors.
        /// </summary>
        [NotNull]
        ICollection<IBindingBehavior> Behaviors { get; }

        /// <summary>
        ///     Sends the current value back to the source.
        /// </summary>
        void UpdateSource();

        /// <summary>
        ///     Forces a data transfer from source to target.
        /// </summary>
        void UpdateTarget();

        /// <summary>
        ///     Validates the current binding and raises the BindingException event if needed.
        /// </summary>
        bool Validate();

        /// <summary>
        ///     Occurs when the binding updates the values.
        /// </summary>
        event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;

        /// <summary>
        ///     Occurs when an exception is not caught.
        /// </summary>
        event EventHandler<IDataBinding, BindingExceptionEventArgs> BindingException;
    }
}