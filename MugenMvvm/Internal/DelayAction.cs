using System;
using System.Threading;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal
{
    public static class DelayAction
    {
        public static int? DelayOverride;

        [ThreadStatic]
        public static int? DelayOverrideThreadStatic;

        public static int GetDelay(int delay) => DelayOverrideThreadStatic.GetValueOrDefault(DelayOverride.GetValueOrDefault(delay));

        public static Action<T> Get<T>(int delay, Action<T> action) => action.WithDelay(delay);

        public static Action<T1, T2> Get<T1, T2>(int delay, Action<T1, T2> action) => action.WithDelay(delay);

        public static Action<T1, T2, T3> Get<T1, T2, T3>(int delay, Action<T1, T2, T3> action) => action.WithDelay(delay);

        public static Action<T1, T2, T3, T4> Get<T1, T2, T3, T4>(int delay, Action<T1, T2, T3, T4> action) => action.WithDelay(delay);

        public static Action<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>(int delay, Action<T1, T2, T3, T4, T5> action) => action.WithDelay(delay);

        public static Action<T> WithDelay<T>(this Action<T> action, int delay) => new DelayAction<Action<T>, T>(action, delay, (del, s) => del(s!)).Invoke;

        public static Action<T1, T2> WithDelay<T1, T2>(this Action<T1, T2> action, int delay) =>
            new DelayAction<Action<T1, T2>, (T1, T2)>(action, delay, (del, s) => del(s.Item1, s.Item2)).Invoke;

        public static Action<T1, T2, T3> WithDelay<T1, T2, T3>(this Action<T1, T2, T3> action, int delay) =>
            new DelayAction<Action<T1, T2, T3>, (T1, T2, T3)>(action, delay, (del, s) => del(s.Item1, s.Item2, s.Item3)).Invoke;

        public static Action<T1, T2, T3, T4> WithDelay<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, int delay) =>
            new DelayAction<Action<T1, T2, T3, T4>, (T1, T2, T3, T4)>(action, delay, (del, s) => del(s.Item1, s.Item2, s.Item3, s.Item4)).Invoke;

        public static Action<T1, T2, T3, T4, T5> WithDelay<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, int delay) =>
            new DelayAction<Action<T1, T2, T3, T4, T5>, (T1, T2, T3, T4, T5)>(action, delay, (del, s) => del(s.Item1, s.Item2, s.Item3, s.Item4, s.Item5)).Invoke;

        private static void Invoke<T1, T2>(this DelayAction<Action<T1, T2>, (T1, T2)> action, T1 arg1, T2 arg2) => action.Invoke((arg1, arg2));

        private static void Invoke<T1, T2, T3>(this DelayAction<Action<T1, T2, T3>, (T1, T2, T3)> action, T1 arg1, T2 arg2, T3 arg3) => action.Invoke((arg1, arg2, arg3));

        private static void Invoke<T1, T2, T3, T4>(this DelayAction<Action<T1, T2, T3, T4>, (T1, T2, T3, T4)> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
            action.Invoke((arg1, arg2, arg3, arg4));

        private static void Invoke<T1, T2, T3, T4, T5>(this DelayAction<Action<T1, T2, T3, T4, T5>, (T1, T2, T3, T4, T5)> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
            action.Invoke((arg1, arg2, arg3, arg4, arg5));
    }

    public sealed class DelayAction<TDelegate, TState> : IHasDisposeState
        where TDelegate : Delegate
    {
        private readonly Action<TDelegate, TState?> _action;
        private TDelegate? _target;
        private Timer? _timer;
        private TState? _state;

        public DelayAction(TDelegate target, int delay, Action<TDelegate, TState?> action)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            _target = target;
            _action = action;
            Delay = delay;
            _timer = WeakTimer.Get(this, h => h.Handle());
        }

        public int Delay { get; set; }

        public bool IsDisposed => _timer == null;

        public void Invoke(TState? state)
        {
            _state = state;
            var delay = DelayAction.GetDelay(Delay);
            if (delay == 0)
                Handle();
            else
                _timer?.SafeChange(delay, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
            _target = null;
        }

        private void Handle()
        {
            var target = _target;
            if (target != null)
                _action(target, _state);
        }
    }
}