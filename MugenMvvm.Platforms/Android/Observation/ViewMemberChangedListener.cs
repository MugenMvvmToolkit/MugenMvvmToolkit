using System;
using System.Collections.Generic;
using Java.Lang;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace MugenMvvm.Android.Observation
{
    public sealed class ViewMemberChangedListener : Object, INativeMemberChangedListener
    {
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
        public const string CheckedMemberName = "Checked";
        public const string CheckedEventName = "CheckedChanged";

        public static readonly ICharSequence ParentMemberNameNative = NativeBindableMemberMugenExtensions.ParentMemberName;
        public static readonly ICharSequence ParentEventNameNative = NativeBindableMemberMugenExtensions.ParentEventName;
        public static readonly ICharSequence ClickEventNameNative = NativeBindableMemberMugenExtensions.ClickEventName;
        public static readonly ICharSequence LongClickEventNameNative = NativeBindableMemberMugenExtensions.LongClickEventName;
        public static readonly ICharSequence TextMemberNameNative = NativeBindableMemberMugenExtensions.TextMemberName;
        public static readonly ICharSequence TextEventNameNative = NativeBindableMemberMugenExtensions.TextEventName;
        public static readonly ICharSequence HomeButtonClickNative = NativeBindableMemberMugenExtensions.HomeButtonClick;
        public static readonly ICharSequence RefreshedEventNameNative = NativeBindableMemberMugenExtensions.RefreshedEventName;
        public static readonly ICharSequence SelectedIndexNameNative = NativeBindableMemberMugenExtensions.SelectedIndexName;
        public static readonly ICharSequence SelectedIndexEventNameNative = NativeBindableMemberMugenExtensions.SelectedIndexEventName;
        public static readonly ICharSequence CheckedMemberNameNative = NativeBindableMemberMugenExtensions.CheckedMemberName;
        public static readonly ICharSequence CheckedEventNameNative = NativeBindableMemberMugenExtensions.CheckedEventName;

        private static readonly Dictionary<string, ICharSequence> NetToJavaMapping = new(3);
        private static readonly Dictionary<ICharSequence, string> JavaToNetMapping = new(3);

        private readonly AndroidViewMemberListenerCollection _listeners;

        private ViewMemberChangedListener(Object view)
        {
            _listeners = new AndroidViewMemberListenerCollection(view);
        }

        public static ActionToken Add(Object target, IEventListener listener, string memberName)
        {
            if (NativeBindableMemberMugenExtensions.GetMemberChangedListener(target) is not ViewMemberChangedListener memberObserver)
            {
                memberObserver = new ViewMemberChangedListener(target);
                NativeBindableMemberMugenExtensions.SetMemberChangedListener(target, memberObserver);
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
            if (member == CheckedMemberNameNative)
                return CheckedMemberName;
            if (member == CheckedEventNameNative)
                return CheckedEventName;
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
                case CheckedMemberName:
                    return CheckedMemberNameNative;
                case CheckedEventName:
                    return CheckedEventNameNative;
            }

            if (!NetToJavaMapping.TryGetValue(member, out var r))
            {
                r = new String(member);
                NetToJavaMapping[member] = r;
            }

            return r;
        }

        public void OnChanged(Object target, ICharSequence path, Object? state) => _listeners.Raise(target, state, GetMember(path), null);

        private sealed class AndroidViewMemberListenerCollection : MemberListenerCollection
        {
            private readonly Object _view;

            public AndroidViewMemberListenerCollection(Object view)
            {
                _view = view;
            }

            protected override void OnListenerAdded(string memberName)
            {
                if (_view.Handle == IntPtr.Zero)
                    return;
                if (!NativeBindableMemberMugenExtensions.AddMemberListener(_view, GetMember(memberName)))
                    ExceptionManager.ThrowInvalidBindingMember(_view, memberName);
            }

            protected override void OnListenerRemoved(string memberName)
            {
                if (_view.Handle != IntPtr.Zero)
                    NativeBindableMemberMugenExtensions.RemoveMemberListener(_view, GetMember(memberName));
            }
        }
    }
}