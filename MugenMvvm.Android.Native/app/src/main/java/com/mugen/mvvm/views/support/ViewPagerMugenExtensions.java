package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentManager;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.internal.support.MugenFragmentPagerAdapter;
import com.mugen.mvvm.internal.support.MugenPagerAdapter;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.FragmentMugenExtensions;

public final class ViewPagerMugenExtensions {
    public static final int ItemsSourceProviderType = BindableMemberMugenExtensions.ContentProviderType;

    private ViewPagerMugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return MugenUtils.hasFlag(MugenInitializationFlags.ViewPagerLib) && view instanceof ViewPager;
    }

    public static int getCurrentItem(@NonNull View view) {
        return ((ViewPager) view).getCurrentItem();
    }

    public static void setCurrentItem(@NonNull View view, int index) {
        ((ViewPager) view).setCurrentItem(index);
    }

    public static void setCurrentItem(@NonNull View view, int index, boolean smoothScroll) {
        ((ViewPager) view).setCurrentItem(index, smoothScroll);
    }

    public static int getOffscreenPageLimit(@NonNull View view) {
        return ((ViewPager) view).getOffscreenPageLimit();
    }

    public static void setOffscreenPageLimit(@NonNull View view, int limit) {
        ((ViewPager) view).setOffscreenPageLimit(limit);
    }

    @Nullable
    public static IItemsSourceProviderBase getItemsSourceProvider(@NonNull View view) {
        PagerAdapter adapter = ((ViewPager) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(@NonNull View view, @Nullable IContentItemsSourceProvider provider, boolean hasFragments) {
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

    public static void onDestroy(@NonNull View view) {
        setItemsSourceProvider(view, null, false);
    }
}
