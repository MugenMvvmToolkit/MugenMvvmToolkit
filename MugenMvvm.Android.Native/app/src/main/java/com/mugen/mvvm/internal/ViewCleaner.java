package com.mugen.mvvm.internal;

import android.app.Activity;
import android.content.Context;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.AdapterViewExtensions;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.support.RecyclerViewExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

public class ViewCleaner implements ILifecycleDispatcher, IViewDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
        if (!(target instanceof Activity) || lifecycle != LifecycleState.Destroy)
            return;

        View view = ((Activity) target).findViewById(android.R.id.content);
        if (view != null)
            ViewExtensions.onDestroyView(view);
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PostInitializer;
    }

    @Override
    public void onParentChanged(View view) {
    }

    @Override
    public void onSetting(Object owner, View view) {
    }

    @Override
    public void onSet(Object owner, View view) {
    }

    @Override
    public void onInflating(int resourceId, Context context) {
    }

    @Override
    public void onInflated(View view, int resourceId, Context context) {
    }

    @Override
    public View onCreated(View view, Context context, AttributeSet attrs) {
        return null;
    }

    @Override
    public void onDestroy(View view) {
        clear(view);
    }

    @Override
    public View tryCreate(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }

    private static void clear(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup group = (ViewGroup) view;
            for (int i = 0; i < group.getChildCount(); i++) {
                clear(group.getChildAt(i));
            }
        }

        if (ViewPagerExtensions.isSupported(view))
            ViewPagerExtensions.onDestroy(view);
        else if (ViewPager2Extensions.isSupported(view))
            ViewPager2Extensions.onDestroy(view);
        else if (RecyclerViewExtensions.isSupported(view))
            RecyclerViewExtensions.onDestroy(view);
        else if (AdapterViewExtensions.isSupported(view))
            AdapterViewExtensions.onDestroy(view);
        ViewExtensions.setAttachedValues(view, null);
    }
}
