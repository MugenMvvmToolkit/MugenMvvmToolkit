package com.mugen.mvvm.interfaces.views;

import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;

public interface IListView extends IAndroidView {
    IItemsSourceProviderBase getItemsSourceProvider();

    void setItemsSourceProvider(IItemsSourceProviderBase provider);
}
