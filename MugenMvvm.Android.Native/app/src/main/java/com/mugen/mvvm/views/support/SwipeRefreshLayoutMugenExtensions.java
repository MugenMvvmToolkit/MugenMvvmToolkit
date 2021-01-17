package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.Nullable;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.MugenInitializationFlags;

public final class SwipeRefreshLayoutMugenExtensions {
    private SwipeRefreshLayoutMugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return MugenUtils.hasFlag(MugenInitializationFlags.SwipeRefreshLib) && view instanceof SwipeRefreshLayout;
    }
}
