package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;

public class NativeResourceItemsSourceProviderWrapper implements IResourceItemsSourceProvider {
    private final IResourceItemsSourceProvider _target;

    public NativeResourceItemsSourceProviderWrapper(IResourceItemsSourceProvider target) {
        _target = target;
    }

    public IResourceItemsSourceProvider getNestedProvider() {
        return _target;
    }

    @Override
    public boolean hasStableId() {
        return _target.hasStableId();
    }

    @Override
    public int getCount() {
        return _target.getCount();
    }

    @Override
    public int getViewTypeCount() {
        return _target.getViewTypeCount();
    }

    @Override
    public long getItemId(int position) {
        return _target.getItemId(position);
    }

    @Override
    public int getItemViewType(int position) {
        return _target.getItemViewType(position);
    }

    @Override
    public void onViewCreated(Object view) {
        _target.onViewCreated(MugenExtensions.wrap(view, true));
    }

    @Override
    public void onBindView(Object view, int position) {
        _target.onBindView(MugenExtensions.wrap(view, true), position);
    }

    @Override
    public void addObserver(IItemsSourceObserver observer) {
        _target.addObserver(observer);
    }

    @Override
    public void removeObserver(IItemsSourceObserver observer) {
        _target.removeObserver(observer);
    }
}
