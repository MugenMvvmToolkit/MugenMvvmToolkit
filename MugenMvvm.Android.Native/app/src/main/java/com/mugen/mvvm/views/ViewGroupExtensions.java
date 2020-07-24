package com.mugen.mvvm.views;

import android.view.View;
import android.view.ViewGroup;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.views.support.RecyclerViewExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

public abstract class ViewGroupExtensions extends ViewExtensions {
    public final static String SelectedIndexName = "SelectedIndex";
    public final static String SelectedIndexEventName = "SelectedIndexChanged";
    public static final int NoneProviderType = 0;
    public static final int ResourceProviderType = 1;
    public static final int ContentProviderType = 2;

    public static int getItemSourceProviderType(View view) {
        if (RecyclerViewExtensions.isSupported(view))
            return RecyclerViewExtensions.ItemsSourceProviderType;
        if (ViewPager2Extensions.isSupported(view))
            return ViewPager2Extensions.ItemsSourceProviderType;
        if (ViewPagerExtensions.isSupported(view))
            return ViewPagerExtensions.ItemsSourceProviderType;
        if (AdapterViewExtensions.isSupported(view))
            return AdapterViewExtensions.ItemsSourceProviderType;
        return NoneProviderType;
    }

    public static boolean isSelectedIndexSupported(View view) {
        return ViewPagerExtensions.isSupported(view) || ViewPager2Extensions.isSupported(view);
    }

    public static int getSelectedIndex(View view) {
        if (ViewPagerExtensions.isSupported(view))
            return ViewPagerExtensions.getCurrentItem(view);
        if (ViewPager2Extensions.isSupported(view))
            return ViewPager2Extensions.getCurrentItem(view);
        return -1;
    }

    public static boolean setSelectedIndex(View view, int index) {
        if (ViewPagerExtensions.isSupported(view)) {
            ViewPagerExtensions.setCurrentItem(view, index);
            return true;
        }
        if (ViewPager2Extensions.isSupported(view)) {
            ViewPager2Extensions.setCurrentItem(view, index);
            return true;
        }
        return false;
    }

    public static IItemsSourceProviderBase getItemsSourceProvider(View view) {
        if (RecyclerViewExtensions.isSupported(view))
            return RecyclerViewExtensions.getItemsSourceProvider(view);
        if (ViewPager2Extensions.isSupported(view))
            return ViewPager2Extensions.getItemsSourceProvider(view);
        if (ViewPagerExtensions.isSupported(view))
            return ViewPagerExtensions.getItemsSourceProvider(view);
        if (AdapterViewExtensions.isSupported(view))
            return AdapterViewExtensions.getItemsSourceProvider(view);
        return null;
    }

    public static void setItemsSourceProvider(View view, IItemsSourceProviderBase provider) {
        if (RecyclerViewExtensions.isSupported(view))
            RecyclerViewExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
        else if (ViewPager2Extensions.isSupported(view))
            ViewPager2Extensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
        else if (ViewPagerExtensions.isSupported(view))
            ViewPagerExtensions.setItemsSourceProvider(view, (IContentItemsSourceProvider) provider);
        else if (AdapterViewExtensions.isSupported(view))
            AdapterViewExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
    }

    public static Object getContent(View view) {
        ViewGroup viewGroup = (ViewGroup) view;
        if (viewGroup.getChildCount() == 0)
            return null;
        return viewGroup.getChildAt(0);
    }

    public static void setContent(View view, Object content) {
        ViewGroup viewGroup = (ViewGroup) view;
        viewGroup.removeAllViews();
        if (content != null)
            viewGroup.addView((View) content);
    }
}
