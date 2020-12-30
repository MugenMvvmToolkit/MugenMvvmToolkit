package com.mugen.mvvm.internal;

import android.view.View;
import android.view.ViewGroup;

import com.mugen.mvvm.views.ViewMugenExtensions;

public final class ViewParentObserver implements ViewGroup.OnHierarchyChangeListener {
    public static final ViewParentObserver Instance = new ViewParentObserver();

    private ViewParentObserver() {
    }

    private static boolean isDisabled(View view) {
        ViewAttachedValues values = ViewMugenExtensions.getNativeAttachedValues(view, false);
        return values != null && values.isParentObserverDisabled();
    }

    @Override
    public void onChildViewAdded(View parent, View child) {
        add(child);
        ViewMugenExtensions.onParentChanged(child);
    }

    @Override
    public void onChildViewRemoved(View parent, View child) {
        ViewMugenExtensions.onParentChanged(child);
    }

    public void add(View view) {
        if (view instanceof ViewGroup && !isDisabled(view))
            ((ViewGroup) view).setOnHierarchyChangeListener(this);
    }

    public void remove(View view, boolean setTag) {
        if (view instanceof ViewGroup) {
            if (setTag)
                ViewMugenExtensions.getNativeAttachedValues(view, true).setParentObserverDisabled(true);
            ((ViewGroup) view).setOnHierarchyChangeListener(null);
        }
    }
}
