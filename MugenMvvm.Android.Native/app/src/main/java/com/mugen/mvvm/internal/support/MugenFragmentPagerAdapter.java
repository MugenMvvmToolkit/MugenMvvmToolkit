package com.mugen.mvvm.internal.support;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentPagerAdapter;

import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.views.IFragmentView;

public class MugenFragmentPagerAdapter extends FragmentPagerAdapter implements IItemsSourceObserver, IMugenAdapter {
    private final IContentItemsSourceProvider _provider;
    private final boolean _hasStableIds;

    public MugenFragmentPagerAdapter(@NonNull IContentItemsSourceProvider provider, @NonNull FragmentManager fm) {
        super(fm, BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);
        _provider = provider;
        _hasStableIds = provider.hasStableId();
        provider.addObserver(this);
    }

    @NonNull
    public IContentItemsSourceProvider getItemsSourceProvider() {
        return _provider;
    }

    public void detach() {
        _provider.removeObserver(this);
    }

    @Nullable
    @Override
    public CharSequence getPageTitle(int position) {
        return _provider.getItemTitle(position);
    }

    @NonNull
    @Override
    public Fragment getItem(int position) {
        return (Fragment) ((IFragmentView) _provider.getContent(position)).getFragment();
    }

    @Override
    public long getItemId(int position) {
        if (_hasStableIds)
            return _provider.getItemId(position);
        return super.getItemId(position);
    }

    @Override
    public int getCount() {
        return _provider.getCount();
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
