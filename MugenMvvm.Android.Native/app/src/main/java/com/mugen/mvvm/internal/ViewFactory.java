package com.mugen.mvvm.internal;

import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IHasContext;
import com.mugen.mvvm.interfaces.views.IHasStateView;
import com.mugen.mvvm.interfaces.views.IViewFactory;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

import java.lang.reflect.InvocationTargetException;
import java.util.ArrayList;

public class ViewFactory implements IViewFactory, ILifecycleDispatcher {

    @Nullable
    @Override
    public Object getView(@Nullable Object container, int resourceId, boolean trackLifecycle, @Nullable Bundle metadata) throws NoSuchMethodException, IllegalAccessException, InvocationTargetException, InstantiationException {
        Class clazz = ViewMugenExtensions.tryGetClassByLayoutId(resourceId, false, metadata);
        if (clazz != null && IFragmentView.class.isAssignableFrom(clazz)) {
            IFragmentView fragmentView = (IFragmentView) clazz.getConstructor().newInstance();
            if (resourceId != 0)
                fragmentView.setViewResourceId(resourceId);
            return fragmentView;
        }

        Context context = getContext(container);
        ViewGroup parent = null;
        if (container instanceof ViewGroup)
            parent = (ViewGroup) container;
        View view = LayoutInflater.from(context).inflate(resourceId, parent, false);
        if (trackLifecycle) {
            Context activity = ActivityMugenExtensions.tryGetActivity(context);
            if (activity instanceof IHasStateView) {
                ArrayList<Object> views = ((ActivityAttachedValues) ViewMugenExtensions.getNativeAttachedValues(activity, true)).getViews(true);
                views.add(view);
            }
        }

        return view;
    }

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state, boolean cancelable) {
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (lifecycle != LifecycleState.Destroy || !(target instanceof IActivityView))
            return;

        ArrayList<Object> views = ((ActivityAttachedValues) ViewMugenExtensions.getNativeAttachedValues(target, true)).getViews(false);
        if (views == null)
            return;

        for (Object view : views)
            LifecycleMugenExtensions.onLifecycleChanged(view, LifecycleState.Destroy, null);
    }

    @Override
    public int getPriority() {
        return PriorityConstant.PreInitializer;
    }

    protected Context getContext(@Nullable Object container) {
        Context context;
        if (container instanceof IHasContext)
            context = ((IHasContext) container).getContext();
        else if (container instanceof View)
            context = ((View) container).getContext();
        else if (container instanceof Context)
            context = (Context) container;
        else
            context = null;

        if (context == null)
            return ActivityMugenExtensions.getCurrentActivity();
        return context;
    }
}
