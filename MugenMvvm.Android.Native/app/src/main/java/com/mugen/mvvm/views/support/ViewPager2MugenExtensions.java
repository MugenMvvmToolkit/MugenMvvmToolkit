package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentManager;
import androidx.lifecycle.LifecycleOwner;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.ItemSourceProviderType;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenFragmentPager2Adapter;
import com.mugen.mvvm.internal.support.MugenPager2Adapter;
import com.mugen.mvvm.views.FragmentMugenExtensions;

public final class ViewPager2MugenExtensions {
    public static final int ItemsSourceProviderType = ItemSourceProviderType.ResourceOrContent;

    private ViewPager2MugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return MugenUtils.hasFlag(MugenInitializationFlags.ViewPager2Lib) && view instanceof ViewPager2;
    }

    public static int getCurrentItem(@NonNull View view) {
        return ((ViewPager2) view).getCurrentItem();
    }

    public static void setCurrentItem(@NonNull View view, int index) {
        ((ViewPager2) view).setCurrentItem(index);
    }

    public static void setCurrentItem(@NonNull View view, int index, boolean smoothScroll) {
        ((ViewPager2) view).setCurrentItem(index, smoothScroll);
    }

    public static int getOffscreenPageLimit(@NonNull View view) {
        return ((ViewPager2) view).getOffscreenPageLimit();
    }

    public static void setOffscreenPageLimit(@NonNull View view, int limit) {
        ((ViewPager2) view).setOffscreenPageLimit(limit);
    }

    public static int getOrientation(@NonNull View view) {
        return ((ViewPager2) view).getOrientation();
    }

    public static void setOrientation(@NonNull View view, int orientation) {
        ((ViewPager2) view).setOrientation(orientation);
    }

    @Nullable
    public static IItemsSourceProviderBase getItemsSourceProvider(@NonNull View view) {
        RecyclerView.Adapter adapter = ((ViewPager2) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(@NonNull View view, @Nullable IItemsSourceProviderBase provider, boolean hasFragments) {
        if (getItemsSourceProvider(view) == provider)
            return;
        ViewPager2 viewPager = (ViewPager2) view;
        if (provider == null)
            viewPager.setAdapter(null);
        else if (hasFragments) {
            LifecycleOwner owner = (LifecycleOwner) FragmentMugenExtensions.getFragmentOwner(view);
            viewPager.setAdapter(new MugenFragmentPager2Adapter((IContentItemsSourceProvider) provider, (FragmentManager) FragmentMugenExtensions.getFragmentManager(owner), owner.getLifecycle()));
        } else
            viewPager.setAdapter(new MugenPager2Adapter(viewPager, (IResourceItemsSourceProvider) provider));
    }

    public static void onDestroy(@NonNull View view) {
        setItemsSourceProvider(view, null, false);
    }
}
