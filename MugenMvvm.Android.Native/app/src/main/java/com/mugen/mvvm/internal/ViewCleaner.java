package com.mugen.mvvm.internal;

import android.app.Activity;
import android.content.Context;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.AdapterViewMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;
import com.mugen.mvvm.views.support.RecyclerViewMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

public class ViewCleaner implements ILifecycleDispatcher, IViewDispatcher {
    private static void clear(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup group = (ViewGroup) view;
            for (int i = 0; i < group.getChildCount(); i++) {
                clear(group.getChildAt(i));
            }
        }

        if (ViewPagerMugenExtensions.isSupported(view))
            ViewPagerMugenExtensions.onDestroy(view);
        else if (ViewPager2MugenExtensions.isSupported(view))
            ViewPager2MugenExtensions.onDestroy(view);
        else if (RecyclerViewMugenExtensions.isSupported(view))
            RecyclerViewMugenExtensions.onDestroy(view);
        else if (AdapterViewMugenExtensions.isSupported(view))
            AdapterViewMugenExtensions.onDestroy(view);
        ViewMugenExtensions.clearNativeAttachedValues(view);
    }

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (!(target instanceof Activity) || lifecycle != LifecycleState.Destroy)
            return;

        View view = ((Activity) target).findViewById(android.R.id.content);
        if (view != null)
            ViewMugenExtensions.onDestroyView(view);
    }

    @Override
    public int getPriority() {
        return PriorityConstant.PostInitializer;
    }

    @Override
    public void onParentChanged(@NonNull View view) {
    }

    @Override
    public void onInitializing(@NonNull Object owner, @NonNull View view) {
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
        clear(view);
    }

    @Override
    public View tryCreate(@Nullable View parent, @NonNull String name, @NonNull Context viewContext, @NonNull AttributeSet attrs) {
        return null;
    }
}
