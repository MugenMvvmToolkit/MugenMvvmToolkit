using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Threading
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ThreadSwitcherAwaitable
    {
        private readonly ThreadExecutionMode? _executionMode;
        private readonly IThreadDispatcher? _threadDispatcher;

        public ThreadSwitcherAwaitable(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            _threadDispatcher = threadDispatcher;
            _executionMode = executionMode;
        }

        public ThreadSwitcherAwaiter GetAwaiter() => new(_threadDispatcher, _executionMode);

        [StructLayout(LayoutKind.Auto)]
        public readonly struct ThreadSwitcherAwaiter : ICriticalNotifyCompletion
        {
            private readonly ThreadExecutionMode? _executionMode;
            private readonly IThreadDispatcher? _threadDispatcher;

            internal ThreadSwitcherAwaiter(IThreadDispatcher? threadDispatcher, ThreadExecutionMode? executionMode)
            {
                _threadDispatcher = threadDispatcher;
                _executionMode = executionMode;
            }

            public bool IsCompleted => _threadDispatcher == null || _threadDispatcher.CanExecuteInline(_executionMode!);

            public void OnCompleted(Action continuation) => OnCompletedInternal(continuation);

            public void UnsafeOnCompleted(Action continuation) => OnCompletedInternal(continuation);

            public void GetResult()
            {
            }

            private void OnCompletedInternal(Action continuation)
            {
                if (_threadDispatcher == null)
                    continuation();
                else
                    _threadDispatcher.Execute(_executionMode!, continuation);
            }
        }
    }
}