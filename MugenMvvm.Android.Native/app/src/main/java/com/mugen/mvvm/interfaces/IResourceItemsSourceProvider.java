package com.mugen.mvvm.interfaces;

import android.view.View;

import androidx.annotation.NonNull;

public interface IResourceItemsSourceProvider extends IItemsSourceProviderBase {
    int getViewTypeCount();

    int getItemViewType(int position);

    void onViewCreated(@NonNull View view);

    void onBindView(@NonNull View view, int position);
}
