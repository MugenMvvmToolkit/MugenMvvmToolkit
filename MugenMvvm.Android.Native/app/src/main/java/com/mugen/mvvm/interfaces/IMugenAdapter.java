package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

public interface IMugenAdapter {
    @NonNull
    IItemsSourceProviderBase getItemsSourceProvider();

    void detach();
}
