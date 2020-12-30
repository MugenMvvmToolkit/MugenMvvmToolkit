package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.recyclerview.widget.RecyclerView;

import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewGroupMugenExtensions;

public final class RecyclerViewMugenExtensions {
    public static final int ItemsSourceProviderType = ViewGroupMugenExtensions.ResourceProviderType;
    private static boolean _supported;

    private RecyclerViewMugenExtensions() {
    }

    public static boolean isSupported(View view) {
        return _supported && view instanceof RecyclerView;
    }

    public static void setSupported() {
        _supported = true;
    }

    public static IItemsSourceProviderBase getItemsSourceProvider(View view) {
        RecyclerView.Adapter adapter = ((RecyclerView) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View v, IResourceItemsSourceProvider provider) {
        RecyclerView view = (RecyclerView) v;
        if (getItemsSourceProvider(view) == provider)
            return;
        if (provider == null) {
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), provider));
    }

    public static void onDestroy(View view) {
        setItemsSourceProvider(view, null);
    }
}
