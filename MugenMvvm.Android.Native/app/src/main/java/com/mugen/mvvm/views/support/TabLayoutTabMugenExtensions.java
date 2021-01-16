package com.mugen.mvvm.views.support;

import android.graphics.drawable.Drawable;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.MugenUtils;

public final class TabLayoutTabMugenExtensions {
    private TabLayoutTabMugenExtensions() {
    }

    public static boolean isSupported(@Nullable Object view) {
        return MugenUtils.isMaterialSupported() && view instanceof TabLayout.Tab;
    }

    @Nullable
    public static Object getTag(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getTag();
    }

    public static void setTag(@NonNull Object tab, @Nullable Object tag) {
        ((TabLayout.Tab) tab).setTag(tag);
    }

    @Nullable
    public static View getCustomView(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getCustomView();
    }

    public static void setCustomView(@NonNull Object tab, @Nullable View view) {
        ((TabLayout.Tab) tab).setCustomView(view);
    }

    public static void setCustomView(@NonNull Object tab, int resId) {
        ((TabLayout.Tab) tab).setCustomView(resId);
    }

    @Nullable
    public static Drawable getIcon(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getIcon();
    }

    public static int getPosition(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getPosition();
    }

    public static CharSequence getText(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getText();
    }

    public static void setIcon(@NonNull Object tab, @Nullable Drawable icon) {
        ((TabLayout.Tab) tab).setIcon(icon);
    }

    public static void setIcon(@NonNull Object tab, int resId) {
        ((TabLayout.Tab) tab).setIcon(resId);
    }

    public static void setText(@NonNull Object tab, @Nullable CharSequence text) {
        ((TabLayout.Tab) tab).setText(text);
    }

    @NonNull
    public static Drawable getOrCreateBadge(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getOrCreateBadge();
    }

    public static void removeBadge(@NonNull Object tab) {
        ((TabLayout.Tab) tab).removeBadge();
    }

    @Nullable
    public static Drawable getBadge(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getBadge();
    }

    public static void setTabLabelVisibility(@NonNull Object tab, int mode) {
        ((TabLayout.Tab) tab).setTabLabelVisibility(mode);
    }

    public static int getTabLabelVisibility(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getTabLabelVisibility();
    }

    public static boolean isSelected(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).isSelected();
    }

    public static void setContentDescription(@NonNull Object tab, @Nullable CharSequence contentDesc) {
        ((TabLayout.Tab) tab).setContentDescription(contentDesc);
    }

    @Nullable
    public static CharSequence getContentDescription(@NonNull Object tab) {
        return ((TabLayout.Tab) tab).getContentDescription();
    }
}
