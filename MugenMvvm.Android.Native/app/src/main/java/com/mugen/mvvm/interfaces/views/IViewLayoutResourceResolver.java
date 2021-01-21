package com.mugen.mvvm.interfaces.views;

import android.content.Intent;

import androidx.annotation.Nullable;

public interface IViewLayoutResourceResolver {
    @Nullable
    Class tryGetClassByLayoutId(int resourceId);

    int tryGetLayoutId(@Nullable Class viewClass, @Nullable Intent intent);
}
