using System;
using System.Threading;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class PriorityLocker : ILocker
    {
        private static int _counter;
        private readonly int _priority;
        private int _priorityThreadWaitCount;

        public PriorityLocker()
        {
            _priority = Interlocked.Decrement(ref _counter);
        }

        public static Func<bool>? IsHighPriorityThread { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public int Priority => _priority;

        public override string ToString() => $"PriorityLocker{_priority}";

        public void Enter(ref bool lockTaken)
        {
            var checker = IsHighPriorityThread;
            if (checker == null || Monitor.IsEntered(this))
                Monitor.Enter(this, ref lockTaken);
            else
                Enter(checker(), ref lockTaken);
        }

        public void TryEnter(int timeout, ref bool lockTaken)
        {
            var checker = IsHighPriorityThread;
            if (checker == null || timeout == 0 || Monitor.IsEntered(this))
                Monitor.TryEnter(this, timeout, ref lockTaken);
            else
                TryEnter(checker(), timeout, ref lockTaken);
        }

        public void Exit() => Monitor.Exit(this);

        internal static ILocker GetLocker(object target) => MugenService.Optional<ILockerProvider>()?.GetLocker(target, null) ?? new PriorityLocker();

        private static bool CheckTime(uint startTime, int timeout)
        {
            var elapsedMilliseconds = (uint) Environment.TickCount - startTime;
            if (elapsedMilliseconds > int.MaxValue)
                return false;
            return timeout - (int) elapsedMilliseconds > 0;
        }

        private void TryEnter(bool isHighPriorityThread, int timeout, ref bool lockTaken)
        {
            var startTime = (uint) Environment.TickCount;
            var spinWait = new SpinWait();
            if (isHighPriorityThread)
                Interlocked.Increment(ref _priorityThreadWaitCount);
            while (true)
            {
                if (!isHighPriorityThread)
                {
                    while (Volatile.Read(ref _priorityThreadWaitCount) != 0)
                    {
                        spinWait.SpinOnce();
                        if (!CheckTime(startTime, timeout))
                            return;
                    }
                }

                Monitor.TryEnter(this, 0, ref lockTaken);
                if (!CheckTime(startTime, timeout))
                {
                    if (isHighPriorityThread)
                        Interlocked.Decrement(ref _priorityThreadWaitCount);
                    return;
                }

                if (lockTaken)
                {
                    if (isHighPriorityThread)
                    {
                        Interlocked.Decrement(ref _priorityThreadWaitCount);
                        return;
                    }

                    if (Volatile.Read(ref _priorityThreadWaitCount) == 0)
                        return;

                    Monitor.Exit(this);
                    lockTaken = false;
                }

                spinWait.SpinOnce();
            }
        }

        private void Enter(bool isHighPriorityThread, ref bool taken)
        {
            if (isHighPriorityThread)
                Interlocked.Increment(ref _priorityThreadWaitCount);
            while (true)
            {
                if (!isHighPriorityThread)
                {
                    var spinWait = new SpinWait();
                    while (Volatile.Read(ref _priorityThreadWaitCount) != 0)
                        spinWait.SpinOnce();
                }

                Monitor.Enter(this, ref taken);
                if (isHighPriorityThread)
                {
                    Interlocked.Decrement(ref _priorityThreadWaitCount);
                    return;
                }

                if (Volatile.Read(ref _priorityThreadWaitCount) == 0)
                    return;

                if (taken)
                {
                    Monitor.Exit(this);
                    taken = false;
                }
            }
        }
    }
}