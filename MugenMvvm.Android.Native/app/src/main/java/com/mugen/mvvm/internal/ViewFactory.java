package com.mugen.mvvm.internal;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.IViewFactory;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IHasTagView;
import com.mugen.mvvm.views.ActivityExtensions;
import com.mugen.mvvm.views.LifecycleExtensions;
import com.mugen.mvvm.views.ViewExtensions;

import java.util.ArrayList;

public class ViewFactory implements IViewFactory, ILifecycleDispatcher {

    @Override
    public Object getView(Object container, int resourceId, boolean trackLifecycle) {
        Context context;
        if (container instanceof IActivityView)
            context = ((IActivityView) container).getActivity();
        else if (container instanceof View)
            context = ((View) container).getContext();
        else
            context = (Context) container;

        //todo check fragment mapping?
        View view = LayoutInflater.from(context).inflate(resourceId, null);
        if (trackLifecycle) {
            Context activity = ActivityExtensions.getActivity(context);
            if (activity instanceof IHasTagView) {
                ArrayList<Object> views = ((ActivityAttachedValues) ViewExtensions.getNativeAttachedValues(activity, true)).getViews(true);
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

        ArrayList<Object> views = ((ActivityAttachedValues) ViewExtensions.getNativeAttachedValues(target, true)).getViews(false);
        if (views == null)
            return;

        for (Object view : views)
            LifecycleExtensions.onLifecycleChanged(view, LifecycleState.Destroy, null);
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PreInitializer;
    }
}
