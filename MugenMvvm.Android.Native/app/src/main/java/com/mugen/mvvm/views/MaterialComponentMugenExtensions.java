package com.mugen.mvvm.views;

import android.view.Menu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.navigation.NavigationView;
import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.MugenInitializationFlags;

public final class MaterialComponentMugenExtensions {
    private MaterialComponentMugenExtensions() {
    }

    public static boolean isSupported() {
        return MugenUtils.hasFlag(MugenInitializationFlags.MaterialLib);
    }

    public static boolean isMenuSupported(@Nullable Object view) {
        return isSupported() && (view instanceof NavigationView || view instanceof BottomNavigationView);
    }

    @NonNull
    public static Menu getMenu(@NonNull Object view) {
        if (view instanceof NavigationView)
            return ((NavigationView) view).getMenu();
        return ((BottomNavigationView) view).getMenu();
    }
}
