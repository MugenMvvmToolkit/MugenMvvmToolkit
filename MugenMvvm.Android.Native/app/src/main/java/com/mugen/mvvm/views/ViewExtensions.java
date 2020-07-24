package com.mugen.mvvm.views;

import android.content.Context;
import android.content.Intent;
import android.util.AttributeSet;
import android.util.SparseArray;
import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.interfaces.IViewFactory;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.internal.ViewFactory;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.views.listeners.ViewMemberChangedListener;

import java.util.ArrayList;
import java.util.HashMap;

public abstract class ViewExtensions {
    public static final String ParentMemberName = "Parent";
    public static final String ParentEventName = "ParentChanged";
    public static final String ClickEventName = "Click";

    private static IViewFactory _viewFactory;

    private final static SparseArray<Class> _resourceViewMapping = new SparseArray<>();
    private final static HashMap<Class, Integer> _viewResourceMapping = new HashMap<>();
    private final static ArrayList<IViewDispatcher> _viewDispatchers = new ArrayList<>();
    private static final ArrayList<IMemberListenerManager> ListenerManagers = new ArrayList<>();
    protected static final Object NullParent = "";

    public static void registerMemberListenerManager(IMemberListenerManager manager) {
        ListenerManagers.add(manager);
    }

    public static void registerMemberListenerManager(IMemberListenerManager manager, int index) {
        ListenerManagers.add(index, manager);
    }

    public static boolean unregisterMemberListenerManager(String memberName, IMemberListenerManager manager) {
        return ListenerManagers.remove(manager);
    }

    public static IMemberChangedListener getMemberChangedListener(View view) {
        ViewMemberChangedListener listener = (ViewMemberChangedListener) view.getTag(R.id.memberObserver);
        if (listener != null)
            return listener.getListener();
        return null;
    }

    public static void setMemberChangedListener(View view, IMemberChangedListener memberObserver) {
        ViewMemberChangedListener listener = (ViewMemberChangedListener) view.getTag(R.id.memberObserver);
        if (listener == null) {
            listener = new ViewMemberChangedListener();
            view.setTag(R.id.memberObserver, listener);
        }
        listener.setListener(memberObserver);
    }

    public static boolean addMemberListener(View view, String memberName) {
        if (ParentEventName.equals(memberName) || ParentMemberName.equals(memberName))
            return true;
        ViewMemberChangedListener listeners = (ViewMemberChangedListener) view.getTag(R.id.memberObserver);
        if (listeners != null) {
            IMemberListener memberListener = listeners.get(memberName);
            if (memberListener != null) {
                memberListener.addListener(view, memberName);
                return true;
            }
        }

        for (int i = 0; i < ListenerManagers.size(); i++) {
            IMemberListener memberListener = ListenerManagers.get(i).tryGetListener(listeners, view, memberName);
            if (memberListener != null) {
                if (listeners == null) {
                    listeners = new ViewMemberChangedListener();
                    view.setTag(R.id.memberObserver);
                }

                listeners.put(memberName, memberListener);
                memberListener.addListener(view, memberName);
                return true;
            }
        }
        return false;
    }

    public static boolean removeMemberListener(View view, String memberName) {
        ViewMemberChangedListener listeners = (ViewMemberChangedListener) view.getTag(R.id.memberObserver);
        if (listeners == null)
            return false;
        IMemberListener memberListener = listeners.get(memberName);
        if (memberListener == null)
            return false;
        memberListener.removeListener(view, memberName);
        return true;
    }

    public static void addParentObserver(View view) {
        ViewParentObserver.Instance.add(view);
    }

    public static void removeParentObserver(View view) {
        ViewParentObserver.Instance.remove(view, true);
    }

    public static void onMemberChanged(View view, String memberName, Object args) {
        ViewMemberChangedListener memberObserver = (ViewMemberChangedListener) view.getTag(R.id.memberObserver);
        if (memberObserver != null)
            memberObserver.onChanged(view, memberName, args);
    }

    public static Object getParent(View view) {
        return getParentRaw(view);
    }

    public static void setParent(View view, Object parent) {
        Object oldParent = getParentRaw(view);
        parent = ActivityExtensions.tryWrapActivity(parent);
        if (oldParent == parent)
            return;

        view.setTag(R.id.parent, parent);
        onMemberChanged(view, ParentMemberName, null);
    }

    public static Object findRelativeSource(View view, String name, int level) {
        int nameLevel = 0;
        Object target = getParentRaw(view);
        while (target != null) {
            if (typeNameEqual(target.getClass(), name) && ++nameLevel == level)
                return target;

            if (target instanceof View)
                target = getParentRaw((View) target);
            else
                target = null;
        }

        return null;
    }

    public static Object getAttachedValues(Object view) {
        if (view instanceof View)
            return ((View) view).getTag(R.id.attachedValues);
        return ((IActivityView) view).getTag(R.id.attachedValues);
    }

    public static void setAttachedValues(Object view, Object values) {
        if (view instanceof View)
            ((View) view).setTag(R.id.attachedValues, values);
        else
            ((IActivityView) view).setTag(R.id.attachedValues, values);
    }

    public static void addViewMapping(Class viewClass, int resourceId) {
        _resourceViewMapping.put(resourceId, viewClass);
        if (_viewResourceMapping.containsKey(viewClass))
            _viewResourceMapping.put(viewClass, 0);
        else
            _viewResourceMapping.put(viewClass, resourceId);
    }

    public static Class tryGetClassById(int resourceId) {
        return _resourceViewMapping.get(resourceId);
    }

    public static int tryGetViewId(Class viewClass, Intent intent, int defaultValue) {
        if (intent != null && intent.hasExtra(ActivityExtensions.ViewIdIntentKey))
            return intent.getIntExtra(ActivityExtensions.ViewIdIntentKey, defaultValue);
        if (viewClass == null)
            return defaultValue;
        Integer value = _viewResourceMapping.get(viewClass);
        if (value == null || value == 0)
            return defaultValue;
        return value;
    }

    public static Object getView(Object container, int resourceId, boolean trackLifecycle) {
        return getViewFactory().getView(container, resourceId, trackLifecycle);
    }

    public static IViewFactory getViewFactory() {
        if (_viewFactory == null)
            setViewFactory(new ViewFactory());
        return _viewFactory;
    }

    public static void setViewFactory(IViewFactory viewFactory) {
        if (_viewFactory instanceof ILifecycleDispatcher)
            LifecycleExtensions.removeLifecycleDispatcher((ILifecycleDispatcher) _viewFactory);
        _viewFactory = viewFactory;
        if (_viewFactory instanceof ILifecycleDispatcher)
            LifecycleExtensions.addLifecycleDispatcher((ILifecycleDispatcher) _viewFactory, false, 0);
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher) {
        _viewDispatchers.add(viewDispatcher);
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher, int index) {
        _viewDispatchers.add(index, viewDispatcher);
    }

    public static void removeViewDispatcher(IViewDispatcher viewDispatcher) {
        _viewDispatchers.remove(viewDispatcher);
    }

    public static void onParentChanged(View view) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onParentChanged(view);
        }
    }

    public static void onSettingView(Object owner, View view) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onSetting(owner, view);
        }
    }

    public static void onSetView(Object owner, View view) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onSet(owner, view);
        }
    }

    public static void onInflatingView(int resourceId, Context context) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onInflating(resourceId, context);
        }
    }

    public static void onInflatedView(View view, int resourceId, Context context) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onInflated(view, resourceId, context);
        }
    }

    public static View onViewCreated(View view, Context context, AttributeSet attrs) {
        if (view == null)
            return null;
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            view = _viewDispatchers.get(i).onCreated(view, context, attrs);
        }
        return view;
    }

    public static void onDestroyView(View view) {
        if (view == null)
            return;
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onDestroy(view);
        }
    }

    public static View tryCreateCustomView(View parent, String name, Context viewContext, AttributeSet attrs) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            View view = _viewDispatchers.get(i).tryCreate(parent, name, viewContext, attrs);
            if (view != null)
                return view;
        }
        return null;
    }

    private static Object getParentRaw(View view) {
        if (view.getId() == android.R.id.content)
            return ActivityExtensions.tryWrapActivity(ActivityExtensions.getActivity(view.getContext()));

        Object parent = view.getTag(R.id.parent);
        if (parent == NullParent)
            return null;

        if (parent == null)
            parent = view.getParent();
        return parent;
    }

    private static boolean typeNameEqual(Class clazz, String typeName) {
        while (clazz != null) {
            if (clazz.getSimpleName().equals(typeName))
                return true;
            clazz = clazz.getSuperclass();
        }
        return false;
    }

    public interface IMemberListenerManager {
        IMemberListener tryGetListener(HashMap<String, IMemberListener> listeners, View view, String memberName);
    }

    public interface IMemberListener {
        void addListener(View view, String memberName);

        void removeListener(View view, String memberName);
    }
}
