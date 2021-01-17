package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;

public interface IViewItemsSourceManager {

    boolean isItemsSourceSupported(@NonNull Object view);

    int getItemSourceProviderType(@NonNull Object view);

    @Nullable
    IItemsSourceProviderBase getItemsSourceProvider(@NonNull Object view);

    void setItemsSourceProvider(@NonNull Object view, @Nullable IItemsSourceProviderBase provider, boolean hasFragments);
}
