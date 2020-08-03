package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenPagerAdapter;
import com.mugen.mvvm.views.ViewGroupExtensions;

public final class ViewPagerExtensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ContentProviderType;
    private static boolean _supported;

    private ViewPagerExtensions() {
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

    public static IContentItemsSourceProvider getItemsSourceProvider(View view) {
        PagerAdapter adapter = ((ViewPager) view).getAdapter();
        if (adapter instanceof MugenPagerAdapter)
            return ((MugenPagerAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View view, IContentItemsSourceProvider provider) {
        if (getItemsSourceProvider(view) == provider)
            return;
        ViewPager viewPager = (ViewPager) view;
        PagerAdapter adapter = viewPager.getAdapter();
        if (provider == null) {
            if (adapter instanceof MugenPagerAdapter)
                ((MugenPagerAdapter) adapter).detach();
            viewPager.setAdapter(null);
        } else
            viewPager.setAdapter(new MugenPagerAdapter(viewPager, provider));
    }

    public static void onDestroy(View view) {
        setItemsSourceProvider(view, null);
    }
}
