package com.mugen.mvvm.interfaces.views;

import com.mugen.mvvm.interfaces.IItemsSourceProvider;

public interface IListView extends IAndroidView {
    IItemsSourceProvider getItemsSourceProvider();

    void setItemsSourceProvider(IItemsSourceProvider provider);
}
