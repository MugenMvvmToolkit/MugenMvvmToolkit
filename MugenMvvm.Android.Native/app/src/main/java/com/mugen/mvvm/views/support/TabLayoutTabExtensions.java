package com.mugen.mvvm.views.support;

import android.graphics.drawable.Drawable;
import android.view.View;

import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.MugenNativeService;

public abstract class TabLayoutTabExtensions {
    private TabLayoutTabExtensions() {
    }

    public static boolean isSupported(Object view) {
        return MugenNativeService.isMaterialSupported() && view instanceof TabLayout.Tab;
    }

    public static Object getTag(Object tab) {
        return ((TabLayout.Tab) tab).getTag();
    }

    public static void setTag(Object tab, Object tag) {
        ((TabLayout.Tab) tab).setTag(tag);
    }

    public static View getCustomView(Object tab) {
        return ((TabLayout.Tab) tab).getCustomView();
    }

    public static void setCustomView(Object tab, View view) {
        ((TabLayout.Tab) tab).setCustomView(view);
    }

    public static void setCustomView(Object tab, int resId) {
        ((TabLayout.Tab) tab).setCustomView(resId);
    }

    public static Drawable getIcon(Object tab) {
        return ((TabLayout.Tab) tab).getIcon();
    }

    public static int getPosition(Object tab) {
        return ((TabLayout.Tab) tab).getPosition();
    }

    public static CharSequence getText(Object tab) {
        return ((TabLayout.Tab) tab).getText();
    }

    public static void setIcon(Object tab, Drawable icon) {
        ((TabLayout.Tab) tab).setIcon(icon);
    }

    public static void setIcon(Object tab, int resId) {
        ((TabLayout.Tab) tab).setIcon(resId);
    }

    public static void setText(Object tab, CharSequence text) {
        ((TabLayout.Tab) tab).setText(text);
    }

    public static Drawable getOrCreateBadge(Object tab) {
        return ((TabLayout.Tab) tab).getOrCreateBadge();
    }

    public static void removeBadge(Object tab) {
        ((TabLayout.Tab) tab).removeBadge();
    }

    public static Drawable getBadge(Object tab) {
        return ((TabLayout.Tab) tab).getBadge();
    }

    public static void setTabLabelVisibility(Object tab, int mode) {
        ((TabLayout.Tab) tab).setTabLabelVisibility(mode);
    }

    public static int getTabLabelVisibility(Object tab) {
        return ((TabLayout.Tab) tab).getTabLabelVisibility();
    }

    public static boolean isSelected(Object tab) {
        return ((TabLayout.Tab) tab).isSelected();
    }

    public static void setContentDescription(Object tab, CharSequence contentDesc) {
        ((TabLayout.Tab) tab).setContentDescription(contentDesc);
    }

    public static CharSequence getContentDescription(Object tab) {
        return ((TabLayout.Tab) tab).getContentDescription();
    }
}
