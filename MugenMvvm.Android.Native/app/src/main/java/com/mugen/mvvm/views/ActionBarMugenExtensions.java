package com.mugen.mvvm.views;

import android.app.ActionBar;
import android.content.Context;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenUtils;

public final class ActionBarMugenExtensions {
    private ActionBarMugenExtensions() {
    }

    public static boolean isSupportedCompat(@Nullable Object actionBar) {
        return MugenUtils.isCompatSupported() && actionBar instanceof androidx.appcompat.app.ActionBar;
    }

    public static boolean isSupported(@Nullable Object actionBar) {
        return isSupportedCompat(actionBar) || actionBar instanceof ActionBar;
    }

    public static Context getThemedContext(@NonNull Object actionBar) {
        if (isSupportedCompat(actionBar))
            return ((androidx.appcompat.app.ActionBar) actionBar).getThemedContext();
        return ((ActionBar) actionBar).getThemedContext();
    }

    public static void setDisplayHomeAsUpEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayHomeAsUpEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayHomeAsUpEnabled(value);
    }

    public static void setDisplayShowCustomEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowCustomEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowCustomEnabled(value);
    }

    public static void setDisplayUseLogoEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayUseLogoEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayUseLogoEnabled(value);
    }

    public static void setDisplayShowHomeEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowHomeEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowHomeEnabled(value);
    }

    public static void setDisplayShowTitleEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setDisplayShowTitleEnabled(value);
        else
            ((ActionBar) actionBar).setDisplayShowTitleEnabled(value);
    }

    public static void setHomeButtonEnabled(@NonNull Object actionBar, boolean value) {
        if (isSupportedCompat(actionBar))
            ((androidx.appcompat.app.ActionBar) actionBar).setHomeButtonEnabled(value);
        else
            ((ActionBar) actionBar).setHomeButtonEnabled(value);
    }
}
