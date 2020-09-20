using System;
using System.Collections.Generic;
using Java.Lang;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace MugenMvvm.Android.Observation
{
    public sealed class ViewMemberChangedListener : Object, INativeMemberChangedListener
    {
        #region Fields

        private readonly AndroidViewMemberListenerCollection _listeners;

        public const string ParentMemberName = "Parent";
        public const string ParentEventName = "ParentChanged";
        public const string ClickEventName = "Click";
        public const string LongClickEventName = "LongClick";
        public const string TextMemberName = "Text";
        public const string TextEventName = "TextChanged";
        public const string HomeButtonClick = "HomeButtonClick";
        public const string RefreshedEventName = "Refreshed";
        public const string SelectedIndexName = "SelectedIndex";
        public const string SelectedIndexEventName = "SelectedIndexChanged";

        public static readonly ICharSequence ParentMemberNameNative = ViewExtensions.ParentMemberName;
        public static readonly ICharSequence ParentEventNameNative = ViewExtensions.ParentEventName;
        public static readonly ICharSequence ClickEventNameNative = ViewExtensions.ClickEventName;
        public static readonly ICharSequence LongClickEventNameNative = ViewExtensions.LongClickEventName;
        public static readonly ICharSequence TextMemberNameNative = ViewExtensions.TextMemberName;
        public static readonly ICharSequence TextEventNameNative = ViewExtensions.TextEventName;
        public static readonly ICharSequence HomeButtonClickNative = ViewExtensions.HomeButtonClick;
        public static readonly ICharSequence RefreshedEventNameNative = ViewExtensions.RefreshedEventName;
        public static readonly ICharSequence SelectedIndexNameNative = ViewExtensions.SelectedIndexName;
        public static readonly ICharSequence SelectedIndexEventNameNative = ViewExtensions.SelectedIndexEventName;

        private static readonly Dictionary<string, ICharSequence> NetToJavaMapping = new Dictionary<string, ICharSequence>(3);
        private static readonly Dictionary<ICharSequence, string> JavaToNetMapping = new Dictionary<ICharSequence, string>(3);

        #endregion

        #region Constructors

        private ViewMemberChangedListener(Object view)
        {
            _listeners = new AndroidViewMemberListenerCollection(view);
        }

        #endregion

        #region Implementation of interfaces

        public void OnChanged(Object target, ICharSequence path, Object? state) => _listeners.Raise(target, state, GetMember(path), null);

        #endregion

        #region Methods

        public static ActionToken Add(Object target, IEventListener listener, string memberName)
        {
            if (!(ViewExtensions.GetMemberChangedListener(target) is ViewMemberChangedListener memberObserver))
            {
                memberObserver = new ViewMemberChangedListener(target);
                ViewExtensions.SetMemberChangedListener(target, memberObserver);
            }

            return memberObserver._listeners.Add(listener, memberName);
        }

        private static string GetMember(ICharSequence member)
        {
            if (member == ParentMemberNameNative)
                return ParentMemberName;
            if (member == ParentEventNameNative)
                return ParentEventName;
            if (member == ClickEventNameNative)
                return ClickEventName;
            if (member == LongClickEventNameNative)
                return LongClickEventName;
            if (member == TextMemberNameNative)
                return TextMemberName;
            if (member == TextEventNameNative)
                return TextEventName;
            if (member == HomeButtonClickNative)
                return HomeButtonClick;
            if (member == RefreshedEventNameNative)
                return RefreshedEventName;
            if (member == SelectedIndexNameNative)
                return SelectedIndexName;
            if (member == SelectedIndexEventNameNative)
                return SelectedIndexEventName;
            if (!JavaToNetMapping.TryGetValue(member, out var r))
            {
                r = member.ToString();
                JavaToNetMapping[member] = r;
            }

            return r;
        }

        private static ICharSequence GetMember(string member)
        {
            switch (member)
            {
                case ParentMemberName:
                    return ParentMemberNameNative;
                case ParentEventName:
                    return ParentEventNameNative;
                case ClickEventName:
                    return ClickEventNameNative;
                case LongClickEventName:
                    return LongClickEventNameNative;
                case TextMemberName:
                    return TextMemberNameNative;
                case TextEventName:
                    return TextEventNameNative;
                case HomeButtonClick:
                    return HomeButtonClickNative;
                case RefreshedEventName:
                    return RefreshedEventNameNative;
                case SelectedIndexName:
                    return SelectedIndexNameNative;
                case SelectedIndexEventName:
                    return SelectedIndexEventNameNative;
            }

            if (!NetToJavaMapping.TryGetValue(member, out var r))
            {
                r = new String(member);
                NetToJavaMapping[member] = r;
            }

            return r;
        }

        #endregion

        #region Nested types

        private sealed class AndroidViewMemberListenerCollection : MemberListenerCollection
        {
            #region Fields

            private readonly Object _view;

            #endregion

            #region Constructors

            public AndroidViewMemberListenerCollection(Object view)
            {
                _view = view;
            }

            #endregion

            #region Methods

            protected override void OnListenerAdded(string memberName)
            {
                if (_view.Handle == IntPtr.Zero)
                    return;
                if (!ViewExtensions.AddMemberListener(_view, GetMember(memberName)))
                    BindingExceptionManager.ThrowInvalidBindingMember(_view, memberName);
            }

            protected override void OnListenerRemoved(string memberName)
            {
                if (_view.Handle != IntPtr.Zero)
                    ViewExtensions.RemoveMemberListener(_view, GetMember(memberName));
            }

            #endregion
        }

        #endregion
    }
}