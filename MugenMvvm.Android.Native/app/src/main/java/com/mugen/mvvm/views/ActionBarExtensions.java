package com.mugen.mvvm.views;

import android.app.ActionBar;
import android.content.Context;

import com.mugen.mvvm.MugenNativeService;

public final class ActionBarExtensions {
    private ActionBarExtensions() {
    }

    public static boolean isSupportedCompat(Object actionBar) {
        return MugenNativeService.isCompatSupported() && actionBar instanceof androidx.appcompat.app.ActionBar;
    }

    public static boolean isSupported(Object actionBar) {
        return isSupportedCompat(actionBar) || actionBar instanceof ActionBar;
    }

    public static Context getThemedContext(Object actionBar) {
        if (isSupportedCompat(actionBar))
            return ((androidx.appcompat.app.ActionBar) actionBar).getThemedContext();
        return ((ActionBar) actionBar).getThemedContext();
    }

    public static void setDisplayHomeAsUpEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayHomeAsUpEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayHomeAsUpEnabled(value);
    }

    public static void setDisplayShowCustomEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowCustomEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowCustomEnabled(value);
    }

    public static void setDisplayUseLogoEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayUseLogoEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayUseLogoEnabled(value);
    }

    public static void setDisplayShowHomeEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowHomeEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowHomeEnabled(value);
    }

    public static void setDisplayShowTitleEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowTitleEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowTitleEnabled(value);
    }

    public static void setHomeButtonEnabled(Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setHomeButtonEnabled(value);
        else
            ((ActionBar) actionBar).setHomeButtonEnabled(value);
    }
}
