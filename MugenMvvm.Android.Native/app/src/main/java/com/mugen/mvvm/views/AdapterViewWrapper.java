package com.mugen.mvvm.views;

import android.view.View;
import android.widget.Adapter;
import android.widget.AdapterView;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IListView;
import com.mugen.mvvm.internal.MugenListAdapter;
import com.mugen.mvvm.internal.NativeResourceItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;

public class AdapterViewWrapper extends ViewWrapper implements IListView {
    public AdapterViewWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public IItemsSourceProviderBase getItemsSourceProvider() {
        AdapterView view = (AdapterView) getView();
        if (view == null)
            return null;
        return getProvider(view);
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProviderBase provider) {
        AdapterView view = (AdapterView) getView();
        if (view != null)
            setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        setItemsSourceProvider((AdapterView) target, null);
    }

    private IResourceItemsSourceProvider getProvider(AdapterView view) {
        Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenListAdapter) {
            IResourceItemsSourceProvider provider = ((MugenListAdapter) adapter).getItemsSourceProvider();
            if (provider != null)
                return ((NativeResourceItemsSourceProviderWrapper) provider).getNestedProvider();
        }
        return null;
    }

    private void setItemsSourceProvider(AdapterView view, IResourceItemsSourceProvider provider) {
        if (getProvider(view) == provider)
            return;
        Adapter adapter = view.getAdapter();
        if (provider == null) {
            if (adapter instanceof MugenListAdapter)
                ((MugenListAdapter) adapter).detach();
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenListAdapter(view, view.getContext(), new NativeResourceItemsSourceProviderWrapper(provider)));
    }
}
