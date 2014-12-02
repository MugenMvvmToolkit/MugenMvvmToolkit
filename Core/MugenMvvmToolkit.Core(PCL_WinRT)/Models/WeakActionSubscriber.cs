using System;
using System.Reflection;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    internal class WeakActionSubscriber<T> : ISubscriber
    {
        #region Fields

        private readonly Action<object, object, T> _delegate;
        private readonly int _hash;
        internal readonly MethodInfo Method;
        private readonly WeakReference _reference;

        #endregion

        #region Constructors

        public WeakActionSubscriber(object target, MethodInfo method)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(method, "method");
            _reference = ToolkitExtensions.GetWeakReference(target);
            Method = method;
            _delegate = (Action<object, object, T>)ServiceProvider
                .ReflectionManager
                .GetMethodDelegate(typeof(Action<object, object, T>), method);
            _hash = (target.GetHashCode() * 397) ^ Method.GetHashCode();
        }

        #endregion

        #region Equality members

        public bool Equals(ISubscriber other)
        {
            return ActionSubscriber<object>.ActionSubscriberEquals(this, other);
        }

        public override bool Equals(object obj)
        {
            return ActionSubscriber<object>.ActionSubscriberEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion

        #region Implementation of ISubscriber

        public bool IsAlive
        {
            get { return _reference.Target != null; }
        }

        public bool AllowDuplicate
        {
            get { return true; }
        }

        public object Target
        {
            get { return _reference.Target; }
        }

        public HandlerResult Handle(object sender, object message)
        {
            object target = _reference.Target;
            if (target == null)
                return HandlerResult.Invalid;
            if (message is T)
            {
                _delegate(target, sender, (T)message);
                return HandlerResult.Handled;
            }
            return HandlerResult.Ignored;
        }

        #endregion
    }
}