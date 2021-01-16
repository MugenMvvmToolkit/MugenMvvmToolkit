package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

public interface IMemberListener {
    void addListener(@NonNull Object target, @NonNull String memberName);

    void removeListener(@NonNull Object target, @NonNull String memberName);
}
