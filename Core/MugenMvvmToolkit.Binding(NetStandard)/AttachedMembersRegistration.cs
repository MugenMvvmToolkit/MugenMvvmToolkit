#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRegistration.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Diagnostics;
using System.Linq;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding
{
    public static class AttachedMembersRegistration
    {
        #region Nested types

        private struct ParentValue
        {
            #region Fields

            private readonly object _attachedParent;
            private readonly IBindingMemberInfo _parentMember;
            private readonly bool _isExplicit;

            #endregion

            #region Constructors

            public ParentValue(object attachedParent, IBindingMemberInfo parentMember, bool isExplicit)
            {
                _attachedParent = attachedParent;
                _parentMember = parentMember;
                _isExplicit = isExplicit;
            }

            #endregion

            #region Methods

            public ParentValue UpdateAttachedParent(object source, object[] args)
            {
                var attachedParent = args[0];
                if (_isExplicit)
                    if (_parentMember != null && _parentMember.CanWrite &&
                        (attachedParent == null || _parentMember.Type.IsInstanceOfType(attachedParent)))
                        _parentMember.SetValue(source, args);
                return new ParentValue(attachedParent, _parentMember, _isExplicit);
            }

            public object GetParent(object source)
            {
                if (_parentMember == null)
                    return _attachedParent;
                return _parentMember.GetValue(source, Empty.Array<object>()) ?? _attachedParent;
            }

            #endregion
        }

        private sealed class ExplicitParentListener : IEventListener
        {
            #region Fields

            private static readonly ExplicitParentListener Instance;

            #endregion

            #region Constructors

            static ExplicitParentListener()
            {
                Instance = new ExplicitParentListener();
            }

            private ExplicitParentListener()
            {
            }

            #endregion

            #region Methods

            public static bool SetParentValue(object o)
            {
                var isExplicit = true;
                var member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(o.GetType(), AttachedMemberConstants.ParentExplicit, false, false);
                if (member == null)
                {
                    member = BindingServiceProvider
                        .MemberProvider
                        .GetBindingMember(o.GetType(), AttachedMemberConstants.Parent, true, false);
                    isExplicit = false;
                }
                if (member == null)
                    return false;
                AttachedParentMember.SetValue(o, new ParentValue(null, member, isExplicit));
                member.TryObserve(o, Instance);
                return true;
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive => true;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                AttachedParentMember.Raise(sender, message);
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ErrorsKey = "@!err";

        private static readonly HashSet<Type> ExplicitParentTypes;
        private static readonly bool DebbugerAttached;
        private static readonly IAttachedBindingMemberInfo<object, object> CommandParameterInternal;
        private static readonly INotifiableAttachedBindingMemberInfo<object, ParentValue> AttachedParentMember;

        #endregion

        #region Constructors

        static AttachedMembersRegistration()
        {
            DebbugerAttached = Debugger.IsAttached;
            ExplicitParentTypes = new HashSet<Type>();
            CommandParameterInternal = AttachedBindingMember.CreateAutoProperty<object, object>("~#@cmdparam");
            AttachedParentMember = AttachedBindingMember.CreateAutoProperty<object, ParentValue>("#" + AttachedMemberConstants.Parent);
        }

        #endregion

        #region Methods

        public static void RegisterDefaultMembers()
        {
            var memberProvider = BindingServiceProvider.MemberProvider;
            var registration = new DefaultAttachedMemberRegistration<object>(CommandParameterInternal, AttachedMemberConstants.CommandParameter);
            memberProvider.Register(registration.ToAttachedBindingMember<object>());
            var parentMember = AttachedBindingMember.CreateMember(AttachedMembersBase.Object.Parent, GetParent, SetParent, ObserveParent, ParentAttached);
            AttachedBindingMember.TrySetRaiseAction(parentMember, (info, o, arg3) => AttachedParentMember.Raise(o, arg3));
            memberProvider.Register(parentMember);
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembersBase.Object.Root, GetRootMember, null, ObserveRootMember));

            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembersBase.Object.Errors, GetErrors, SetErrors));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembersBase.Object.HasErrors, GetHasErrors, null, ObserveHasErrors));

            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersBase.Object.IsFlatTree));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersBase.Object.IsFlatContext));

            memberProvider.Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.AsErrorsSource, (info, o) => BindingConstants.ErrorsSourceValue, null));
        }

        private static IDisposable ObserveParent(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return AttachedParentMember.TryObserve(o, arg3);
        }

        private static object SetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            var value = AttachedParentMember.GetValue(o, null);
            return AttachedParentMember.SetValue(o, value.UpdateAttachedParent(o, arg3));
        }

        private static object GetParent(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            return AttachedParentMember.GetValue(o, arg3).GetParent(o);
        }

        private static void ParentAttached(object o, MemberAttachedEventArgs args)
        {
            if (!ExplicitParentListener.SetParentValue(o) && DebbugerAttached)
                lock (ExplicitParentTypes)
                {
                    var type = o.GetType();
                    if (!ExplicitParentTypes.Contains(type))
                    {
                        Tracer.Warn(
                            @"Could not find a 'Parent' property on type '{0}', you should register it, without this the data bindings may not work properly. You can ignore this message if you are using the attached parent member.",
                            type);
                        ExplicitParentTypes.Add(type);
                    }
                }
        }

        private static IDisposable ObserveRootMember(IBindingMemberInfo member, object o, IEventListener arg3)
        {
            return BindingServiceProvider.VisualTreeManager.GetRootMember(o.GetType())?.TryObserve(o, arg3);
        }

        private static object GetRootMember(IBindingMemberInfo member, object o, object[] arg3)
        {
            return BindingServiceProvider.VisualTreeManager.GetRootMember(o.GetType())?.GetValue(o, arg3);
        }

        private static bool SetErrors(IBindingMemberInfo bindingMemberInfo, object o, IEnumerable<object> errors)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return false;
            var errorsList = errors as IList<object>;
            if (errorsList == null)
                errorsList = errors == null ? Empty.Array<object>() : errors.ToArray();
            errorProvider.SetErrors(o, ErrorsKey, errorsList, DataContext.Empty);
            return true;
        }

        private static IEnumerable<object> GetErrors(IBindingMemberInfo bindingMemberInfo, object o)
        {
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return Empty.Array<object>();
            return errorProvider.GetErrors(o, string.Empty, DataContext.Empty);
        }

        private static bool GetHasErrors(IBindingMemberInfo bindingMemberInfo, object o, object[] arg3)
        {
            var member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(o.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false);
            if (member == null)
                return false;
            var value = member.GetValue(o, arg3) as ICollection<object>;
            return value != null && value.Count != 0;
        }

        private static IDisposable ObserveHasErrors(IBindingMemberInfo bindingMemberInfo, object o, IEventListener arg3)
        {
            return BindingServiceProvider
                .MemberProvider
                .GetBindingMember(o.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false)
                ?.TryObserve(o, arg3);
        }

        #endregion
    }
}