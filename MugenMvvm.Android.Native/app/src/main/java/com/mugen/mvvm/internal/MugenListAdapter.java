package com.mugen.mvvm.internal;

import android.content.Context;
import android.util.SparseIntArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Adapter;
import android.widget.BaseAdapter;

import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.views.ViewExtensions;

public class MugenListAdapter extends BaseAdapter implements IItemsSourceObserver, IMugenAdapter {
    private final IResourceItemsSourceProvider _provider;
    private final boolean _hasStableId;
    private final int _viewTypeCount;
    private final LayoutInflater _inflater;

    private SparseIntArray _resourceTypeToItemType;
    private int _currentTypeIndex;

    public MugenListAdapter(Context context, IResourceItemsSourceProvider provider) {
        _inflater = LayoutInflater.from(context);
        _provider = provider;
        _hasStableId = provider.hasStableId();
        _viewTypeCount = provider.getViewTypeCount();
        provider.addObserver(this);
    }

    public IResourceItemsSourceProvider getItemsSourceProvider() {
        return _provider;
    }

    public void detach() {
        _provider.removeObserver(this);
    }

    @Override
    public boolean hasStableIds() {
        return _hasStableId;
    }

    @Override
    public int getItemViewType(int position) {
        if (getCount() == 0)
            return Adapter.IGNORE_ITEM_VIEW_TYPE;
        if (_viewTypeCount == 1)
            return super.getItemViewType(position);
        int resourceId = _provider.getItemViewType(position);
        if (resourceId == 0)
            return 0;
        if (_resourceTypeToItemType == null)
            _resourceTypeToItemType = new SparseIntArray();
        int type = _resourceTypeToItemType.get(resourceId, -1);
        if (type < 0) {
            type = _currentTypeIndex++;
            _resourceTypeToItemType.put(resourceId, type);
        }
        return type;
    }

    @Override
    public int getViewTypeCount() {
        return _viewTypeCount;
    }

    @Override
    public int getCount() {
        return _provider.getCount();
    }

    @Override
    public Object getItem(int position) {
        return null;
    }

    @Override
    public long getItemId(int position) {
        if (_provider.hasStableId())
            return _provider.getItemId(position);
        return position;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        int itemResourceId = _provider.getItemViewType(position);
        if (convertView == null || !isValidView(convertView, itemResourceId)) {
            convertView = _inflater.inflate(itemResourceId, parent, false);
            ViewExtensions.getNativeAttachedValues(convertView, true).setListResourceId(itemResourceId);
            _provider.onViewCreated(convertView);
        }
        _provider.onBindView(convertView, position);
        return convertView;
    }

    private boolean isValidView(View convertView, int itemResourceId) {
        return ViewExtensions.getNativeAttachedValues(convertView, true).getListResourceId() == itemResourceId;
    }

    @Override
    public boolean isDiffUtilSupported() {
        return false;
    }

    @Override
    public void onItemChanged(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemInserted(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemMoved(int fromPosition, int toPosition) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRemoved(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeChanged(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeInserted(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeRemoved(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onReset() {
        notifyDataSetChanged();
    }
}
