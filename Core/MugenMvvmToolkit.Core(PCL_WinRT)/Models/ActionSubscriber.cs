#region Copyright

// ****************************************************************************
// <copyright file="ActionSubscriber.cs">
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
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    internal interface IActionSubscriber : ISubscriber
    {
        MethodInfo Method { get; }
    }

    internal sealed class ActionSubscriber<T> : IActionSubscriber
    {
        #region Fields

        private readonly Action<object, T> _action;
        private readonly int _hash;

        #endregion

        #region Constructors

        public ActionSubscriber(Action<object, T> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
            _hash = ActionSubscriberGetHashCode(action.Target, action.GetMethodInfo());
        }

        #endregion

        #region Implementation of ISubscriber

        public bool IsAlive => true;

        public bool AllowDuplicate => true;

        public object Target => _action.Target ?? Method;

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

        public static bool ActionSubscriberEquals(IActionSubscriber x, IActionSubscriber y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            return ReferenceEquals(x.Target, y.Target) && Equals(x.Method, y.Method);
        }

        public static int ActionSubscriberGetHashCode(object target, MethodInfo method)
        {
            return (target == null ? 0 : RuntimeHelpers.GetHashCode(target) * 397) ^ method.GetHashCode();
        }

        public bool Equals(ISubscriber other)
        {
            return ActionSubscriberEquals(this, other as IActionSubscriber);
        }

        public override bool Equals(object obj)
        {
            return ActionSubscriberEquals(this, obj as IActionSubscriber);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion

        #region Implementation of IActionSubscriber

        public MethodInfo Method => _action.GetMethodInfo();

        #endregion
    }
}
