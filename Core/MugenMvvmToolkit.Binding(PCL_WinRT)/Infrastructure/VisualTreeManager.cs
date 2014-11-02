#region Copyright
// ****************************************************************************
// <copyright file="VisualTreeManager.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Reflection;
using System.Threading;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the visual tree manager.
    /// </summary>
    public class VisualTreeManager : IVisualTreeManager
    {
        #region Netsted types

        private sealed class RootObserver : List<IDisposable>, IDisposable, IEventListener
        {
            #region Fields

            private bool _updating;
            private readonly WeakReference _reference;
            private WeakEventListenerWrapper _listener;

            #endregion

            #region Constructors

            public RootObserver(object target, IEventListener listener)
            {
                _reference = ServiceProvider.WeakReferenceFactory(target, true);
                _listener = listener.ToWeakWrapper();
                Handle(null, null);
            }

            #endregion

            #region Implementation of interfaces

            public bool IsAlive
            {
                get { return _reference.Target != null && _listener.EventListener.IsAlive; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public void Handle(object sender, object message)
            {
                TryHandle(sender, message);
            }

            public bool TryHandle(object sender, object message)
            {
                if (_updating)
                    return true;
                if (_listener.IsEmpty)
                    return false;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_listener.IsEmpty)
                        return false;
                    _updating = true;
                    object currentItem = _reference.Target;
                    if (currentItem == null)
                    {
                        Dispose(true);
                        return false;
                    }

                    if (!_listener.EventListener.TryHandle(currentItem, message))
                    {
                        Dispose(true);
                        return false;
                    }

                    Dispose(false);
                    var treeManager = BindingServiceProvider.VisualTreeManager;
                    while (currentItem != null)
                    {
                        var parentMember = treeManager.GetParentMember(currentItem.GetType());
                        if (parentMember == null)
                            break;
                        var observer = parentMember.TryObserve(currentItem, this);
                        if (observer != null)
                            Add(observer);
                        currentItem = parentMember.GetValue(currentItem, null);
                    }
                    return true;
                }
                finally
                {
                    _updating = false;
                    if (lockTaken)
                        Monitor.Exit(this);
                }
            }

            public void Dispose()
            {
                lock (this)
                    Dispose(true);
            }

            #endregion

            #region Methods

            private void Dispose(bool dispose)
            {
                if (_listener.IsEmpty)
                    return;
                if (dispose)
                    _listener = WeakEventListenerWrapper.Empty;
                for (int i = 0; i < Count; i++)
                    this[i].Dispose();
                Clear();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly IAttachedBindingMemberInfo<object, object> RootMember;

        #endregion

        #region Constructors

        static VisualTreeManager()
        {
            RootMember = AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.RootElement, GetRootElement, null, ObserveRootElement);
        }

        #endregion

        #region Implementation of ITargetTreeManager

        /// <summary>
        ///     Gets the root member, if any.
        /// </summary>
        public virtual IBindingMemberInfo GetRootMember(Type type)
        {
            if (GetParentMember(type) == null)
                return null;
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(type, AttachedMemberConstants.RootElement, false, false) ?? RootMember;
        }

        /// <summary>
        ///     Gets the parent member, if any.
        /// </summary>
        public virtual IBindingMemberInfo GetParentMember(Type type)
        {
            Should.NotBeNull(type, "type");
            return BindingServiceProvider.MemberProvider.GetBindingMember(type, AttachedMemberConstants.Parent, false, false);
        }

        /// <summary>
        ///     Tries to find parent.
        /// </summary>
        public virtual object FindParent(object target)
        {
            Should.NotBeNull(target, "target");
            Type type = target.GetType();
            IBindingMemberInfo parentProp = GetParentMember(type);
            return parentProp == null ? null : parentProp.GetValue(target, null);
        }

        /// <summary>
        ///     Tries to find element by it's name.
        /// </summary>
        public virtual object FindByName(object target, string elementName)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(elementName, "elementName");
            var member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), AttachedMemberConstants.FindByNameMethod, false, false);
            if (member == null)
                return null;
            return member.GetValue(target, new object[] { elementName });
        }

        /// <summary>
        ///     Tries to find relative source.
        /// </summary>
        public virtual object FindRelativeSource(object target, string typeName, uint level)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(typeName, "typeName");
            object fullNameSource = null;
            object nameSource = null;
            uint fullNameLevel = 0;
            uint nameLevel = 0;

            target = FindParent(target);
            while (target != null)
            {
                bool shortNameEqual;
                bool fullNameEqual;
                TypeNameEqual(target.GetType(), typeName, out shortNameEqual, out fullNameEqual);
                if (shortNameEqual)
                {
                    nameSource = target;
                    nameLevel++;
                }
                if (fullNameEqual)
                {
                    fullNameSource = target;
                    fullNameLevel++;
                }

                if (fullNameSource != null && fullNameLevel == level)
                    return fullNameSource;
                if (nameSource != null && nameLevel == level)
                    return nameSource;

                target = FindParent(target);
            }
            return null;
        }

        #endregion

        #region Methods

        private static IDisposable ObserveRootElement(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return new RootObserver(o, arg3);
        }

        private static object GetRootElement(IBindingMemberInfo bindingMemberInfo, object currentItem)
        {
            var treeManager = BindingServiceProvider.VisualTreeManager;
            while (currentItem != null)
            {
                var parentMember = treeManager.GetParentMember(currentItem.GetType());
                if (parentMember == null)
                    return currentItem;
                var next = parentMember.GetValue(currentItem, null);
                if (next == null)
                    return currentItem;
                currentItem = next;
            }
            return null;
        }

        private static void TypeNameEqual(Type type, string typeName, out bool shortNameEqual, out bool fullNameEqual)
        {
            shortNameEqual = false;
            fullNameEqual = false;
            while (type != null)
            {
                if (!shortNameEqual)
                {
                    if (type.Name == typeName)
                    {
                        shortNameEqual = true;
                        if (fullNameEqual)
                            break;
                    }
                }
                if (!fullNameEqual && type.FullName == typeName)
                {
                    fullNameEqual = true;
                    if (shortNameEqual)
                        break;
                }
#if PCL_WINRT
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif
            }
        }

        #endregion
    }
}