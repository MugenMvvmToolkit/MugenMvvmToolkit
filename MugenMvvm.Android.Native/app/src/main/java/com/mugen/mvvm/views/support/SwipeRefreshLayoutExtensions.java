package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

public final class SwipeRefreshLayoutExtensions {
    private static boolean _supported;

    private SwipeRefreshLayoutExtensions() {
    }

    public static boolean isSupported(View view) {
        return _supported && view instanceof SwipeRefreshLayout;
    }

    public static void setSupported() {
        _supported = true;
    }
}
