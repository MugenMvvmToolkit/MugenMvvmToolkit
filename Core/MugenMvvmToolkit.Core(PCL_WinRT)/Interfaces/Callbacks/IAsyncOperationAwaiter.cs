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
    public interface IAsyncOperationAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }

        void GetResult();
    }

    public interface IAsyncOperationAwaiter<out TResult> : INotifyCompletion
    {
        bool IsCompleted { get; }

        TResult GetResult();
    }

    public interface IAsyncStateMachineAware
    {
        void SetStateMachine(IAsyncStateMachine stateMachine);
    }
}
