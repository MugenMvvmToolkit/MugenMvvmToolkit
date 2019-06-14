using System;
using System.Threading;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Infrastructure.Internal
{
    public static class WeakActionToken
    {
        #region Methods

        public static IDisposable Create<TTarget>(TTarget target, Action<TTarget> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget>(action, target);
        }

        public static IDisposable Create<TTarget, TArg1>(TTarget target, TArg1 arg1, Action<TTarget, TArg1> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1>(action, target, arg1);
        }

        public static IDisposable Create<TTarget, TArg1, TArg2>(TTarget target, TArg1 arg1, TArg2 arg2, Action<TTarget, TArg1, TArg2> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1, TArg2>(action, target, arg1, arg2);
        }

        private static IWeakReference GetWeakReference(object target)
        {
            return Service<IWeakReferenceProvider>.Instance.GetWeakReference(target, Default.Metadata);
        }

        #endregion

        #region Nested types

        private class WeakActionTokenInternal<TTarget> : IDisposable where TTarget : class
        {
            #region Fields

            private object? _action;
            private IWeakReference? _target;

            #endregion

            #region Constructors

            public WeakActionTokenInternal(object action, TTarget target)
            {
                Should.NotBeNull(action, nameof(action));
                Should.NotBeNull(target, nameof(target));
                _action = action;
                _target = GetWeakReference(target);
            }

            #endregion

            #region Implementation of interfaces

            void IDisposable.Dispose()
            {
                var action = Interlocked.Exchange(ref _action, null);
                if (action == null)
                    return;
                var target = _target?.Target;
                if (target != null)
                    OnDispose(action, (TTarget) target);
                _target = null;
            }

            #endregion

            #region Methods

            protected virtual void OnDispose(object action, TTarget target)
            {
                ((Action<TTarget>) action).Invoke(target);
            }

            #endregion
        }

        private class WeakActionTokenInternal<TTarget, TArg1> : WeakActionTokenInternal<TTarget> where TTarget : class
        {
            #region Fields

            protected TArg1 Arg1;

            #endregion

            #region Constructors

            public WeakActionTokenInternal(object action, TTarget target, TArg1 arg1) : base(action, target)
            {
                Arg1 = arg1;
            }

            #endregion

            #region Methods

            protected override void OnDispose(object action, TTarget target)
            {
                ((Action<TTarget, TArg1>) action).Invoke(target, Arg1);
                Arg1 = default;
            }

            #endregion
        }

        private class WeakActionTokenInternal<TTarget, TArg1, TArg2> : WeakActionTokenInternal<TTarget, TArg1> where TTarget : class
        {
            #region Fields

            protected TArg2 Arg2;

            #endregion

            #region Constructors

            public WeakActionTokenInternal(object action, TTarget target, TArg1 arg1, TArg2 arg2)
                : base(action, target, arg1)
            {
                Arg2 = arg2;
            }

            #endregion

            #region Methods

            protected override void OnDispose(object action, TTarget target)
            {
                ((Action<TTarget, TArg1, TArg2>) action).Invoke(target, Arg1, Arg2);
                Arg1 = default;
                Arg2 = default;
            }

            #endregion
        }

        #endregion
    }
}