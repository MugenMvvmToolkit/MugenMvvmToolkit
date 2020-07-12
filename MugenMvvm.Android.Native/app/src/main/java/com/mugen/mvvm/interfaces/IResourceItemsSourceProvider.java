package com.mugen.mvvm.interfaces;

public interface IResourceItemsSourceProvider extends IItemsSourceProviderBase {
    boolean hasStableId();

    int getViewTypeCount();

    long getItemId(int position);

    int getItemViewType(int position);

    void onViewCreated(Object view);

    void onBindView(Object view, int position);
}
