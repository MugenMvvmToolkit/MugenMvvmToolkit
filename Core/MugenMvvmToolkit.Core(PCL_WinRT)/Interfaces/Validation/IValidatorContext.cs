#region Copyright

// ****************************************************************************
// <copyright file="IValidatorContext.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents the validation context.
    /// </summary>
    public interface IValidatorContext : IServiceProvider
    {
        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        [NotNull]
        object Instance { get; }

        /// <summary>
        ///     Gets or sets the validation metadata.
        /// </summary>
        [NotNull]
        IDataContext ValidationMetadata { get; }

        /// <summary>
        ///     Gets the mapping of model properties.
        /// </summary>
        [NotNull]
        IDictionary<string, ICollection<string>> PropertyMappings { get; }

        /// <summary>
        ///     Gets the list of properties that will not be validated.
        /// </summary>
        [NotNull]
        ICollection<string> IgnoreProperties { get; }

        /// <summary>
        ///     Gets the service provider.
        /// </summary>
        [CanBeNull]
        IServiceProvider ServiceProvider { get; }
    }
}