package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

public interface IWrapperManager {
    boolean canWrap(@NonNull Object target);

    @NonNull
    Object wrap(@NonNull Object target);
}
