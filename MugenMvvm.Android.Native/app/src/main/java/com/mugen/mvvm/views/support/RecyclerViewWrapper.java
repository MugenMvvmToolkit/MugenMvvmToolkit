package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.recyclerview.widget.RecyclerView;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IListView;
import com.mugen.mvvm.internal.NativeResourceItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewWrapper;

public class RecyclerViewWrapper extends ViewWrapper implements IListView {
    public RecyclerViewWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public int getProviderType() {
        return ItemSourceProviderType;
    }

    @Override
    public IItemsSourceProviderBase getItemsSourceProvider() {
        RecyclerView view = (RecyclerView) getView();
        if (view == null)
            return null;
        return getProvider(view);
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProviderBase provider) {
        RecyclerView view = (RecyclerView) getView();
        if (view == null || getProvider(view) == provider)
            return;
        if (provider == null) {
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), new NativeResourceItemsSourceProviderWrapper((IResourceItemsSourceProvider) provider)));
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        ((RecyclerView) target).setAdapter(null);
    }

    private IResourceItemsSourceProvider getProvider(RecyclerView view) {
        RecyclerView.Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter) {
            IResourceItemsSourceProvider provider = ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider();
            if (provider != null)
                return ((NativeResourceItemsSourceProviderWrapper) provider).getNestedProvider();
        }
        return null;
    }
}
