package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.mugen.mvvm.views.ViewExtensions;

public abstract class SwipeRefreshLayoutExtensions extends ViewExtensions {
    private static boolean _supported;

    public static boolean isSupported(View view) {
        return _supported && view instanceof SwipeRefreshLayout;
    }

    public static void setSupported() {
        _supported = true;
    }
}
