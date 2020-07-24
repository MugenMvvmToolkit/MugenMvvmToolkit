package com.mugen.mvvm.internal;

import android.view.View;
import android.view.ViewGroup;
import com.mugen.mvvm.R;
import com.mugen.mvvm.views.ViewExtensions;

public final class ViewParentObserver implements ViewGroup.OnHierarchyChangeListener {
    public static final ViewParentObserver Instance = new ViewParentObserver();

    private ViewParentObserver() {
    }

    @Override
    public void onChildViewAdded(View parent, View child) {
        add(child);
        ViewExtensions.onParentChanged(child);
    }

    @Override
    public void onChildViewRemoved(View parent, View child) {
        ViewExtensions.onParentChanged(child);
    }

    public void add(View view) {
        if (view instanceof ViewGroup && view.getTag(R.id.disableParentObserver) == null)
            ((ViewGroup) view).setOnHierarchyChangeListener(this);
    }

    public void remove(View view, boolean setTag) {
        if (view instanceof ViewGroup) {
            if (setTag)
                view.setTag(R.id.disableParentObserver, "");
            ((ViewGroup) view).setOnHierarchyChangeListener(null);
        }
    }
}
