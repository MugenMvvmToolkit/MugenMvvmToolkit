package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IViewContentManager {
    boolean isContentSupported(@NonNull Object view);

    @Nullable
    Object getContent(@NonNull Object view);

    void setContent(@NonNull Object view, Object content);
}
