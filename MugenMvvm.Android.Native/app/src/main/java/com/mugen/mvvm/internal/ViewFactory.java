package com.mugen.mvvm.internal;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IHasStateView;
import com.mugen.mvvm.interfaces.views.IViewFactory;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

import java.lang.reflect.InvocationTargetException;
import java.util.ArrayList;

public class ViewFactory implements IViewFactory, ILifecycleDispatcher {

    @Override
    public Object getView(Object container, int resourceId, boolean trackLifecycle) throws NoSuchMethodException, IllegalAccessException, InvocationTargetException, InstantiationException {
        Context context;
        if (container instanceof IActivityView)
            context = (Context) ((IActivityView) container).getActivity();
        else if (container instanceof View)
            context = ((View) container).getContext();
        else
            context = (Context) container;

        Class clazz = ViewMugenExtensions.tryGetClassById(resourceId);
        if (clazz != null && IFragmentView.class.isAssignableFrom(clazz))
            return clazz.getConstructor().newInstance();

        View view = LayoutInflater.from(context).inflate(resourceId, null);
        if (trackLifecycle) {
            Context activity = ActivityMugenExtensions.getActivity(context);
            if (activity instanceof IHasStateView) {
                ArrayList<Object> views = ((ActivityAttachedValues) ViewMugenExtensions.getNativeAttachedValues(activity, true)).getViews(true);
                views.add(view);
            }
        }

        return view;
    }

    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
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
        return PriorityConstants.PreInitializer;
    }
}
