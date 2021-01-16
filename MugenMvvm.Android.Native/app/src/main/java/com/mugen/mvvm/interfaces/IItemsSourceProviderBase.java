package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public interface IItemsSourceProviderBase {
    boolean hasStableId();

    long getItemId(int position);

    boolean containsItem(long itemId);

    int getCount();

    @Nullable
    CharSequence getItemTitle(int position);

    void addObserver(@NonNull IItemsSourceObserver observer);

    void removeObserver(@NonNull IItemsSourceObserver observer);

    void move(int fromIndex, int toIndex);

    void swap(int i, int j);

    void removeAt(int index);
}
