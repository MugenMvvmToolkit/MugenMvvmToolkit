package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IViewItemsSourceManager {

    boolean isItemsSourceSupported(@NonNull Object view);

    int getItemSourceProviderType(@NonNull Object view);

    @Nullable
    IItemsSourceProviderBase getItemsSourceProvider(@NonNull Object view);

    void setItemsSourceProvider(@NonNull Object view, @Nullable IItemsSourceProviderBase provider, boolean hasFragments);
}
