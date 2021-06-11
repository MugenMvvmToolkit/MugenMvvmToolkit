package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.viewpager.widget.ViewPager;
import androidx.viewpager2.widget.ViewPager2;

import com.google.android.material.tabs.TabLayout;
import com.google.android.material.tabs.TabLayoutMediator;
import com.mugen.mvvm.constants.ItemSourceProviderType;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.views.MaterialComponentMugenExtensions;

public final class TabLayoutMugenExtensions {
    public static final int ItemsSourceProviderType = ItemSourceProviderType.ContentRaw;

    private TabLayoutMugenExtensions() {
    }

    public static boolean isSupported(@Nullable Object view) {
        return MaterialComponentMugenExtensions.isSupported() && view instanceof TabLayout;
    }

    @NonNull
    public static Object newTab(@NonNull View view) {
        return ((TabLayout) view).newTab();
    }

    @Nullable
    public static Object getTabAt(@NonNull View view, int index) {
        return ((TabLayout) view).getTabAt(index);
    }

    public static void selectTab(@NonNull View view, @NonNull Object tab) {
        ((TabLayout) view).selectTab((TabLayout.Tab) tab);
    }

    public static void selectTab(@NonNull View view, @NonNull Object tab, boolean updateIndicator) {
        ((TabLayout) view).selectTab((TabLayout.Tab) tab, updateIndicator);
    }

    public static void addTab(@NonNull View view, @NonNull Object tab, boolean setSelected) {
        ((TabLayout) view).addTab((TabLayout.Tab) tab, setSelected);
    }

    public static void addTab(@NonNull View view, @NonNull Object tab, int position, boolean setSelected) {
        ((TabLayout) view).addTab((TabLayout.Tab) tab, position, setSelected);
    }

    public static void removeTab(@NonNull View view, @NonNull Object tab) {
        ((TabLayout) view).removeTab((TabLayout.Tab) tab);
    }

    public static void removeTab(@NonNull View view, int position) {
        ((TabLayout) view).removeTabAt(position);
    }

    public static void clearTabs(@NonNull View view) {
        ((TabLayout) view).removeAllTabs();
    }

    public static void setupWithViewPager(@NonNull final View view, @NonNull final View viewPager) {
        if (!ViewPager2MugenExtensions.isSupported(viewPager)) {
            ((TabLayout) view).setupWithViewPager((ViewPager) viewPager);
            return;
        }

        new TabLayoutMediator((TabLayout) view, (ViewPager2) viewPager, true, new TabLayoutMediator.TabConfigurationStrategy() {
            @Override
            public void onConfigureTab(@NonNull TabLayout.Tab tab, int position) {
                IItemsSourceProviderBase provider = ViewPager2MugenExtensions.getItemsSourceProvider(viewPager);
                if (provider != null)
                    tab.setText(provider.getItemTitle(position));
            }
        }).attach();
    }

    public static int getSelectedTabPosition(@NonNull View view) {
        return ((TabLayout) view).getSelectedTabPosition();
    }

    public static void setSelectedTabPosition(@NonNull View view, int position) {
        TabLayout tabLayout = (TabLayout) view;
        tabLayout.selectTab(tabLayout.getTabAt(position));
    }

    public static int getTabCount(@NonNull View view) {
        return ((TabLayout) view).getTabCount();
    }

    public static int getTabGravity(@NonNull View view) {
        return ((TabLayout) view).getTabGravity();
    }

    public static int getTabIndicatorGravity(@NonNull View view) {
        return ((TabLayout) view).getTabIndicatorGravity();
    }

    public static int getTabMode(@NonNull View view) {
        return ((TabLayout) view).getTabMode();
    }

    public static void setInlineLabel(@NonNull View view, boolean inline) {
        ((TabLayout) view).setInlineLabel(inline);
    }

    public static void setSelectedTabIndicatorColor(@NonNull View view, int color) {
        ((TabLayout) view).setSelectedTabIndicatorColor(color);
    }

    public static void setSelectedTabIndicatorGravity(@NonNull View view, int gravity) {
        ((TabLayout) view).setSelectedTabIndicatorGravity(gravity);
    }

    public static void setTabGravity(@NonNull View view, int gravity) {
        ((TabLayout) view).setTabGravity(gravity);
    }

    public static void setTabIndicatorFullWidth(@NonNull View view, boolean fullWidth) {
        ((TabLayout) view).setTabIndicatorFullWidth(fullWidth);
    }

    public static void setTabMode(@NonNull View view, int mode) {
        ((TabLayout) view).setTabMode(mode);
    }

    public static void setTabTextColors(@NonNull View view, int normalColor, int selectedColor) {
        ((TabLayout) view).setTabTextColors(normalColor, selectedColor);
    }
}
