#region Copyright

// ****************************************************************************
// <copyright file="VisualTreeManager.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class VisualTreeManager : IVisualTreeManager
    {
        #region Netsted types

        private sealed class RootListener : EventListenerList, IEventListener
        {
            #region Fields

            private readonly WeakReference _target;
            private WeakReference _parent;

            #endregion

            #region Constructors

            public RootListener(object target)
            {
                _target = ServiceProvider.WeakReferenceFactory(target);
                _parent = Empty.WeakReference;
                UpdateParent(target);
                BindingServiceProvider.VisualTreeManager.GetParentMember(target.GetType())?.TryObserve(target, this);
            }

            #endregion

            #region Methods

            public static RootListener GetOrAdd(object target)
            {
                return ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(target, "_#@rtls#@_", (o, o1) => new RootListener(o), null);
            }

            public object GetRoot()
            {
                var parent = _parent.Target;
                if (parent == null)
                    return _target.Target;
                return GetOrAdd(parent).GetRoot();
            }

            private void UpdateParent(object target)
            {
                var oldParent = _parent.Target;
                var parent = BindingServiceProvider.VisualTreeManager.FindParent(target);
                if (oldParent != parent)
                {
                    if (oldParent != null)
                        GetOrAdd(oldParent).Remove(this);
                    if (parent != null)
                        GetOrAdd(parent).Add(this);
                    _parent = ServiceProvider.WeakReferenceFactory(parent);
                }
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive => _target.Target != null;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                var target = _target.Target;
                if (target == null)
                    return false;
                UpdateParent(target);
                Raise(sender, message);
                return true;
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

        #region Implementation of IVisualTreeManager

        public virtual IBindingMemberInfo GetRootMember(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(type, AttachedMemberConstants.RootElement, false, false) ?? RootMember;
        }

        public virtual IBindingMemberInfo GetParentMember(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return BindingServiceProvider.MemberProvider.GetBindingMember(type, AttachedMemberConstants.Parent, false, false);
        }

        public virtual object FindParent(object target)
        {
            Should.NotBeNull(target, nameof(target));
            Type type = target.GetType();
            IBindingMemberInfo parentProp = GetParentMember(type);
            return parentProp == null ? null : parentProp.GetValue(target, null);
        }

        public virtual object FindByName(object target, string elementName)
        {
            Should.NotBeNull(elementName, nameof(elementName));
            while (target != null)
            {
                var result = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(target.GetType(), AttachedMemberConstants.FindByNameMethod, false, false)
                    ?.GetValue(target, new object[] { elementName });
                if (result != null)
                    return result;
                target = FindParent(target);
            }
            return null;
        }

        public virtual object FindRelativeSource(object target, string typeName, uint level)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
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
            return RootListener.GetOrAdd(o).AddWithUnsubscriber(arg3);
        }

        private static object GetRootElement(IBindingMemberInfo bindingMemberInfo, object currentItem)
        {
            return RootListener.GetOrAdd(currentItem).GetRoot();
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
                if (!fullNameEqual && (type.FullName == typeName || type.AssemblyQualifiedName == typeName))
                {
                    fullNameEqual = true;
                    if (shortNameEqual)
                        break;
                }
#if NET_STANDARD
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif
            }
        }

        #endregion
    }
}
