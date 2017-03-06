#region Copyright

// ****************************************************************************
// <copyright file="IAsyncOperation.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    public interface IAsyncOperation
    {
        bool IsCompleted { get; }

        IOperationResult Result { get; }

        [NotNull]
        IDataContext Context { get; }

        void Wait();

        bool Wait(int millisecondsTimeout);

        [NotNull]
        IAsyncOperation ContinueWith([NotNull] IActionContinuation continuationAction);

        [NotNull]
        IAsyncOperation<TResult> ContinueWith<TResult>([NotNull] IFunctionContinuation<TResult> continuationFunction);

        [NotNull]
        IOperationCallback ToOperationCallback();
    }

    public interface IAsyncOperation<out TResult> : IAsyncOperation
    {
        new IOperationResult<TResult> Result { get; }

        [NotNull]
        IAsyncOperation ContinueWith([NotNull] IActionContinuation<TResult> continuationAction);

        [NotNull]
        IAsyncOperation<TNewResult> ContinueWith<TNewResult>([NotNull] IFunctionContinuation<TResult, TNewResult> continuationFunction);
    }

    internal interface IAsyncOperationInternal : IAsyncOperation, IContinuation
    {
        bool SetResult(IOperationResult result, bool throwOnError);
    }
}
