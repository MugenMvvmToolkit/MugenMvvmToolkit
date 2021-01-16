package com.mugen.mvvm.interfaces.views;

import androidx.annotation.Nullable;

public interface IViewAttributeAccessor {
    @Nullable
    String getString(int index);

    int getResourceId(int index);

    @Nullable
    String getBind();

    @Nullable
    String getBindStyle();

    int getItemTemplate();
}
