#region Copyright

// ****************************************************************************
// <copyright file="IAsyncOperation.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the async operation.
    /// </summary>
    public interface IAsyncOperation : IDisposable
    {
        /// <summary>
        ///     Gets whether the operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        IOperationResult Result { get; }

        /// <summary>
        ///     Waits for the operation to complete execution.
        /// </summary>
        void Wait();

        /// <summary>
        ///     Waits for the operation to complete execution.
        /// </summary>
        bool Wait(int millisecondsTimeout);

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        IAsyncOperation ContinueWith([NotNull] IActionContinuation continuationAction);

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        IAsyncOperation<TResult> ContinueWith<TResult>([NotNull] IFunctionContinuation<TResult> continuationFunction);

        /// <summary>
        ///     Converts the current <see cref="IAsyncOperation{TResult}" /> to the <see cref="IOperationCallback" />.
        /// </summary>
        [NotNull]
        IOperationCallback ToOperationCallback();
    }

    /// <summary>
    ///     Represents the async operation.
    /// </summary>
    public interface IAsyncOperation<out TResult> : IAsyncOperation
    {
        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        new IOperationResult<TResult> Result { get; }

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        IAsyncOperation ContinueWith([NotNull] IActionContinuation<TResult> continuationAction);

        /// <summary>
        ///     Creates a continuation that executes when the target operation completes.
        /// </summary>
        [NotNull]
        IAsyncOperation<TNewResult> ContinueWith<TNewResult>(
            [NotNull] IFunctionContinuation<TResult, TNewResult> continuationFunction);
    }

    internal interface IAsyncOperationInternal : IAsyncOperation, IContinuation
    {
        bool SetResult(IOperationResult result, bool throwOnError);
    }
}