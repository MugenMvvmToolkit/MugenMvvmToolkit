#region Copyright

// ****************************************************************************
// <copyright file="HandlerSubscriber.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Models
{
    internal sealed class HandlerSubscriber : ISubscriber
    {
        #region Nested types

        private struct HandlerMessageKey
        {
            #region Fields

            public Type HandlerType;
            public Type MessageType;

            #endregion

            #region Constructors

            public HandlerMessageKey(Type handlerType, Type messageType)
            {
                HandlerType = handlerType;
                MessageType = messageType;
            }

            #endregion
        }

        private sealed class HandlerMessageKeyComparer : IEqualityComparer<HandlerMessageKey>
        {
            #region Methods

            public bool Equals(HandlerMessageKey x, HandlerMessageKey y)
            {
                return x.HandlerType.Equals(y.HandlerType) && x.MessageType.Equals(y.MessageType);
            }

            public int GetHashCode(HandlerMessageKey obj)
            {
                unchecked
                {
                    return (obj.HandlerType.GetHashCode() * 397) ^ obj.MessageType.GetHashCode();
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Dictionary<Type, Type[]> TypeToHandlerInterfaces;
        private static readonly Dictionary<HandlerMessageKey, Func<object, object[], object>[]> TypeToHandlers;

        private readonly int _hashCode;
        private readonly WeakReference _reference;
        private readonly bool _isViewModelHandler;

        #endregion

        #region Constructors

        static HandlerSubscriber()
        {
            TypeToHandlerInterfaces = new Dictionary<Type, Type[]>();
            TypeToHandlers = new Dictionary<HandlerMessageKey, Func<object, object[], object>[]>(new HandlerMessageKeyComparer());
        }

        private HandlerSubscriber(object target, bool isViewModelHandler)
        {
            _hashCode = RuntimeHelpers.GetHashCode(target);
            _reference = ToolkitExtensions.GetWeakReference(target);
            _isViewModelHandler = isViewModelHandler;
        }

        #endregion

        #region Implementation of ISubscriber

        public bool IsAlive
        {
            get { return _reference.Target != null; }
        }

        public bool AllowDuplicate
        {
            get { return false; }
        }

        public object Target
        {
            get { return _reference.Target; }
        }

        public HandlerResult Handle(object sender, object message)
        {
            var target = _reference.Target;
            if (target == null)
                return HandlerResult.Invalid;
            if (_isViewModelHandler)
                ((IHandler<object>)target).Handle(sender, message);
            else
            {
                var handlers = GetHandlers(target, message);
                if (handlers.Length == 0)
                    return HandlerResult.Ignored;
                var args = new[] { sender, message };
                for (int index = 0; index < handlers.Length; index++)
                    handlers[index].Invoke(target, args);
            }
            return HandlerResult.Handled;
        }

        #endregion

        #region Methods

        public static HandlerSubscriber Get(object target)
        {
            var type = target.GetType();
            Type[] interfaces;
            lock (TypeToHandlerInterfaces)
            {
                if (!TypeToHandlerInterfaces.TryGetValue(type, out interfaces))
                {
                    interfaces = GetHandlerInterfaces(type);
                    TypeToHandlerInterfaces[type] = interfaces;
                }
            }
            if (interfaces.Length == 0)
                return null;
            return new HandlerSubscriber(target, interfaces.Length == 1 && target is ViewModelBase);
        }

        private static Func<object, object[], object>[] GetHandlers(object handler, object message)
        {
            var key = new HandlerMessageKey(handler.GetType(), message.GetType());
            lock (TypeToHandlers)
            {
                Func<object, object[], object>[] value;
                if (!TypeToHandlers.TryGetValue(key, out value))
                {
                    var items = new List<Func<object, object[], object>>();
                    var interfaces = GetHandlerInterfaces(key.HandlerType);
                    for (int index = 0; index < interfaces.Length; index++)
                    {
                        Type @interface = interfaces[index];
                        Type typeMessage = @interface.GetGenericArguments()[0];
                        MethodInfo method = @interface.GetMethodEx("Handle");
                        if (typeMessage.IsAssignableFrom(key.MessageType))
                            items.Add(ServiceProvider.ReflectionManager.GetMethodDelegate(method));
                    }
                    value = items.ToArray();
                    TypeToHandlers[key] = value;
                }
                return value;
            }
        }

        private static Type[] GetHandlerInterfaces(Type type)
        {
            return type.GetInterfaces()
#if PCL_WINRT
.Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IHandler<>)))
#else
.Where(x => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IHandler<>)))
#endif
.ToArray();
        }

        #endregion

        #region Equality members

        public bool Equals(ISubscriber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ReferenceEquals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HandlerSubscriber)obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        #endregion
    }
}
