package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.recyclerview.widget.RecyclerView;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;

public abstract class RecyclerViewExtensions extends ViewExtensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ResourceProviderType;
    private static boolean _supported;

    public static boolean isSupported(View view) {
        return _supported && view instanceof RecyclerView;
    }

    public static void setSupported() {
        _supported = true;
    }

    public static IResourceItemsSourceProvider getItemsSourceProvider(View view) {
        RecyclerView.Adapter adapter = ((RecyclerView) view).getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter)
            return ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider();
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
