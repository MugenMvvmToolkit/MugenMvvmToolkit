package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.Nullable;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

public final class SwipeRefreshLayoutMugenExtensions {
    private static boolean _supported;

    private SwipeRefreshLayoutMugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return _supported && view instanceof SwipeRefreshLayout;
    }

    public static void setSupported() {
        _supported = true;
    }
}
