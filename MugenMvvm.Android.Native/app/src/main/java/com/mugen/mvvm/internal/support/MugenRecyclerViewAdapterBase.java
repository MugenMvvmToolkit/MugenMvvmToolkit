package com.mugen.mvvm.internal.support;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;

public abstract class MugenRecyclerViewAdapterBase<T extends IItemsSourceProviderBase> extends RecyclerView.Adapter implements IItemsSourceObserver {
    protected final T _provider;
    private int _attachCount;

    public MugenRecyclerViewAdapterBase(T provider) {
        _provider = provider;
    }

    public T getItemsSourceProvider() {
        return _provider;
    }

    @Override
    public void onAttachedToRecyclerView(@NonNull RecyclerView recyclerView) {
        super.onAttachedToRecyclerView(recyclerView);
        if (_attachCount++ == 0)
            _provider.addObserver(this);
    }

    @Override
    public void onDetachedFromRecyclerView(@NonNull RecyclerView recyclerView) {
        super.onDetachedFromRecyclerView(recyclerView);
        if (_attachCount != 0 && --_attachCount == 0)
            _provider.removeObserver(this);
    }

    @Override
    public int getItemCount() {
        return _provider.getCount();
    }

    @Override
    public void onItemChanged(int position) {
        notifyItemChanged(position);
    }

    @Override
    public void onItemInserted(int position) {
        notifyItemInserted(position);
    }

    @Override
    public void onItemMoved(int fromPosition, int toPosition) {
        notifyItemMoved(fromPosition, toPosition);
    }

    @Override
    public void onItemRemoved(int position) {
        notifyItemRemoved(position);
    }

    @Override
    public void onItemRangeChanged(int positionStart, int itemCount) {
        notifyItemRangeChanged(positionStart, itemCount);
    }

    @Override
    public void onItemRangeInserted(int positionStart, int itemCount) {
        notifyItemRangeInserted(positionStart, itemCount);
    }

    @Override
    public void onItemRangeRemoved(int positionStart, int itemCount) {
        notifyItemRangeRemoved(positionStart, itemCount);
    }

    @Override
    public void onReset() {
        notifyDataSetChanged();
    }
}