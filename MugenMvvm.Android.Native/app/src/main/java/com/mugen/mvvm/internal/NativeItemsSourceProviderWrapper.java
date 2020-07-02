package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IAndroidView;

public class NativeItemsSourceProviderWrapper implements IItemsSourceProvider {
    private final IItemsSourceProvider _target;

    public NativeItemsSourceProviderWrapper(IItemsSourceProvider _target) {
        this._target = _target;
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
    public int getItemResourceId(int position) {
        return _target.getItemResourceId(position);
    }

    @Override
    public void onViewCreated(Object owner, Object view) {
        Object viewWrapper = MugenExtensions.wrap(view, true);
        _target.onViewCreated(MugenExtensions.wrap(owner, true), viewWrapper);
        if (viewWrapper instanceof IAndroidView)
            ((IAndroidView) viewWrapper).setParent(owner);
    }

    @Override
    public void onBindView(Object owner, Object view, int position) {
        _target.onBindView(MugenExtensions.wrap(owner, true), MugenExtensions.wrap(view, true), position);
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
