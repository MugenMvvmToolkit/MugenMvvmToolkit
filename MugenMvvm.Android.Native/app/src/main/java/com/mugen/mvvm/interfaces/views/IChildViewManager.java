package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IChildViewManager {
    boolean isSupported(@NonNull Object view);

    boolean isChildRecycleSupported(@NonNull Object view);

    @Nullable
    Object getChildAt(@NonNull Object view, int index);

    void addChild(@NonNull Object view, @NonNull Object child, int position, boolean setSelected);

    void removeChildAt(@NonNull Object view, int position);

    void clear(@NonNull Object view);
}
