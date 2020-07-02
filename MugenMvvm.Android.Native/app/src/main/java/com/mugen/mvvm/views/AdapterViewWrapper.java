package com.mugen.mvvm.views;

import android.view.View;
import android.widget.Adapter;
import android.widget.AdapterView;
import com.mugen.mvvm.interfaces.IItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IListView;
import com.mugen.mvvm.internal.MugenListAdapter;
import com.mugen.mvvm.internal.NativeItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;

public class AdapterViewWrapper extends ViewWrapper implements IListView {
    public AdapterViewWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public IItemsSourceProvider getItemsSourceProvider() {
        AdapterView view = (AdapterView) getView();
        if (view == null)
            return null;
        Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenListAdapter)
            return ((MugenListAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProvider provider) {
        AdapterView view = (AdapterView) getView();
        if (view != null)
            setItemsSourceProvider(view, provider);
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        setItemsSourceProvider((AdapterView) target, null);
    }

    private void setItemsSourceProvider(AdapterView view, IItemsSourceProvider provider) {
        Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenListAdapter && ((MugenListAdapter) adapter).getItemsSourceProvider() == provider)
            return;
        if (provider == null) {
            if (adapter instanceof MugenListAdapter)
                ((MugenListAdapter) adapter).detach();
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenListAdapter(view, view.getContext(), new NativeItemsSourceProviderWrapper(provider)));
    }
}
