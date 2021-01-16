package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;

public interface IBindViewCallback {
    void setViewAccessor(@NonNull IViewAttributeAccessor accessor);

    void onSetView(@NonNull Object owner, @NonNull Object view);

    void bind(@NonNull Object view);
}
