package com.mugen.mvvm.interfaces;

import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.views.IActivityView;

public interface IActivityManager {
    boolean tryStartActivity(@Nullable IActivityView activityView, @Nullable Class activityClass, int requestId, @Nullable String viewModelId, int resourceId, int flags);
}
