package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.recyclerview.widget.RecyclerView;
import com.mugen.mvvm.interfaces.IItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IListView;
import com.mugen.mvvm.internal.NativeItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewWrapper;

public class RecyclerViewWrapper extends ViewWrapper implements IListView {
    public RecyclerViewWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public IItemsSourceProvider getItemsSourceProvider() {
        RecyclerView view = (RecyclerView) getView();
        if (view == null)
            return null;
        RecyclerView.Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter)
            return ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProvider provider) {
        RecyclerView view = (RecyclerView) getView();
        if (view == null)
            return;
        RecyclerView.Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter && ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider() == provider)
            return;
        if (provider == null) {
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenRecyclerViewAdapter(view, view.getContext(), new NativeItemsSourceProviderWrapper(provider)));
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        ((RecyclerView) target).setAdapter(null);
    }
}
