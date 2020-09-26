package com.mugen.mvvm.internal.support;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;

public class MugenRecyclerViewAdapter extends MugenRecyclerViewAdapterBase<IResourceItemsSourceProvider> implements IItemsSourceObserver {
    private final LayoutInflater _inflater;

    public MugenRecyclerViewAdapter(Context context, IResourceItemsSourceProvider provider) {
        super(provider);
        _inflater = LayoutInflater.from(context);
        setHasStableIds(provider.hasStableId());
    }

    @NonNull
    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = _inflater.inflate(viewType, parent, false);
        _provider.onViewCreated(view);
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
        return _provider.getItemViewType(position);
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
        _provider.onBindView(holder.itemView, position);
    }

    private static final class ViewHolderIml extends RecyclerView.ViewHolder {
        public ViewHolderIml(@NonNull View itemView) {
            super(itemView);
        }
    }
}
