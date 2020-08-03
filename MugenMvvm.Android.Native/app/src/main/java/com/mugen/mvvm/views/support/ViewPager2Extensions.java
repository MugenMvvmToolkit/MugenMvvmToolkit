package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewGroupExtensions;

public final class ViewPager2Extensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ResourceProviderType;
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

    public static IResourceItemsSourceProvider getItemsSourceProvider(View view) {
        RecyclerView.Adapter adapter = ((ViewPager2) view).getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter)
            return ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View view, IResourceItemsSourceProvider provider) {
        if (getItemsSourceProvider(view) == provider)
            return;
        ViewPager2 viewPager = (ViewPager2) view;
        if (provider == null)
            viewPager.setAdapter(null);
        else
            viewPager.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), provider));
    }

    public static void onDestroy(View view) {
        setItemsSourceProvider(view, null);
    }
}
