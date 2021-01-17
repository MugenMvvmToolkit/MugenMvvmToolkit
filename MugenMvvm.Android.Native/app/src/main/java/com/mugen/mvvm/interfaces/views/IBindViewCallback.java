package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IBindViewCallback {
    void setViewAccessor(@Nullable IViewAttributeAccessor accessor);

    void onSetView(@NonNull Object owner, @NonNull Object view);

    void bind(@NonNull Object view);
}
