package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface ILifecycleDispatcher extends IHasPriority {
    boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state, boolean cancelable);

    void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state);
}
