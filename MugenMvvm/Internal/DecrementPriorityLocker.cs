using System.Threading;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class DecrementPriorityLocker : ILocker
    {
        private static int _counter;

        public DecrementPriorityLocker()
        {
            Priority = Interlocked.Decrement(ref _counter);
        }

        public int Priority { get; }

        public override string ToString() => $"DecrementPriorityLocker{Priority}";

        public void Enter(ref bool lockTaken) => Monitor.Enter(this, ref lockTaken);

        public void Exit() => Monitor.Exit(this);
    }
}