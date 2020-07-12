package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IAndroidView;

public class NativeResourceItemsSourceProviderWrapper implements IResourceItemsSourceProvider {
    private final IResourceItemsSourceProvider _target;
    private final Object _owner;

    public NativeResourceItemsSourceProviderWrapper(Object owner, IResourceItemsSourceProvider target) {
        _owner = owner;
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
        Object viewWrapper = MugenExtensions.wrap(view, true);
        _target.onViewCreated(viewWrapper);
        if (viewWrapper instanceof IAndroidView)
            ((IAndroidView) viewWrapper).setParent(_owner);
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
