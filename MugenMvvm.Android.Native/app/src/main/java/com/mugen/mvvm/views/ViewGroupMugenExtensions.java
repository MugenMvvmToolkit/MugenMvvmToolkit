package com.mugen.mvvm.views;

import android.view.View;
import android.view.ViewGroup;

import androidx.fragment.app.Fragment;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IHasLifecycleView;
import com.mugen.mvvm.internal.ViewAttachedValues;
import com.mugen.mvvm.views.support.RecyclerViewMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

import java.util.Objects;

public final class ViewGroupMugenExtensions {
    public static final int NoneProviderType = 0;
    public static final int ResourceProviderType = 1;
    public static final int ContentProviderType = 2;
    public static final int ContentRawProviderType = 3;
    public static final int ResourceOrContentProviderType = 4;

    private ViewGroupMugenExtensions() {
    }

    public static Object get(View view, int index) {
        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.getTabAt(view, index);
        return ((ViewGroup) view).getChildAt(index);
    }

    public static void add(View view, Object child, int position, boolean setSelected) {
        if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.addTab(view, child, position, setSelected);
        else
            ((ViewGroup) view).addView((View) child, position);
    }

    public static void remove(View view, int position) {
        if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.removeTab(view, position);
        else
            ((ViewGroup) view).removeViewAt(position);
    }

    public static void clear(View view) {
        if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.clearTabs(view);
        else
            ((ViewGroup) view).removeAllViews();
    }

    public static int getItemSourceProviderType(View view) {
        if (RecyclerViewMugenExtensions.isSupported(view))
            return RecyclerViewMugenExtensions.ItemsSourceProviderType;
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.ItemsSourceProviderType;
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.ItemsSourceProviderType;
        if (AdapterViewMugenExtensions.isSupported(view))
            return AdapterViewMugenExtensions.ItemsSourceProviderType;
        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.ItemsSourceProviderType;
        if (view instanceof ViewGroup)
            return ContentRawProviderType;
        return NoneProviderType;
    }

    public static boolean isSelectedIndexSupported(View view) {
        return ViewPagerMugenExtensions.isSupported(view) || ViewPager2MugenExtensions.isSupported(view) || TabLayoutMugenExtensions.isSupported(view);
    }

    public static int getSelectedIndex(View view) {
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.getCurrentItem(view);
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.getCurrentItem(view);
        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.getSelectedTabPosition(view);
        return -1;
    }

    public static boolean setSelectedIndex(View view, int index) {
        if (ViewPagerMugenExtensions.isSupported(view)) {
            ViewPagerMugenExtensions.setCurrentItem(view, index);
            return true;
        }
        if (ViewPager2MugenExtensions.isSupported(view)) {
            ViewPager2MugenExtensions.setCurrentItem(view, index);
            return true;
        }
        if (TabLayoutMugenExtensions.isSupported(view)) {
            TabLayoutMugenExtensions.setSelectedTabPosition(view, index);
            return true;
        }
        return false;
    }

    public static IItemsSourceProviderBase getItemsSourceProvider(View view) {
        if (RecyclerViewMugenExtensions.isSupported(view))
            return RecyclerViewMugenExtensions.getItemsSourceProvider(view);
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.getItemsSourceProvider(view);
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.getItemsSourceProvider(view);
        if (AdapterViewMugenExtensions.isSupported(view))
            return AdapterViewMugenExtensions.getItemsSourceProvider(view);
        return null;
    }

    public static void setItemsSourceProvider(View view, IItemsSourceProviderBase provider, boolean hasFragments) {
        if (RecyclerViewMugenExtensions.isSupported(view))
            RecyclerViewMugenExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
        else if (ViewPager2MugenExtensions.isSupported(view))
            ViewPager2MugenExtensions.setItemsSourceProvider(view, provider, hasFragments);
        else if (ViewPagerMugenExtensions.isSupported(view))
            ViewPagerMugenExtensions.setItemsSourceProvider(view, (IContentItemsSourceProvider) provider, hasFragments);
        else if (AdapterViewMugenExtensions.isSupported(view))
            AdapterViewMugenExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
    }

    public static Object getContent(View view) {
        ViewGroup viewGroup = (ViewGroup) view;
        if (viewGroup.getChildCount() == 0)
            return null;
        View result = viewGroup.getChildAt(0);
        if (result == null)
            return null;
        ViewAttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(result, false);
        if (attachedValues == null)
            return result;
        Fragment fragment = attachedValues.getFragment();
        if (fragment == null)
            return result;
        return fragment;
    }

    public static void setContent(View view, Object content) {
        Object oldContent = getContent(view);
        if (Objects.equals(oldContent, content))
            return;

        ViewGroup viewGroup = (ViewGroup) view;
        if (MugenUtils.isCompatSupported()) {
            if (content == null && FragmentMugenExtensions.setFragment(view, null))
                return;
            if (FragmentMugenExtensions.isSupported(content) && FragmentMugenExtensions.setFragment(view, (IFragmentView) content))
                return;
        }

        boolean hasLifecycleOld = oldContent != null && !(oldContent instanceof IHasLifecycleView);
        boolean hasLifecycleNew = content != null && !(content instanceof IHasLifecycleView);
        if (hasLifecycleOld)
            LifecycleMugenExtensions.onLifecycleChanging(oldContent, LifecycleState.Pause, null);
        if (hasLifecycleNew)
            LifecycleMugenExtensions.onLifecycleChanging(content, LifecycleState.Resume, null);

        viewGroup.removeAllViews();
        if (content != null)
            viewGroup.addView((View) content);

        if (hasLifecycleOld)
            LifecycleMugenExtensions.onLifecycleChanged(oldContent, LifecycleState.Pause, null);
        if (hasLifecycleNew)
            LifecycleMugenExtensions.onLifecycleChanged(content, LifecycleState.Resume, null);
    }
}
