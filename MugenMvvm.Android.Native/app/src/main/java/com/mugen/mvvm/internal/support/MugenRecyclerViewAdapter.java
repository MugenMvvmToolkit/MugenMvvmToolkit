package com.mugen.mvvm.internal.support;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IItemsSourceProvider;

public class MugenRecyclerViewAdapter extends RecyclerView.Adapter implements IItemsSourceObserver {
    private final IItemsSourceProvider _provider;
    private final LayoutInflater _inflater;
    private final Object _owner;
    private int _attachCount;

    public MugenRecyclerViewAdapter(Object owner, Context context, IItemsSourceProvider provider) {
        _owner = owner;
        _inflater = LayoutInflater.from(context);
        _provider = provider;
        setHasStableIds(provider.hasStableId());
    }

    public IItemsSourceProvider getItemsSourceProvider() {
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

    @NonNull
    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = _inflater.inflate(viewType, parent, false);
        _provider.onViewCreated(_owner, view);
        return new ViewHolderIml(view);
    }

    @Override
    public long getItemId(int position) {
        if (hasStableIds())
            return _provider.getItemId(position);
        return super.getItemId(position);
    }

    @Override
    public int getItemViewType(int position) {
        return _provider.getItemResourceId(position);
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
        _provider.onBindView(_owner, holder.itemView, position);
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

    private static final class ViewHolderIml extends RecyclerView.ViewHolder {
        public ViewHolderIml(@NonNull View itemView) {
            super(itemView);
        }
    }
}
