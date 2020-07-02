package com.mugen.mvvm.interfaces;

public interface IItemsSourceObserver {
    void onItemChanged(int position);

    void onItemInserted(int position);

    void onItemMoved(int fromPosition, int toPosition);

    void onItemRemoved(int position);

    void onItemRangeChanged(int positionStart, int itemCount);

    void onItemRangeInserted(int positionStart, int itemCount);

    void onItemRangeRemoved(int positionStart, int itemCount);

    void onReset();
}
