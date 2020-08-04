package com.mugen.mvvm.views;

import android.view.View;
import android.view.ViewGroup;
import androidx.fragment.app.Fragment;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.internal.ViewAttachedValues;
import com.mugen.mvvm.views.support.RecyclerViewExtensions;
import com.mugen.mvvm.views.support.TabLayoutExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

public final class ViewGroupExtensions {
    public static final int NoneProviderType = 0;
    public static final int ResourceProviderType = 1;
    public static final int ContentProviderType = 2;
    public static final int ContentRawProviderType = 3;
    public static final int ResourceOrContentProviderType = 4;

    private ViewGroupExtensions() {
    }

    public static Object get(View view, int index) {
        if (TabLayoutExtensions.isSupported(view))
            return TabLayoutExtensions.getTabAt(view, index);
        return ((ViewGroup) view).getChildAt(index);
    }

    public static void add(View view, Object child, int position, boolean setSelected) {
        if (TabLayoutExtensions.isSupported(view))
            TabLayoutExtensions.addTab(view, child, position, setSelected);
        else
            ((ViewGroup) view).addView((View) child, position);
    }

    public static void remove(View view, int position) {
        if (TabLayoutExtensions.isSupported(view))
            TabLayoutExtensions.removeTab(view, position);
        else
            ((ViewGroup) view).removeViewAt(position);
    }

    public static void clear(View view) {
        if (TabLayoutExtensions.isSupported(view))
            TabLayoutExtensions.clearTabs(view);
        else
            ((ViewGroup) view).removeAllViews();
    }

    public static int getItemSourceProviderType(View view) {
        if (RecyclerViewExtensions.isSupported(view))
            return RecyclerViewExtensions.ItemsSourceProviderType;
        if (ViewPager2Extensions.isSupported(view))
            return ViewPager2Extensions.ItemsSourceProviderType;
        if (ViewPagerExtensions.isSupported(view))
            return ViewPagerExtensions.ItemsSourceProviderType;
        if (AdapterViewExtensions.isSupported(view))
            return AdapterViewExtensions.ItemsSourceProviderType;
        if (TabLayoutExtensions.isSupported(view))
            return TabLayoutExtensions.ItemsSourceProviderType;
        if (view instanceof ViewGroup)
            return ContentRawProviderType;
        return NoneProviderType;
    }

    public static boolean isSelectedIndexSupported(View view) {
        return ViewPagerExtensions.isSupported(view) || ViewPager2Extensions.isSupported(view) || TabLayoutExtensions.isSupported(view);
    }

    public static int getSelectedIndex(View view) {
        if (ViewPagerExtensions.isSupported(view))
            return ViewPagerExtensions.getCurrentItem(view);
        if (ViewPager2Extensions.isSupported(view))
            return ViewPager2Extensions.getCurrentItem(view);
        if (TabLayoutExtensions.isSupported(view))
            return TabLayoutExtensions.getSelectedTabPosition(view);
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
        if (TabLayoutExtensions.isSupported(view)) {
            TabLayoutExtensions.setSelectedTabPosition(view, index);
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

    public static void setItemsSourceProvider(View view, IItemsSourceProviderBase provider, boolean hasFragments) {
        if (RecyclerViewExtensions.isSupported(view))
            RecyclerViewExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
        else if (ViewPager2Extensions.isSupported(view))
            ViewPager2Extensions.setItemsSourceProvider(view, provider, hasFragments);
        else if (ViewPagerExtensions.isSupported(view))
            ViewPagerExtensions.setItemsSourceProvider(view, (IContentItemsSourceProvider) provider, hasFragments);
        else if (AdapterViewExtensions.isSupported(view))
            AdapterViewExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
    }

    public static Object getContent(View view) {
        ViewGroup viewGroup = (ViewGroup) view;
        if (viewGroup.getChildCount() == 0)
            return null;
        View result = viewGroup.getChildAt(0);
        if (result == null)
            return null;
        ViewAttachedValues attachedValues = ViewExtensions.getNativeAttachedValues(result, false);
        if (attachedValues == null)
            return result;
        Fragment fragment = attachedValues.getFragment();
        if (fragment == null)
            return result;
        return fragment;
    }

    public static void setContent(View view, Object content) {
        ViewGroup viewGroup = (ViewGroup) view;
        if (MugenNativeService.isCompatSupported()) {
            if (content == null && FragmentExtensions.setFragment(view, null))
                return;
            if (FragmentExtensions.isSupported(content) && FragmentExtensions.setFragment(view, (IFragmentView) content))
                return;
        }

        viewGroup.removeAllViews();
        if (content != null)
            viewGroup.addView((View) content);
    }
}
