using System;
using Android.Runtime;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Binding
{
    public sealed class MenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
    {
        #region Fields

        private readonly EventListenerCollection? _listeners;
        private readonly IMenuItem? _menuItem;

        #endregion

        #region Constructors

        private MenuItemClickListener(IMenuItem menuItem)
        {
            Should.NotBeNull(menuItem, nameof(menuItem));
            _menuItem = menuItem;
            _listeners = new EventListenerCollection();
        }

        private MenuItemClickListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool OnMenuItemClick(IMenuItem item)
        {
            var menuItem = _menuItem;
            if (menuItem == null || _listeners == null)
                return false;
            _listeners.Raise(menuItem, EventArgs.Empty, null);
            return true;
        }

        #endregion

        #region Methods

        public static ActionToken AddListener(IMenuItem menuItem, IEventListener listener)
        {
            return MugenService
                .AttachedValueManager
                .GetOrAdd(menuItem, AndroidInternalConstant.MenuClickListener, menuItem, (item, _) =>
                {
                    var l = new MenuItemClickListener(item);
                    item.SetOnMenuItemClickListener(l);
                    return l;
                }).AddListener(listener);
        }

        private ActionToken AddListener(IEventListener listener)
        {
            if (_listeners == null)
                return default;
            return _listeners.Add(listener);
        }

        #endregion
    }
}