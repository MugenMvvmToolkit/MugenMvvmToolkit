package com.mugen.mvvm.extensions;

import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.content.Intent;
import android.util.SparseArray;
import android.view.View;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;
import com.mugen.mvvm.interfaces.views.IWrapperFactory;
import com.mugen.mvvm.internal.MugenService;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.views.ActivityWrapper;
import com.mugen.mvvm.views.ViewWrapper;

import java.util.HashMap;
import java.util.Map;

public final class MugenExtensions {

    private static final IWrapperFactory DefaultFactory = new IWrapperFactory() {
        @Override
        public Object wrap(Object view) {
            if (view instanceof View)
                return new ViewWrapper(view);
            return new ActivityWrapper(view);
        }

        @Override
        public int getPriority() {
            return 0;
        }
    };

    private final static SparseArray<Class> _resourceActivityMapping = new SparseArray<>();
    private final static HashMap<Class, Integer> _activityResourceMapping = new HashMap<>();

    private final static HashMap<Class, IWrapperFactory> _viewMapping = new HashMap<>();
    private final static HashMap<Class, IWrapperFactory> _cacheViewFactoryMapping = new HashMap<>();

    private static final String ViewIdIntentKey = "~v_id!";

    private MugenExtensions() {
    }

    public static Object wrap(Object view, boolean required) {
        if (view instanceof View)
            return wrap((View) view, required);
        else if (view instanceof INativeActivityView)
            return wrap((INativeActivityView) view, required);
        return view;
    }

    public static IActivityView wrap(INativeActivityView activityView, boolean required) {
        if (activityView == null)
            return null;
        IActivityView wrapper = (IActivityView) activityView.getTag(R.id.wrapper);
        if (!required || wrapper != null)
            return wrapper;

        wrapper = (IActivityView) wrap(activityView);
        activityView.setTag(R.id.wrapper, wrapper);
        return wrapper;
    }

    public static IAndroidView wrap(View view, boolean required) {
        if (view == null)
            return null;
        IAndroidView wrapper = (IAndroidView) view.getTag(R.id.wrapper);
        if (!required || wrapper != null)
            return wrapper;

        wrapper = (IAndroidView) wrap(view);
        view.setTag(R.id.wrapper, wrapper);
        return wrapper;
    }

    public static void addWrapperMapping(Class nativeClass, IWrapperFactory factory) {
        _viewMapping.put(nativeClass, factory);
        _cacheViewFactoryMapping.clear();
    }

    public static int tryGetViewId(Class activityClass, Intent intent, int defaultValue) {
        if (intent != null && intent.hasExtra(ViewIdIntentKey))
            return intent.getIntExtra(ViewIdIntentKey, defaultValue);
        if (activityClass == null)
            return defaultValue;
        Integer value = _activityResourceMapping.get(activityClass);
        if (value == null || value == 0)
            return defaultValue;
        return value;
    }

    public static void addActivityViewMapping(Class activityType, int resourceId) {
        _resourceActivityMapping.put(resourceId, activityType);
        if (_activityResourceMapping.containsKey(activityType))
            _activityResourceMapping.put(activityType, 0);
        else
            _activityResourceMapping.put(activityType, resourceId);
    }

    public static boolean startActivity(IActivityView activityView, Class activityClass, int resourceId, int flags) {
        if (activityClass == null)
            activityClass = _resourceActivityMapping.get(resourceId);
        if (activityClass == null)
            return false;

        Context context = activityView == null ? MugenService.getAppContext() : (Context) activityView;
        Intent intent = new Intent(context, activityClass);
        if (activityView == null)
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (flags != 0)
            intent.addFlags(flags);
        if (resourceId != 0)
            intent.putExtra(ViewIdIntentKey, resourceId);
        context.startActivity(intent);
        return true;
    }

    public static Activity getActivity(Context context) {
        while (true) {
            if (context instanceof Activity)
                return (Activity) context;

            if (context instanceof ContextWrapper) {
                context = ((ContextWrapper) context).getBaseContext();
                continue;
            }
            return null;
        }
    }

    private static Object wrap(Object view) {
        Class key = view.getClass();
        IWrapperFactory factory = _cacheViewFactoryMapping.get(key);
        if (factory == null) {
            for (Map.Entry<Class, IWrapperFactory> entry : _viewMapping.entrySet()) {
                if (entry.getKey().isInstance(view)) {
                    IWrapperFactory value = entry.getValue();
                    if (factory == null || factory.getPriority() <= value.getPriority())
                        factory = value;
                }
            }

            if (factory == null)
                factory = DefaultFactory;
            _cacheViewFactoryMapping.put(key, factory);
        }

        return factory.wrap(view);
    }
}
