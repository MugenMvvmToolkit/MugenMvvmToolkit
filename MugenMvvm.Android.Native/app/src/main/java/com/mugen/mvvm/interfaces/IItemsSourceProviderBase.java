package com.mugen.mvvm.interfaces;

public interface IItemsSourceProviderBase {
    boolean hasStableId();

    long getItemId(int position);

    boolean containsItem(long itemId);

    int getCount();

    CharSequence getItemTitle(int position);

    void addObserver(IItemsSourceObserver observer);

    void removeObserver(IItemsSourceObserver observer);
}
