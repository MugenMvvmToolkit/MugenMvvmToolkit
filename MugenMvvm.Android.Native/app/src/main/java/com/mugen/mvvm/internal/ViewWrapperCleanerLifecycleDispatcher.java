package com.mugen.mvvm.internal;

import android.app.Activity;
import android.view.View;
import android.view.ViewGroup;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IAndroidView;

public class ViewWrapperCleanerLifecycleDispatcher implements ILifecycleDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
        if (!(target instanceof Activity) || lifecycle != LifecycleState.Destroy) {
            return;
        }
        View view = ((Activity) target).findViewById(android.R.id.content);
        if (view != null)
            clear(view);
    }

    private static void clear(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup group = (ViewGroup) view;
            for (int i = 0; i < group.getChildCount(); i++) {
                clear(group.getChildAt(i));
            }
        }

        IAndroidView wrap = MugenExtensions.wrap(view, false);
        if (wrap != null)
            wrap.release();
    }
}
