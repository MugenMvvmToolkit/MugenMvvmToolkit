#region Copyright
// ****************************************************************************
// <copyright file="ISingleBindingSourceAccessor.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Sources;

namespace MugenMvvmToolkit.Binding.Interfaces.Accessors
{
    /// <summary>
    ///     Represents the accessor for the binding source.
    /// </summary>
    public interface ISingleBindingSourceAccessor : IBindingSourceAccessor
    {
        /// <summary>
        ///     Gets the underlying source.
        /// </summary>
        [NotNull]
        IBindingSource Source { get; }
    }
}