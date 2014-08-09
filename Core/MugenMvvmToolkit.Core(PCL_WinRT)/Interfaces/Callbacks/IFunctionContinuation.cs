#region Copyright
// ****************************************************************************
// <copyright file="IFunctionContinuation.cs">
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
namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents a function to run when the <see cref="IAsyncOperation" /> completes.
    /// </summary>
    public interface IFunctionContinuation<out TResult> : IContinuation
    {
        /// <summary>
        ///     Invokes the function using the specified operation result.
        /// </summary>
        TResult Invoke(IOperationResult result);
    }

    /// <summary>
    ///     Represents a function to run when the <see cref="IAsyncOperation{TResult}" /> completes.
    /// </summary>
    public interface IFunctionContinuation<in TResult, out TNewResult> : IContinuation
    {
        /// <summary>
        ///     Invokes the function using the specified operation result.
        /// </summary>
        TNewResult Invoke(IOperationResult<TResult> result);
    }
}