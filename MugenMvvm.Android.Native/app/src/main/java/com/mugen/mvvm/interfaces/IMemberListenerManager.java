package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import java.util.HashMap;

public interface IMemberListenerManager extends IHasPriority {
    @Nullable
    IMemberListener tryGetListener(@NonNull Object target, @NonNull String memberName, @Nullable HashMap<String, IMemberListener> listeners);
}
