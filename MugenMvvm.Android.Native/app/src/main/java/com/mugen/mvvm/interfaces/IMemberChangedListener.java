package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IMemberChangedListener {
    void onChanged(@NonNull Object sender, @NonNull CharSequence member, @Nullable Object args);
}
