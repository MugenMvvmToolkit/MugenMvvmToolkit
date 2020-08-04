package com.mugen.mvvm.interfaces;

public interface IItemsSourceProviderBase {
    boolean hasStableId();

    long getItemId(int position);

    int getCount();

    void addObserver(IItemsSourceObserver observer);

    void removeObserver(IItemsSourceObserver observer);
}
