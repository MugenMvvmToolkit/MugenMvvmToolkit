#region Copyright

// ****************************************************************************
// <copyright file="IBindingSourceAccessor.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Accessors
{
    /// <summary>
    ///     Represents the accessor for the binding source.
    /// </summary>
    public interface IBindingSourceAccessor : IDisposable
    {
        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        ///     Gets the underlying sources.
        /// </summary>
        [NotNull]
        IList<IBindingSource> Sources { get; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        /// <param name="targetMember">The specified member to set value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be obtained; false to return
        ///     <see cref="BindingConstants.InvalidValue" /> if the value cannot be obtained.
        /// </param>
        [CanBeNull]
        object GetValue([NotNull] IBindingMemberInfo targetMember, [NotNull] IDataContext context, bool throwOnError);

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        bool SetValue([NotNull] IBindingSourceAccessor targetAccessor, [NotNull] IDataContext context, bool throwOnError);

        /// <summary>
        ///     Occurs before the value changes.
        /// </summary>
        event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;
    }
}