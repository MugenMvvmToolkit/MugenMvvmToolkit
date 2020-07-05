using Java.Lang;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Observation
{
    public sealed class AndroidViewMemberObserver : Object, IAndroidNativeMemberObserver
    {
        #region Fields

        private readonly AndroidViewMemberListenerCollection _listeners;

        #endregion

        #region Constructors

        private AndroidViewMemberObserver(IAndroidView view)
        {
            _listeners = new AndroidViewMemberListenerCollection(view);
        }

        #endregion

        #region Implementation of interfaces

        public void OnMemberChanged(Object target, string path, Object? state)
        {
            _listeners.Raise(target, state, path, null);
        }

        #endregion

        #region Methods

        public static ActionToken Add(IAndroidView view, IEventListener listener, string memberName)
        {
            if (!(view.MemberObserver is AndroidViewMemberObserver memberObserver))
            {
                memberObserver = new AndroidViewMemberObserver(view);
                view.MemberObserver = memberObserver;
            }

            return memberObserver._listeners.Add(listener, memberName);
        }

        #endregion

        #region Nested types

        private sealed class AndroidViewMemberListenerCollection : MemberListenerCollection
        {
            #region Fields

            private readonly IAndroidView _view;

            #endregion

            #region Constructors

            public AndroidViewMemberListenerCollection(IAndroidView view)
            {
                _view = view;
            }

            #endregion

            #region Methods

            protected override void OnListenerAdded(string memberName)
            {
                _view.AddMemberListener(memberName);
            }

            protected override void OnListenerRemoved(string memberName)
            {
                _view.RemoveMemberListener(memberName);
            }

            #endregion
        }

        #endregion
    }
}