using System;
using System.Reflection;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    internal sealed class ActionSubscriber<T> : ISubscriber
    {
        #region Fields

        private readonly Action<object, T> _action;
        private readonly int _hash;

        #endregion

        #region Constructors

        public ActionSubscriber(Action<object, T> action)
        {
            Should.NotBeNull(action, "action");
            _action = action;
            object target = action.Target;
            _hash = (target == null ? 0 : target.GetHashCode() * 397) ^ action.GetMethodInfo().GetHashCode();
        }

        #endregion

        #region Implementation of ISubscriber

        public bool IsAlive
        {
            get { return true; }
        }

        public bool AllowDuplicate
        {
            get { return true; }
        }

        public object Target
        {
            get { return _action.Target ?? _action.GetMethodInfo(); }
        }

        public HandlerResult Handle(object sender, object message)
        {
            if (message is T)
            {
                _action(sender, (T)message);
                return HandlerResult.Handled;
            }
            return HandlerResult.Ignored;
        }

        #endregion

        #region Equality members

        public static bool ActionSubscriberEquals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            object xTarget;
            MethodInfo xMethod;
            if (!TryGetMethodAndTarget(x, out xTarget, out xMethod))
                return false;

            object yTarget;
            MethodInfo yMethod;
            if (!TryGetMethodAndTarget(y, out yTarget, out yMethod))
                return false;
            return Equals(xTarget, yTarget) && Equals(xMethod, yMethod);
        }

        private static bool TryGetMethodAndTarget(object subscriber, out object target, out MethodInfo method)
        {
            var actionSubscriber = subscriber as ActionSubscriber<T>;
            if (actionSubscriber != null)
            {
                target = actionSubscriber.Target;
                method = actionSubscriber._action.GetMethodInfo();
                return true;
            }
            var weakActionSubscriber = subscriber as WeakActionSubscriber<T>;
            if (weakActionSubscriber != null)
            {
                target = weakActionSubscriber.Target;
                method = weakActionSubscriber.Method;
                return true;
            }
            target = null;
            method = null;
            return false;
        }

        public bool Equals(ISubscriber other)
        {
            return ActionSubscriberEquals(this, other);
        }

        public override bool Equals(object obj)
        {
            return ActionSubscriberEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion
    }
}