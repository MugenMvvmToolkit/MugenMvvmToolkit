package com.mugen.mvvm.views;

import android.content.Context;
import android.content.Intent;
import android.util.AttributeSet;
import android.util.SparseArray;
import android.view.Menu;
import android.view.View;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.IHasPriority;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.interfaces.views.IViewFactory;
import com.mugen.mvvm.interfaces.views.*;
import com.mugen.mvvm.internal.*;
import com.mugen.mvvm.views.support.TabLayoutTabExtensions;

import java.lang.reflect.InvocationTargetException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;

public final class ViewExtensions {
    public static final CharSequence ParentMemberName = "Parent";
    public static final CharSequence ParentEventName = "ParentChanged";
    public static final CharSequence ClickEventName = "Click";
    public static final CharSequence LongClickEventName = "LongClick";
    public static final CharSequence TextMemberName = "Text";
    public static final CharSequence TextEventName = "TextChanged";
    public static final CharSequence HomeButtonClick = "HomeButtonClick";
    public static final CharSequence RefreshedEventName = "Refreshed";
    public final static CharSequence SelectedIndexName = "SelectedIndex";
    public final static CharSequence SelectedIndexEventName = "SelectedIndexChanged";

    private static IViewFactory _viewFactory;

    private final static SparseArray<Class> _resourceViewMapping = new SparseArray<>();
    private final static HashMap<Class, Integer> _viewResourceMapping = new HashMap<>();
    private final static ArrayList<IViewDispatcher> _viewDispatchers = new ArrayList<>();
    private final static ArrayList<IMemberListenerManager> ListenerManagers = new ArrayList<>();
    protected final static Object NullParent = "";

    private ViewExtensions() {
    }

    public static void registerMemberListenerManager(IMemberListenerManager manager) {
        ListenerManagers.add(manager);
        Collections.sort(ListenerManagers, HasPriorityComparator.Instance);
    }

    public static boolean unregisterMemberListenerManager(String memberName, IMemberListenerManager manager) {
        return ListenerManagers.remove(manager);
    }

    public static IMemberChangedListener getMemberChangedListener(Object target) {
        AttachedValues attachedValues = getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return null;
        return attachedValues.getMemberListener();
    }

    public static void setMemberChangedListener(Object target, IMemberChangedListener listener) {
        getNativeAttachedValues(target, true).setMemberListener(listener);
    }

    public static boolean addMemberListener(Object target, CharSequence memberNameChar) {
        String memberName = (String) memberNameChar;
        if (ParentEventName.equals(memberName) || ParentMemberName.equals(memberName) || HomeButtonClick.equals(memberName))
            return true;

        AttachedValues attachedValues = getNativeAttachedValues(target, false);
        MemberChangedListenerWrapper listeners = attachedValues == null ? null : attachedValues.getMemberListenerWrapper(false);
        if (listeners != null) {
            IMemberListener memberListener = listeners.get(memberName);
            if (memberListener != null) {
                memberListener.addListener(target, memberName);
                return true;
            }
        }

        for (int i = 0; i < ListenerManagers.size(); i++) {
            IMemberListener memberListener = ListenerManagers.get(i).tryGetListener(listeners, target, memberName);
            if (memberListener != null) {
                if (listeners == null) {
                    if (attachedValues == null)
                        attachedValues = getNativeAttachedValues(target, true);
                    listeners = attachedValues.getMemberListenerWrapper(true);
                }

                listeners.put(memberName, memberListener);
                memberListener.addListener(target, memberName);
                return true;
            }
        }
        return false;
    }

    public static boolean removeMemberListener(Object target, CharSequence memberName) {
        AttachedValues attachedValues = getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return false;

        MemberChangedListenerWrapper listeners = attachedValues.getMemberListenerWrapper(false);
        if (listeners == null)
            return false;

        IMemberListener memberListener = listeners.get((String) memberName);
        if (memberListener == null)
            return false;

        memberListener.removeListener(target, (String) memberName);
        return true;
    }

    public static void addParentObserver(View view) {
        ViewParentObserver.Instance.add(view);
    }

    public static void removeParentObserver(View view) {
        ViewParentObserver.Instance.remove(view, true);
    }

    public static boolean onMemberChanged(Object target, CharSequence memberName, Object args) {
        AttachedValues attachedValues = getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return false;
        MemberChangedListenerWrapper listener = attachedValues.getMemberListenerWrapper(false);
        if (listener == null)
            return false;

        listener.onChanged(target, memberName, args);
        return true;
    }

    public static Object getParent(View view) {
        return getParentRaw(view);
    }

    public static void setParent(View view, Object parent) {
        Object oldParent = getParentRaw(view);
        parent = tryWrap(parent);
        if (oldParent == parent)
            return;

        getNativeAttachedValues(view, true).setParent(parent);
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

    public static boolean isMenuSupported(View view) {
        return ToolbarExtensions.isSupported(view);
    }

    public static Menu getMenu(View view) {
        return ToolbarExtensions.getMenu(view);
    }

    public static boolean isSupportAttachedValues(Object target) {
        return target instanceof View || target instanceof IHasStateView || TabLayoutTabExtensions.isSupported(target) || ActionBarExtensions.isSupported(target);
    }

    public static ViewAttachedValues getNativeAttachedValues(View view, boolean required) {
        if (MugenNativeService.isRawViewTagMode()) {
            ViewAttachedValues result = (ViewAttachedValues) view.getTag();
            if (result != null || !required)
                return result;
            result = new ViewAttachedValues();
            view.setTag(result);
            return result;
        }

        ViewAttachedValues result = (ViewAttachedValues) view.getTag(R.id.attachedValues);
        if (result != null || !required)
            return result;
        result = new ViewAttachedValues();
        view.setTag(R.id.attachedValues, result);
        return result;
    }

    public static AttachedValues getNativeAttachedValues(Object target, boolean required) {
        if (target instanceof View)
            return getNativeAttachedValues((View) target, required);

        if (target instanceof IHasStateView) {
            IHasStateView hasStateView = (IHasStateView) target;
            AttachedValues result = (AttachedValues) hasStateView.getState();
            if (result != null || !required)
                return result;
            if (target instanceof IActivityView)
                result = new ActivityAttachedValues();
            else if (target instanceof IFragmentView)
                result = new FragmentAttachedValues();
            else
                result = new AttachedValues();
            hasStateView.setState(result);
            return result;
        }

        if (ActionBarExtensions.isSupported(target)) {
            ActivityAttachedValues attachedValues = (ActivityAttachedValues) getNativeAttachedValues(ActivityExtensions.getActivity(ActionBarExtensions.getThemedContext(target)), true);
            AttachedValues result = attachedValues.getActionBarAttachedValues();
            if (result != null || !required)
                return result;
            result = new AttachedValues();
            attachedValues.setActionBarAttachedValues(result);
            return result;
        }

        if (TabLayoutTabExtensions.isSupported(target)) {
            AttachedValues result = (AttachedValues) TabLayoutTabExtensions.getTag(target);
            if (result != null || !required)
                return result;
            result = new AttachedValues();
            TabLayoutTabExtensions.setTag(target, result);
            return result;
        }

        throw new UnsupportedOperationException("Object not supported " + target);
    }

    public static Object getAttachedValues(Object view) {
        AttachedValues values = getNativeAttachedValues(view, false);
        if (values == null)
            return null;
        return values.getAttachedValues();
    }

    public static void setAttachedValues(Object view, Object values) {
        getNativeAttachedValues(view, true).setAttachedValues(values);
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

    public static Object getView(Object container, int resourceId, boolean trackLifecycle) throws InvocationTargetException, NoSuchMethodException, InstantiationException, IllegalAccessException {
        return ViewExtensions.tryWrap(getViewFactory().getView(container, resourceId, trackLifecycle));
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
            LifecycleExtensions.addLifecycleDispatcher((ILifecycleDispatcher) _viewFactory, false);
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher) {
        _viewDispatchers.add(viewDispatcher);
        Collections.sort(_viewDispatchers, HasPriorityComparator.Instance);
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

    public static Object tryWrap(Object target) {
        if (!MugenNativeService.isNativeMode())
            return target;

        if (target instanceof INativeActivityView) {
            ActivityAttachedValues attachedValues = (ActivityAttachedValues) ViewExtensions.getNativeAttachedValues(target, true);
            Object wrapper = attachedValues.getWrapper();
            if (wrapper == null) {
                wrapper = new ActivityWrapper((INativeActivityView) target);
                attachedValues.setWrapper(wrapper);
            }
            return wrapper;
        }

        if (target instanceof INativeFragmentView) {
            FragmentAttachedValues attachedValues = (FragmentAttachedValues) ViewExtensions.getNativeAttachedValues(target, true);
            Object wrapper = attachedValues.getWrapper();
            if (wrapper == null) {
                if (target instanceof IDialogFragmentView)
                    wrapper = new DialogFragmentWrapper((INativeFragmentView) target);
                else
                    wrapper = new FragmentWrapper((INativeFragmentView) target);
                attachedValues.setWrapper(wrapper);
            }
            return wrapper;
        }
        return target;
    }

    private static Object getParentRaw(View view) {
        if (view.getId() == android.R.id.content)
            return tryWrap(ActivityExtensions.getActivity(view.getContext()));

        ViewAttachedValues attachedValues = getNativeAttachedValues(view, false);
        Object parent = attachedValues == null ? null : attachedValues.getParent();
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

    public interface IMemberListenerManager extends IHasPriority {
        IMemberListener tryGetListener(HashMap<String, IMemberListener> listeners, Object target, String memberName);
    }

    public interface IMemberListener {
        void addListener(Object target, String memberName);

        void removeListener(Object target, String memberName);
    }
}
