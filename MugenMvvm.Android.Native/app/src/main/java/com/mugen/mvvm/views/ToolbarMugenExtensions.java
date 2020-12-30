package com.mugen.mvvm.views;

import android.annotation.SuppressLint;
import android.graphics.drawable.Drawable;
import android.os.Build;
import android.view.Menu;
import android.view.View;
import android.widget.Toolbar;

import com.mugen.mvvm.MugenUtils;

@SuppressLint("NewApi")
public final class ToolbarMugenExtensions {
    private ToolbarMugenExtensions() {
    }

    public static boolean isSupportedCompat(View view) {
        return MugenUtils.isCompatSupported() && view instanceof androidx.appcompat.widget.Toolbar;
    }

    public static boolean isSupported(View view) {
        return isSupportedCompat(view) || (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP && view instanceof Toolbar);
    }

    public static Menu getMenu(View view) {
        if (isSupportedCompat(view))
            return ((androidx.appcompat.widget.Toolbar) view).getMenu();
        return ((Toolbar) view).getMenu();
    }

    public static CharSequence getTitle(View view) {
        if (isSupportedCompat(view))
            return ((androidx.appcompat.widget.Toolbar) view).getTitle();
        return ((Toolbar) view).getTitle();
    }

    public static void setTitle(View view, CharSequence title) {
        if (isSupportedCompat(view))
            ((androidx.appcompat.widget.Toolbar) view).setTitle(title);
        else
            ((Toolbar) view).setTitle(title);
    }

    public static CharSequence getSubtitle(View view) {
        if (isSupportedCompat(view))
            return ((androidx.appcompat.widget.Toolbar) view).getSubtitle();
        return ((Toolbar) view).getSubtitle();
    }

    public static void setSubtitle(View view, CharSequence subtitle) {
        if (isSupportedCompat(view))
            ((androidx.appcompat.widget.Toolbar) view).setSubtitle(subtitle);
        else
            ((Toolbar) view).setSubtitle(subtitle);
    }

    public static Drawable getNavigationIcon(View view) {
        if (isSupportedCompat(view))
            return ((androidx.appcompat.widget.Toolbar) view).getNavigationIcon();
        return ((Toolbar) view).getNavigationIcon();
    }

    public static void setNavigationIcon(View view, Drawable icon) {
        if (isSupportedCompat(view))
            ((androidx.appcompat.widget.Toolbar) view).setNavigationIcon(icon);
        else
            ((Toolbar) view).setNavigationIcon(icon);
    }
}
