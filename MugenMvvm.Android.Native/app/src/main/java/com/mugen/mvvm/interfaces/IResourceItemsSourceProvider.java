package com.mugen.mvvm.interfaces;

import android.view.View;

public interface IResourceItemsSourceProvider extends IItemsSourceProviderBase {
    int getViewTypeCount();

    int getItemViewType(int position);

    void onViewCreated(View view);

    void onBindView(View view, int position);
}
