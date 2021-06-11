package com.mugen.mvvm.internal;

import android.content.Context;
import android.os.Bundle;
import android.util.AttributeSet;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.FragmentMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class FragmentDispatcher implements ILifecycleDispatcher, IViewDispatcher {
    private final String ViewIdKey = "f_vid";

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (lifecycle == LifecycleState.Create && state instanceof Bundle) {
            if (MugenUtils.isFragmentStateDisabled())
                FragmentMugenExtensions.clearFragmentState((Bundle) state);
            else if (target instanceof IFragmentView) {
                int viewId = ((Bundle) state).getInt(ViewIdKey);
                if (viewId != 0)
                    ((IFragmentView) target).setViewResourceId(viewId);
            }
        }
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (lifecycle == LifecycleState.SaveState && !MugenUtils.isFragmentStateDisabled() && target instanceof IFragmentView && state instanceof Bundle) {
            int viewId = ((IFragmentView) target).getViewId();
            if (viewId != 0)
                ((Bundle) state).putInt(ViewIdKey, viewId);
        }
    }

    @Override
    public int getPriority() {
        return PriorityConstant.PreInitializer;
    }

    @Override
    public void onParentChanged(@NonNull View view) {
    }

    @Override
    public void onInitializing(@NonNull Object owner, @NonNull View view) {
        if (FragmentMugenExtensions.isSupported(owner)) {
            ViewMugenExtensions.getNativeAttachedValues(view, true).setFragment(owner);
            BindableMemberMugenExtensions.setParent(view, owner);
        }
    }

    @Override
    public void onInitialized(@NonNull Object owner, @NonNull View view) {
    }

    @Override
    public void onInflating(int resourceId, @NonNull Context context) {
    }

    @Override
    public void onInflated(@NonNull View view, int resourceId, @NonNull Context context) {
    }

    @Override
    public View onCreated(@NonNull View view, @NonNull Context context, @NonNull AttributeSet attrs) {
        return view;
    }

    @Override
    public void onDestroy(@NonNull View view) {
    }

    @Override
    public View tryCreate(@Nullable View parent, @NonNull String name, @NonNull Context viewContext, @NonNull AttributeSet attrs) {
        return null;
    }
}
