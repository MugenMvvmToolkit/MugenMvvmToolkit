using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Observation
{
    public sealed class AndroidViewMemberChangedListener : Object, IAndroidNativeMemberChangedListener
    {
        #region Fields

        private readonly AndroidViewMemberListenerCollection _listeners;

        #endregion

        #region Constructors

        private AndroidViewMemberChangedListener(View view)
        {
            _listeners = new AndroidViewMemberListenerCollection(view);
        }

        #endregion

        #region Implementation of interfaces

        public void OnChanged(Object target, string path, Object? state)
        {
            _listeners.Raise(target, state, path, null);
        }

        #endregion

        #region Methods

        public static ActionToken Add(View view, IEventListener listener, string memberName)
        {
            if (!(ViewExtensions.GetMemberChangedListener(view) is AndroidViewMemberChangedListener memberObserver))
            {
                memberObserver = new AndroidViewMemberChangedListener(view);
                ViewExtensions.SetMemberChangedListener(view, memberObserver);
            }

            return memberObserver._listeners.Add(listener, memberName);
        }

        #endregion

        #region Nested types

        private sealed class AndroidViewMemberListenerCollection : MemberListenerCollection
        {
            #region Fields

            private readonly View _view;

            #endregion

            #region Constructors

            public AndroidViewMemberListenerCollection(View view)
            {
                _view = view;
            }

            #endregion

            #region Methods

            protected override void OnListenerAdded(string memberName)
            {
                if (!ViewExtensions.AddMemberListener(_view, memberName))
                    BindingExceptionManager.ThrowInvalidBindingMember(_view, memberName);
            }

            protected override void OnListenerRemoved(string memberName)
            {
                ViewExtensions.RemoveMemberListener(_view, memberName);
            }

            #endregion
        }

        #endregion
    }
}