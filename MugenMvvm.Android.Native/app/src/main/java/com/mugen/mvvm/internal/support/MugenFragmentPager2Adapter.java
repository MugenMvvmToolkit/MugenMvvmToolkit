package com.mugen.mvvm.internal.support;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.lifecycle.Lifecycle;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.adapter.FragmentStateAdapter;

import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.views.IFragmentView;

public class MugenFragmentPager2Adapter extends FragmentStateAdapter implements IItemsSourceObserver, IMugenAdapter {
    private final IContentItemsSourceProvider _provider;
    private final boolean _hasStableIds;
    private int _attachCount;

    public MugenFragmentPager2Adapter(IContentItemsSourceProvider provider, @NonNull FragmentManager fragmentManager, @NonNull Lifecycle lifecycle) {
        super(fragmentManager, lifecycle);
        _provider = provider;
        _hasStableIds = provider.hasStableId();
    }

    public IContentItemsSourceProvider getItemsSourceProvider() {
        return _provider;
    }

    public void detach() {
    }

    @NonNull
    @Override
    public Fragment createFragment(int position) {
        Fragment fragment = (Fragment) ((IFragmentView) _provider.getContent(position)).getFragment();
        //note we need to remove fragment and request new because adapter expects new fragment
        FragmentManager fragmentManager = fragment.getFragmentManager();
        if (fragmentManager != null) {
            fragmentManager.beginTransaction().remove(fragment).commitNowAllowingStateLoss();
            fragment = (Fragment) ((IFragmentView) _provider.getContent(position)).getFragment();
        }
        return fragment;
    }

    @Override
    public int getItemCount() {
        return _provider.getCount();
    }

    @Override
    public long getItemId(int position) {
        if (_hasStableIds)
            return _provider.getItemId(position);
        return super.getItemId(position);
    }

    @Override
    public boolean containsItem(long itemId) {
        if (_hasStableIds)
            return _provider.containsItem(itemId);
        return super.containsItem(itemId);
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
    public boolean isDiffUtilSupported() {
        return true;
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
