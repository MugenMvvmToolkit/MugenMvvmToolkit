#region Copyright

// ****************************************************************************
// <copyright file="IBindingResourceObject.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the binding expression object.
    /// </summary>
    public interface IBindingResourceObject : ISourceValue
    {
        /// <summary>
        ///     Gets the type of object.
        /// </summary>
        [NotNull]
        Type Type { get; }
    }
}