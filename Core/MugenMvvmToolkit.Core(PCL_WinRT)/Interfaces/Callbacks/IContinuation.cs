#region Copyright
// ****************************************************************************
// <copyright file="IContinuation.cs">
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

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the base interfaces for all continuations.
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        ///     Tries to convert current operation to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        [CanBeNull]
        ISerializableCallback ToSerializableCallback();
    }
}