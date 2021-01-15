package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.fragment.app.FragmentManager;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.internal.support.MugenFragmentPagerAdapter;
import com.mugen.mvvm.internal.support.MugenPagerAdapter;
import com.mugen.mvvm.views.FragmentMugenExtensions;
import com.mugen.mvvm.views.ViewGroupMugenExtensions;

public final class ViewPagerMugenExtensions {
    public static final int ItemsSourceProviderType = ViewGroupMugenExtensions.ContentProviderType;
    private static boolean _supported;

    private ViewPagerMugenExtensions() {
    }

    public static boolean isSupported(View view) {
        return _supported && view instanceof ViewPager;
    }

    public static void setSupported() {
        _supported = true;
    }

    public static int getCurrentItem(View view) {
        return ((ViewPager) view).getCurrentItem();
    }

    public static void setCurrentItem(View view, int index) {
        ((ViewPager) view).setCurrentItem(index);
    }

    public static void setCurrentItem(View view, int index, boolean smoothScroll) {
        ((ViewPager) view).setCurrentItem(index, smoothScroll);
    }

    public static int getOffscreenPageLimit(View view) {
        return ((ViewPager) view).getOffscreenPageLimit();
    }

    public static void setOffscreenPageLimit(View view, int limit) {
        ((ViewPager) view).setOffscreenPageLimit(limit);
    }

    public static IItemsSourceProviderBase getItemsSourceProvider(View view) {
        PagerAdapter adapter = ((ViewPager) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View view, IContentItemsSourceProvider provider, boolean hasFragments) {
        if (getItemsSourceProvider(view) == provider)
            return;
        ViewPager viewPager = (ViewPager) view;
        PagerAdapter adapter = viewPager.getAdapter();
        if (provider == null) {
            if (adapter instanceof IMugenAdapter)
                ((IMugenAdapter) adapter).detach();
            viewPager.setAdapter(null);
        } else if (hasFragments)
            viewPager.setAdapter(new MugenFragmentPagerAdapter(provider, (FragmentManager) FragmentMugenExtensions.getFragmentManager(view)));
        else
            viewPager.setAdapter(new MugenPagerAdapter(provider));
    }

    public static void onDestroy(View view) {
        setItemsSourceProvider(view, null, false);
    }
}
