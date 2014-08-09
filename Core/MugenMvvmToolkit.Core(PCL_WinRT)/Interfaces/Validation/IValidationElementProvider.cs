#region Copyright
// ****************************************************************************
// <copyright file="IValidationElementProvider.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents the provider to get the <see cref="IValidationElement" />s for the specified instance.
    /// </summary>
    public interface IValidationElementProvider
    {
        /// <summary>
        ///     Gets the series of instances of <see cref="IValidationElement" /> for the specified instance.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <returns>A series of instances of <see cref="IValidationElement" />.</returns>
        [NotNull]
        IDictionary<string, IList<IValidationElement>> GetValidationElements([NotNull] object instance);
    }
}