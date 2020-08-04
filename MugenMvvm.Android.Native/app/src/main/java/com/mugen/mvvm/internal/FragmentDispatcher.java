package com.mugen.mvvm.internal;

import android.content.Context;
import android.os.Bundle;
import android.util.AttributeSet;
import android.view.View;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.FragmentExtensions;
import com.mugen.mvvm.views.ViewExtensions;

public class FragmentDispatcher implements ILifecycleDispatcher, IViewDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        if (MugenNativeService.isFragmentStateDisabled() && lifecycle == LifecycleState.Create && state instanceof Bundle)
            FragmentExtensions.clearFragmentState((Bundle) state);
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PreInitializer;
    }

    @Override
    public void onParentChanged(View view) {
    }

    @Override
    public void onSetting(Object owner, View view) {
        if (FragmentExtensions.isSupported(owner))
            ViewExtensions.getNativeAttachedValues(view, true).setFragment(owner);
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
        return view;
    }

    @Override
    public void onDestroy(View view) {
    }

    @Override
    public View tryCreate(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }
}
