package com.mugen.mvvm.views;

import android.view.View;
import android.widget.Adapter;
import android.widget.AdapterView;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.MugenListAdapter;

//todo release setItemsSourceProvider((AdapterView) target, null);
public abstract class AdapterViewExtensions extends ViewExtensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ResourceProviderType;

    public static boolean isSupported(View view) {
        return view instanceof AdapterView;
    }

    public static IResourceItemsSourceProvider getItemsSourceProvider(View view) {
        Adapter adapter = ((AdapterView) view).getAdapter();
        if (adapter instanceof MugenListAdapter)
            return ((MugenListAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(View view, IResourceItemsSourceProvider provider) {
        if (getItemsSourceProvider(view) == provider)
            return;
        AdapterView<Adapter> adapterView = (AdapterView<Adapter>) view;
        Adapter adapter = adapterView.getAdapter();
        if (provider == null) {
            if (adapter instanceof MugenListAdapter)
                ((MugenListAdapter) adapter).detach();
            adapterView.setAdapter(null);
        } else
            adapterView.setAdapter(new MugenListAdapter(view.getContext(), provider));
    }
}
