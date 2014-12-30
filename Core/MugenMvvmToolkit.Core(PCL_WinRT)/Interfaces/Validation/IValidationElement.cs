#region Copyright

// ****************************************************************************
// <copyright file="IValidationElement.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Serves as the base interface for all validation elements.
    /// </summary>
    public interface IValidationElement
    {
        /// <summary>
        ///     Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        ///     A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The context information about the validation operation.</param>
        [NotNull]
        IEnumerable<IValidationResult> Validate([NotNull]IValidationContext validationContext);
    }
}