using System.Threading;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class DecrementPriorityLocker : ILocker
    {
        private static int _counter;
        private readonly int _priority;

        public DecrementPriorityLocker()
        {
            _priority = Interlocked.Decrement(ref _counter);
        }

        // ReSharper disable once ConvertToAutoProperty
        public int Priority => _priority;

        public override string ToString() => $"DecrementPriorityLocker{_priority}";

        public void Enter(ref bool lockTaken) => Monitor.Enter(this, ref lockTaken);

        public void Exit() => Monitor.Exit(this);
    }
}