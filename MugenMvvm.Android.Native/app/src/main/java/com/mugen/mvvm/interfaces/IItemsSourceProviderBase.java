package com.mugen.mvvm.interfaces;

public interface IItemsSourceProviderBase {
    int getCount();

    void addObserver(IItemsSourceObserver observer);

    void removeObserver(IItemsSourceObserver observer);
}
