package com.mugen.mvvm.interfaces;

public interface IMugenAdapter {
    IItemsSourceProviderBase getItemsSourceProvider();

    void detach();
}
