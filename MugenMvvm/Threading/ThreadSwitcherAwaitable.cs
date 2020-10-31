using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Threading
{
    public readonly struct ThreadSwitcherAwaitable
    {
        #region Fields

        private readonly ThreadExecutionMode? _executionMode;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        public ThreadSwitcherAwaitable(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(executionMode, nameof(executionMode));
            _threadDispatcher = threadDispatcher;
            _executionMode = executionMode;
        }

        #endregion

        #region Methods

        public ThreadSwitcherAwaiter GetAwaiter() => new ThreadSwitcherAwaiter(_threadDispatcher, _executionMode);

        #endregion

        #region Nested types

        public readonly struct ThreadSwitcherAwaiter : ICriticalNotifyCompletion
        {
            #region Fields

            private readonly ThreadExecutionMode? _executionMode;
            private readonly IThreadDispatcher? _threadDispatcher;

            #endregion

            #region Constructors

            internal ThreadSwitcherAwaiter(IThreadDispatcher? threadDispatcher, ThreadExecutionMode? executionMode)
            {
                _threadDispatcher = threadDispatcher;
                _executionMode = executionMode;
            }

            #endregion

            #region Properties

            public bool IsCompleted => _threadDispatcher == null || _threadDispatcher.CanExecuteInline(_executionMode!);

            #endregion

            #region Implementation of interfaces

            public void OnCompleted(Action continuation) => OnCompletedInternal(continuation);

            public void UnsafeOnCompleted(Action continuation) => OnCompletedInternal(continuation);

            #endregion

            #region Methods

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

            #endregion
        }

        #endregion
    }
}