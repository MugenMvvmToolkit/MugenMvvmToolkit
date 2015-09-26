#region Copyright

// ****************************************************************************
// <copyright file="WeakActionToken.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Infrastructure
{
    public static class WeakActionToken
    {
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
                Should.NotBeNull(action, "action");
                Should.NotBeNull(target, "target");
                _action = action;
                _target = GetWeakReference(target);
            }

            public WeakActionTokenInternal(Action<TTarget, TArg1> action, TTarget target, TArg1 arg1)
            {
                Should.NotBeNull(action, "action");
                Should.NotBeNull(target, "target");
                _action = action;
                _target = new object[]
                {
                    GetWeakReference(target),
                    arg1
                };
            }

            public WeakActionTokenInternal(Action<TTarget, TArg1, TArg2> action, TTarget target, TArg1 arg1,
                TArg2 arg2)
            {
                Should.NotBeNull(action, "action");
                Should.NotBeNull(target, "target");
                _action = action;
                _target = new object[]
                {
                    GetWeakReference(target),
                    arg1,
                    arg2
                };
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (_action == null)
                    return;
                object action;
                lock (_target)
                {
                    if (_action == null)
                        return;
                    action = _action;
                    _action = null;
                }
                TTarget target;
                var weakReference = _target as WeakReference;
                if (weakReference != null)
                {
                    target = (TTarget)weakReference.Target;
                    if (target != null)
                        ((Action<TTarget>)action).Invoke(target);
                }
                else
                {
                    var objs = (object[])_target;
                    if (typeof(TTarget) == typeof(WeakReference))
                        target = (TTarget)objs[0];
                    else
                        target = (TTarget)((WeakReference)objs[0]).Target;
                    if (target != null)
                    {
                        switch (objs.Length)
                        {
                            case 2:
                                ((Action<TTarget, TArg1>)action).Invoke(target, (TArg1)objs[1]);
                                break;
                            case 3:
                                ((Action<TTarget, TArg1, TArg2>)action).Invoke(target, (TArg1)objs[1], (TArg2)objs[2]);
                                break;
                        }
                    }
                }
                _target = this;
            }

            #endregion
        }

        #endregion

        #region Methods

        public static IDisposable Create<TTarget>([NotNull] TTarget target, [NotNull] Action<TTarget> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, object, object>(action, target);
        }

        public static IDisposable Create<TTarget, TArg1>([NotNull] TTarget target, TArg1 arg1,
            [NotNull] Action<TTarget, TArg1> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1, object>(action, target, arg1);
        }

        public static IDisposable Create<TTarget, TArg1, TArg2>([NotNull] TTarget target, TArg1 arg1, TArg2 arg2,
            [NotNull] Action<TTarget, TArg1, TArg2> action)
            where TTarget : class
        {
            return new WeakActionTokenInternal<TTarget, TArg1, TArg2>(action, target, arg1, arg2);
        }

        private static WeakReference GetWeakReference(object target)
        {
            return target as WeakReference ?? ToolkitExtensions.GetWeakReference(target);
        }

        #endregion
    }
}
