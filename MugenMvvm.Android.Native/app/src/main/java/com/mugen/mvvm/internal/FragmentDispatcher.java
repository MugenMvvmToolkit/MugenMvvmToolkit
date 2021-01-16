package com.mugen.mvvm.internal;

import android.content.Context;
import android.os.Bundle;
import android.util.AttributeSet;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.FragmentMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class FragmentDispatcher implements ILifecycleDispatcher, IViewDispatcher {
    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (MugenUtils.isFragmentStateDisabled() && lifecycle == LifecycleState.Create && state instanceof Bundle)
            FragmentMugenExtensions.clearFragmentState((Bundle) state);
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PreInitializer;
    }

    @Override
    public void onParentChanged(@NonNull View view) {
    }

    @Override
    public void onInitializing(@NonNull Object owner, @NonNull View view) {
        if (FragmentMugenExtensions.isSupported(owner))
            ViewMugenExtensions.getNativeAttachedValues(view, true).setFragment(owner);
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
