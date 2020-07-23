package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.views.IAndroidView;

public class NativeContentItemsSourceProviderWrapper implements IContentItemsSourceProvider {
    private final IContentItemsSourceProvider _target;

    public NativeContentItemsSourceProviderWrapper(IContentItemsSourceProvider target) {
        _target = target;
    }

    public IContentItemsSourceProvider getNestedProvider() {
        return _target;
    }

    @Override
    public CharSequence getTitle(int position) {
        return _target.getTitle(position);
    }

    @Override
    public Object getContent(int position) {
        //todo add fragments
        Object content = _target.getContent(position);
        if (content instanceof IAndroidView)
            return ((IAndroidView) content).getView();
        return content;
    }

    @Override
    public int getContentPosition(Object content) {
        return _target.getContentPosition(MugenExtensions.wrap(content, true));
    }

    @Override
    public void onPrimaryContentChanged(int position, Object oldContent, Object newContent) {
        _target.onPrimaryContentChanged(position, MugenExtensions.wrap(oldContent, true), MugenExtensions.wrap(newContent, true));
    }

    @Override
    public void destroyContent(int position, Object content) {
        _target.destroyContent(position, MugenExtensions.wrap(content, true));
    }

    @Override
    public int getCount() {
        return _target.getCount();
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
