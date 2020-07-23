package com.mugen.mvvm.interfaces.views;

import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;

public interface IListView extends IAndroidView {
    int ItemSourceProviderType = 1;
    int ContentProviderType = 2;

    int getProviderType();

    IItemsSourceProviderBase getItemsSourceProvider();

    void setItemsSourceProvider(IItemsSourceProviderBase provider);
}
