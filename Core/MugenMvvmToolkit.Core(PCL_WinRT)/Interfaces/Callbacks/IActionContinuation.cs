#region Copyright
// ****************************************************************************
// <copyright file="IActionContinuation.cs">
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
    ///     Represents an action to run when the <see cref="IAsyncOperation" /> completes.
    /// </summary>
    public interface IActionContinuation : IContinuation
    {
        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
        void Invoke([NotNull] IOperationResult result);
    }

    /// <summary>
    ///     Represents an action to run when the <see cref="IAsyncOperation{TResult}" /> completes.
    /// </summary>
    public interface IActionContinuation<in TResult> : IContinuation
    {
        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
        void Invoke([NotNull] IOperationResult<TResult> result);
    }
}