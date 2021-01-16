package com.mugen.mvvm.interfaces.views;

import androidx.annotation.Nullable;

public interface IHasStateView {
    @Nullable
    Object getState();

    void setState(@Nullable Object value);
}
