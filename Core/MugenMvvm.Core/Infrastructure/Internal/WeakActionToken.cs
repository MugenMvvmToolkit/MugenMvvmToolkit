using System;
using System.Threading;

namespace MugenMvvm.Infrastructure.Internal
{
    public static class WeakActionToken
    {
        #region Methods

        public static IDisposable Create<TTarget>(TTarget target, Action<TTarget> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, object, object>(action, target);
        }

        public static IDisposable Create<TTarget, TArg1>(TTarget target, TArg1 arg1, Action<TTarget, TArg1> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1, object>(action, target, arg1);
        }

        public static IDisposable Create<TTarget, TArg1, TArg2>(TTarget target, TArg1 arg1, TArg2 arg2, Action<TTarget, TArg1, TArg2> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1, TArg2>(action, target, arg1, arg2);
        }

        private static WeakReference GetWeakReference(object target)
        {
            return target as WeakReference ?? MugenExtensions.GetWeakReference(target);
        }

        #endregion

        #region Nested types

        private sealed class WeakActionTokenInternal<TTarget, TArg1, TArg2> : IDisposable
            where TTarget : class
        {
            #region Fields

            private object _action;
            private object _target;

            #endregion

            #region Constructors

            public WeakActionTokenInternal(Action<TTarget> action, TTarget target)
            {
                Should.NotBeNull(action, nameof(action));
                Should.NotBeNull(target, nameof(target));
                _action = action;
                _target = GetWeakReference(target);
            }

            public WeakActionTokenInternal(Action<TTarget, TArg1> action, TTarget target, TArg1 arg1)
            {
                Should.NotBeNull(action, nameof(action));
                Should.NotBeNull(target, nameof(target));
                _action = action;
                _target = new object[]
                {
                    GetWeakReference(target),
                    arg1!
                };
            }

            public WeakActionTokenInternal(Action<TTarget, TArg1, TArg2> action, TTarget target, TArg1 arg1, TArg2 arg2)
            {
                Should.NotBeNull(action, nameof(action));
                Should.NotBeNull(target, nameof(target));
                _action = action;
                _target = new object[]
                {
                    GetWeakReference(target),
                    arg1!,
                    arg2!
                };
            }

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
                var action = Interlocked.Exchange(ref _action, null);
                if (action == null)
                    return;

                TTarget target;
                if (_target is WeakReference weakReference)
                {
                    target = (TTarget) weakReference.Target;
                    if (target != null)
                        ((Action<TTarget>) action).Invoke(target);
                }
                else
                {
                    var objects = (object[]) _target!;
                    if (typeof(TTarget) == typeof(WeakReference))
                        target = (TTarget) objects[0];
                    else
                        target = (TTarget) ((WeakReference) objects[0]).Target;
                    if (target != null)
                    {
                        switch (objects.Length)
                        {
                            case 2:
                                ((Action<TTarget, TArg1>) action).Invoke(target, (TArg1) objects[1]);
                                break;
                            case 3:
                                ((Action<TTarget, TArg1, TArg2>) action).Invoke(target, (TArg1) objects[1], (TArg2) objects[2]);
                                break;
                        }
                    }
                }

                _target = null!;
            }

            #endregion
        }

        #endregion
    }
}