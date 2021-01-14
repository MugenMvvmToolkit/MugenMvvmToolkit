using System;
using Android.Runtime;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Bindings
{
    public sealed class MenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
    {
        private readonly EventListenerCollection? _listeners;
        private readonly IMenuItem? _menuItem;

        private MenuItemClickListener(IMenuItem menuItem)
        {
            Should.NotBeNull(menuItem, nameof(menuItem));
            _menuItem = menuItem;
            _listeners = new EventListenerCollection();
        }

        private MenuItemClickListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public static ActionToken AddListener(IMenuItem menuItem, IEventListener listener) =>
            menuItem.AttachedValues().GetOrAdd(AndroidInternalConstant.MenuClickListener, menuItem, (key, item) =>
            {
                var l = new MenuItemClickListener(item);
                item.SetOnMenuItemClickListener(l);
                return l;
            }).AddListener(listener);

        public bool OnMenuItemClick(IMenuItem? item)
        {
            var menuItem = _menuItem;
            if (menuItem == null || _listeners == null)
                return false;
            _listeners.Raise(menuItem, EventArgs.Empty, null);
            return true;
        }

        private ActionToken AddListener(IEventListener listener)
        {
            if (_listeners == null)
                return default;
            return _listeners.Add(listener);
        }
    }
}