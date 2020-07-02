package com.mugen.mvvm.interfaces;

public interface IItemsSourceProvider {
    boolean hasStableId();

    int getCount();

    int getViewTypeCount();

    long getItemId(int position);

    int getItemResourceId(int position);

    void onViewCreated(Object owner, Object view);

    void onBindView(Object owner, Object view, int position);

    void addObserver(IItemsSourceObserver observer);

    void removeObserver(IItemsSourceObserver observer);
}
