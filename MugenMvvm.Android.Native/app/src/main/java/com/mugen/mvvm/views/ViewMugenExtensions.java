package com.mugen.mvvm.views;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.AttributeSet;
import android.util.Log;
import android.util.SparseArray;
import android.view.View;
import android.view.ViewTreeObserver;
import android.view.inputmethod.InputMethodManager;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.R;
import com.mugen.mvvm.constants.ActivityIntentKey;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IAttachedValueProvider;
import com.mugen.mvvm.interfaces.IWrapperManager;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IHasStateView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.interfaces.views.IViewLayoutResourceResolver;
import com.mugen.mvvm.internal.ActivityAttachedValues;
import com.mugen.mvvm.internal.AttachedValues;
import com.mugen.mvvm.internal.FragmentAttachedValues;
import com.mugen.mvvm.internal.ViewAttachedValues;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.views.activities.ActivityWrapper;
import com.mugen.mvvm.views.fragments.DialogFragmentWrapper;
import com.mugen.mvvm.views.fragments.FragmentWrapper;
import com.mugen.mvvm.views.support.TabLayoutTabMugenExtensions;

import java.lang.reflect.InvocationTargetException;
import java.util.ArrayList;
import java.util.HashMap;

public final class ViewMugenExtensions {
    private final static SparseArray<Class> _resourceViewMapping = new SparseArray<>();
    private final static HashMap<Class, Integer> _viewResourceMapping = new HashMap<>();

    private ViewMugenExtensions() {
    }

    public static boolean isDestroyed(@NonNull View view) {
        Activity activity = (Activity) ActivityMugenExtensions.tryGetActivity(view.getContext());
        if (activity == null)
            return false;
        return activity.isFinishing() || activity.isDestroyed();
    }

    public static void addParentObserver(@NonNull View view) {
        ViewParentObserver.Instance.add(view);
    }

    public static void removeParentObserver(@NonNull View view) {
        ViewParentObserver.Instance.remove(view, true);
    }

    public static boolean isSupportAttachedValues(@NonNull Object target) {
        if (MugenService.getAttachedValueProvider() != null && MugenService.getAttachedValueProvider().isSupportAttachedValues(target))
            return true;
        return target instanceof View || target instanceof IHasStateView || TabLayoutTabMugenExtensions.isSupported(target) || ActionBarMugenExtensions.isSupported(target);
    }

    @Nullable
    public static Object getAttachedValues(@NonNull Object view) {
        AttachedValues values = getNativeAttachedValues(view, false);
        if (values == null)
            return null;
        return values.getAttachedValues();
    }

    public static void setAttachedValues(@NonNull Object view, @Nullable Object values) {
        AttachedValues attachedValues = getNativeAttachedValues(view, values != null);
        if (attachedValues != null)
            attachedValues.setAttachedValues(values);
    }

    public static AttachedValues getNativeAttachedValues(@NonNull Object target, boolean required) {
        if (target instanceof View)
            return getNativeAttachedValues((View) target, required);

        IAttachedValueProvider attachedValueProvider = MugenService.getAttachedValueProvider();
        if (attachedValueProvider != null && attachedValueProvider.isSupportAttachedValues(target))
            return attachedValueProvider.getAttachedValues(target, required);

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

        if (ActionBarMugenExtensions.isSupported(target)) {
            ActivityAttachedValues attachedValues = (ActivityAttachedValues) getNativeAttachedValues(ActivityMugenExtensions.tryGetActivity(ActionBarMugenExtensions.getThemedContext(target)), true);
            AttachedValues result = attachedValues.getActionBarAttachedValues();
            if (result != null || !required)
                return result;
            result = new AttachedValues();
            attachedValues.setActionBarAttachedValues(result);
            return result;
        }

        if (TabLayoutTabMugenExtensions.isSupported(target)) {
            AttachedValues result = (AttachedValues) TabLayoutTabMugenExtensions.getTag(target);
            if (result != null || !required)
                return result;
            result = new AttachedValues();
            TabLayoutTabMugenExtensions.setTag(target, result);
            return result;
        }

        throw new UnsupportedOperationException("Object not supported " + target);
    }

    public static ViewAttachedValues getNativeAttachedValues(@NonNull View view, boolean required) {
        IAttachedValueProvider attachedValueProvider = MugenService.getAttachedValueProvider();
        if (attachedValueProvider != null && attachedValueProvider.isSupportAttachedValues(view))
            return (ViewAttachedValues) attachedValueProvider.getAttachedValues(view, required);

        if (MugenUtils.isRawViewTagMode()) {
            Object tag = view.getTag();
            if (tag == null || tag instanceof ViewAttachedValues) {
                ViewAttachedValues result = (ViewAttachedValues) tag;
                if (result != null || !required)
                    return result;
                result = new ViewAttachedValues();
                view.setTag(result);
                return result;
            }
            if (MugenUtils.hasFlag(MugenInitializationFlags.Debug))
                Log.e(MugenUtils.LogTag, "Attached values wrong tag value " + tag);
        }

        ViewAttachedValues result = (ViewAttachedValues) view.getTag(R.id.attachedValues);
        if (result != null || !required)
            return result;
        result = new ViewAttachedValues();
        view.setTag(R.id.attachedValues, result);
        return result;
    }

    public static void clearNativeAttachedValues(@NonNull View view) {
        if (!MugenUtils.isRawViewTagMode()) {
            if (view.getTag(R.id.attachedValues) != null)
                view.setTag(R.id.attachedValues, null);
        } else if (view.getTag() instanceof ViewAttachedValues)
            view.setTag(null);
    }

    public static void addViewMapping(@NonNull Class viewClass, int resourceId, boolean rewrite) {
        _resourceViewMapping.put(resourceId, viewClass);
        if (!rewrite && _viewResourceMapping.containsKey(viewClass))
            _viewResourceMapping.put(viewClass, 0);
        else
            _viewResourceMapping.put(viewClass, resourceId);
    }

    @Nullable
    public static Class tryGetClassByLayoutId(int resourceId, boolean isActivity, @Nullable Bundle metadata) {
        IViewLayoutResourceResolver layoutResourceResolver = MugenService.getLayoutResourceResolver();
        if (layoutResourceResolver != null) {
            Class clazz = layoutResourceResolver.tryGetClassByLayoutId(resourceId, isActivity, metadata);
            if (clazz != null)
                return clazz;
        }
        return _resourceViewMapping.get(resourceId);
    }

    public static int tryGetLayoutId(@Nullable Class viewClass, @Nullable Intent intent, int defaultValue, @Nullable Bundle metadata) {
        IViewLayoutResourceResolver layoutResourceResolver = MugenService.getLayoutResourceResolver();
        if (layoutResourceResolver != null) {
            int id = layoutResourceResolver.tryGetLayoutId(viewClass, intent, metadata);
            if (id != 0)
                return id;
        }

        if (intent != null && intent.hasExtra(ActivityIntentKey.ViewId))
            return intent.getIntExtra(ActivityIntentKey.ViewId, defaultValue);
        if (viewClass == null)
            return defaultValue;
        Integer value = _viewResourceMapping.get(viewClass);
        if (value == null || value == 0)
            return defaultValue;
        return value;
    }

    @Nullable
    public static Object getView(@Nullable Object container, int resourceId, boolean trackLifecycle, @Nullable Bundle metadata) throws InvocationTargetException, NoSuchMethodException, InstantiationException, IllegalAccessException {
        return ViewMugenExtensions.tryWrap(MugenService.getViewFactory().getView(container, resourceId, trackLifecycle, metadata));
    }

    public static void onParentChanged(@NonNull View view) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onParentChanged(view);
    }

    public static void onInitializingView(@NonNull Object owner, @NonNull View view) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onInitializing(owner, view);
    }

    public static void onInitializedView(@NonNull Object owner, @NonNull View view) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onInitialized(owner, view);
    }

    public static void onInflatingView(int resourceId, @NonNull Context context) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onInflating(resourceId, context);
    }

    public static void onInflatedView(@NonNull View view, int resourceId, @NonNull Context context) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onInflated(view, resourceId, context);
    }

    @Nullable
    public static View onCreatedView(@Nullable View view, @NonNull Context context, @NonNull AttributeSet attrs) {
        if (view == null)
            return null;
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            view = viewDispatchers.get(i).onCreated(view, context, attrs);
        return view;
    }

    public static void onDestroyView(@Nullable View view) {
        if (view == null)
            return;
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++)
            viewDispatchers.get(i).onDestroy(view);
    }

    @Nullable
    public static View tryCreateCustomView(@Nullable View parent, @NonNull String name, @NonNull Context viewContext, @NonNull AttributeSet attrs) {
        ArrayList<IViewDispatcher> viewDispatchers = MugenService.getViewDispatchers();
        for (int i = 0; i < viewDispatchers.size(); i++) {
            View view = viewDispatchers.get(i).tryCreate(parent, name, viewContext, attrs);
            if (view != null)
                return view;
        }
        return null;
    }

    @NonNull
    public static Object tryWrap(@Nullable Object target) {
        if (target == null)
            return null;
        if (!MugenUtils.isNativeMode())
            return target;

        IWrapperManager wrapperManager = MugenService.getWrapperManager();
        if (wrapperManager != null && wrapperManager.canWrap(target))
            return wrapperManager.wrap(target);

        if (target instanceof INativeActivityView) {
            ActivityAttachedValues attachedValues = (ActivityAttachedValues) ViewMugenExtensions.getNativeAttachedValues(target, true);
            Object wrapper = attachedValues.getWrapper();
            if (wrapper == null) {
                wrapper = new ActivityWrapper((INativeActivityView) target);
                attachedValues.setWrapper(wrapper);
            }
            return wrapper;
        }

        if (target instanceof INativeFragmentView) {
            FragmentAttachedValues attachedValues = (FragmentAttachedValues) ViewMugenExtensions.getNativeAttachedValues(target, true);
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

    public static boolean showKeyboard(@NonNull final View view) {
        if (view.getVisibility() != View.VISIBLE || !view.isEnabled())
            return false;
        view.requestFocus();
        if (view.hasWindowFocus())
            showKeyboardNow(view);
        else {
            view.getViewTreeObserver().addOnWindowFocusChangeListener(new ViewTreeObserver.OnWindowFocusChangeListener() {
                @Override
                public void onWindowFocusChanged(boolean hasFocus) {
                    if (hasFocus) {
                        showKeyboardNow(view);
                        view.getViewTreeObserver().removeOnWindowFocusChangeListener(this);
                    }
                }
            });
        }
        return true;
    }

    public static void hideKeyboard(@Nullable View view, boolean force) {
        if (view != null && (view.isFocused() || force) && !isFinishing(view)) {
            InputMethodManager manager = (InputMethodManager) view.getContext().getSystemService(Context.INPUT_METHOD_SERVICE);
            if (manager != null)
                manager.hideSoftInputFromWindow(view.getWindowToken(), 0);
        }
    }

    private static void showKeyboardNow(final View view) {
        view.post(new Runnable() {
            @Override
            public void run() {
                if (view.requestFocus() && !isFinishing(view)) {
                    InputMethodManager manager = (InputMethodManager) view.getContext().getSystemService(Context.INPUT_METHOD_SERVICE);
                    if (manager != null)
                        manager.showSoftInput(view, InputMethodManager.SHOW_IMPLICIT);
                }
            }
        });
    }

    private static boolean isFinishing(View view) {
        Activity activity = (Activity) ActivityMugenExtensions.tryGetActivity(view.getContext());
        return activity != null && activity.isFinishing();
    }
}
