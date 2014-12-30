#region Copyright

// ****************************************************************************
// <copyright file="IAsyncOperationAwaiter.cs">
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

using System.Runtime.CompilerServices;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the async operation awaiter.
    /// </summary>
    public interface IAsyncOperationAwaiter : INotifyCompletion
    {
        /// <summary>
        ///     Gets whether the operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        void GetResult();
    }

    /// <summary>
    ///     Represents the async operation awaiter.
    /// </summary>
    public interface IAsyncOperationAwaiter<out TResult> : INotifyCompletion
    {
        /// <summary>
        ///     Gets whether the operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        ///     Gets the operation result.
        /// </summary>
        TResult GetResult();
    }

    /// <summary>
    ///     Represents the interface that allows to get state machine.
    /// </summary>
    public interface IAsyncStateMachineAware
    {
        /// <summary>
        ///     Configures the state machine with a heap-allocated replica.
        /// </summary>
        /// <param name="stateMachine">The heap-allocated replica.</param>
        void SetStateMachine(IAsyncStateMachine stateMachine);
    }
}