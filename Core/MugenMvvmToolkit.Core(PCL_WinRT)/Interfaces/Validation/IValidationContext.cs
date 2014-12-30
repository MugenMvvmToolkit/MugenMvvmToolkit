#region Copyright

// ****************************************************************************
// <copyright file="IValidationContext.cs">
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

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Describes the context in which a validation check is performed.
    /// </summary>
    public interface IValidationContext : IServiceProvider
    {
        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        /// <returns>
        ///     The object to validate.
        /// </returns>
        [NotNull]
        object ObjectInstance { get; }

        /// <summary>
        ///     Gets the type of the object to validate.
        /// </summary>
        /// <returns>
        ///     The type of the object to validate.
        /// </returns>
        [NotNull]
        Type ObjectType { get; }

        /// <summary>
        ///     Gets or sets the name of the member to validate.
        /// </summary>
        /// <returns>
        ///     The name of the member to validate.
        /// </returns>
        [NotNull]
        string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the member to validate.
        /// </summary>
        /// <returns>
        ///     The name of the member to validate.
        /// </returns>
        string MemberName { get; set; }

        /// <summary>
        ///     Gets the dictionary of key/value pairs that is associated with this context.
        /// </summary>
        /// <returns>
        ///     The dictionary of the key/value pairs for this context.
        /// </returns>
        IDictionary<object, object> Items { get; }

        /// <summary>
        ///     Gets the validation services provider.
        /// </summary>
        /// <returns>
        ///     The validation services provider.
        /// </returns>
        [CanBeNull]
        IServiceProvider ServiceProvider { get; }
    }
}