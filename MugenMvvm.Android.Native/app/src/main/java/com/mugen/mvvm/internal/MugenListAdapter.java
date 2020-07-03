package com.mugen.mvvm.internal;

import android.content.Context;
import android.util.SparseIntArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Adapter;
import android.widget.BaseAdapter;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IItemsSourceProvider;

public class MugenListAdapter extends BaseAdapter implements IItemsSourceObserver {
    private final IItemsSourceProvider _provider;
    private final boolean _hasStableId;
    private final int _viewTypeCount;
    private final LayoutInflater _inflater;
    private final Object _owner;

    private SparseIntArray _resourceTypeToItemType;
    private int _currentTypeIndex;

    public MugenListAdapter(Object owner, Context context, IItemsSourceProvider provider) {
        _owner = owner;
        _inflater = LayoutInflater.from(context);
        _provider = provider;
        _hasStableId = provider.hasStableId();
        _viewTypeCount = provider.getViewTypeCount();
        provider.addObserver(this);
    }

    public IItemsSourceProvider getItemsSourceProvider() {
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
        int resourceId = _provider.getItemResourceId(position);
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
        int itemResourceId = _provider.getItemResourceId(position);
        if (convertView == null || !isValidView(convertView, itemResourceId)) {
            convertView = _inflater.inflate(itemResourceId, parent, false);
            convertView.setTag(R.id.listItemResourceId, itemResourceId);
            _provider.onViewCreated(_owner, convertView);
        }
        _provider.onBindView(_owner, convertView, position);
        return convertView;
    }

    private boolean isValidView(View convertView, int itemResourceId) {
        Integer tag = (Integer) convertView.getTag(R.id.listItemResourceId);
        if (tag == null)
            return false;
        return tag == itemResourceId;
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
