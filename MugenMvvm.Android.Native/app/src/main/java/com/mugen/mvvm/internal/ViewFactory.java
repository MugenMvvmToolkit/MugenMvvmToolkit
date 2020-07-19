package com.mugen.mvvm.internal;

import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.IViewFactory;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.IHasTagView;

import java.util.ArrayList;

public class ViewFactory implements IViewFactory, ILifecycleDispatcher {

    @Override
    public Object getView(Object container, int resourceId) {
        Context context;
        if (container instanceof IAndroidView)
            context = ((IAndroidView) container).getView().getContext();
        else if (container instanceof IActivityView)
            context = ((IActivityView) container).getActivity();
        else if (container instanceof View)
            context = ((View) container).getContext();
        else
            context = (Context) container;

        //todo check fragment mapping?
        View view = LayoutInflater.from(context).inflate(resourceId, null);

        //note need to keep strong reference
        if (MugenService.IsNativeConfiguration) {
            Activity activity = MugenExtensions.getActivity(context);
            if (activity instanceof IHasTagView) {
                IHasTagView hasTagView = (IHasTagView) activity;
                ArrayList<Object> views = (ArrayList<Object>) hasTagView.getTag(R.id.views);
                if (views == null) {
                    views = new ArrayList<>();
                    hasTagView.setTag(R.id.views, views);
                }
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
        ArrayList<Object> views = (ArrayList<Object>) ((IActivityView) target).getTag(R.id.views);
        if (views == null)
            return;

        for (Object view : views)
            MugenService.onLifecycleChanged(view, LifecycleState.Destroy, null);
    }
}
