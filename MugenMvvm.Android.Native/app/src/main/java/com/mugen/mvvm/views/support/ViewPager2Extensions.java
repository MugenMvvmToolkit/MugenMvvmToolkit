package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.fragment.app.FragmentManager;
import androidx.lifecycle.LifecycleOwner;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenFragmentPager2Adapter;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.FragmentExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;

public final class ViewPager2Extensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ResourceOrContentProviderType;
    private static boolean _supported;

    private ViewPager2Extensions() {
    }

    public static boolean isSupported(View view) {
        return _supported && view instanceof ViewPager2;
    }

    public static void setSupported() {
        _supported = true;
    }

    public static int getCurrentItem(View view) {
        return ((ViewPager2) view).getCurrentItem();
    }

    public static void setCurrentItem(View view, int index) {
        ((ViewPager2) view).setCurrentItem(index);
    }

    public static int setOffscreenPageLimit(View view) {
        return ((ViewPager2) view).getOffscreenPageLimit();
    }

    public static void setOffscreenPageLimit(View view, int limit) {
        ((ViewPager2) view).setOffscreenPageLimit(limit);
    }

    public static IItemsSourceProviderBase getItemsSourceProvider(View view) {
        RecyclerView.Adapter adapter = ((ViewPager2) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View view, IItemsSourceProviderBase provider, boolean hasFragments) {
        if (getItemsSourceProvider(view) == provider)
            return;
        ViewPager2 viewPager = (ViewPager2) view;
        if (provider == null)
            viewPager.setAdapter(null);
        else if (hasFragments) {
            LifecycleOwner owner = (LifecycleOwner) FragmentExtensions.getFragmentOwner(view);
            viewPager.setAdapter(new MugenFragmentPager2Adapter((IContentItemsSourceProvider) provider, (FragmentManager) FragmentExtensions.getFragmentManager(owner), owner.getLifecycle()));
        } else
            viewPager.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), (IResourceItemsSourceProvider) provider));
    }

    public static void onDestroy(View view) {
        setItemsSourceProvider(view, null, false);
    }
}
