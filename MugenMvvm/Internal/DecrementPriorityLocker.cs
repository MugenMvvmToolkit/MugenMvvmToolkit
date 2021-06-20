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

        public int Priority => _priority;

        public object SyncRoot => this;

        public override string ToString() => $"DecrementPriorityLocker{_priority}";
    }
}