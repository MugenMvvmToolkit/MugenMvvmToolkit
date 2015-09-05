#region Copyright

// ****************************************************************************
// <copyright file="ISingleBindingSourceAccessor.cs">
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
        IObserver Source { get; }
    }
}