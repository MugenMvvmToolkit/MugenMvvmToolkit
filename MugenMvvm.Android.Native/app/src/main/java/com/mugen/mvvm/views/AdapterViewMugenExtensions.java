package com.mugen.mvvm.views;

import android.view.View;
import android.widget.Adapter;
import android.widget.AdapterView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.ItemSourceProviderType;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.MugenListAdapter;

public final class AdapterViewMugenExtensions {
    public static final int ItemsSourceProviderType = ItemSourceProviderType.Resource;

    private AdapterViewMugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return view instanceof AdapterView;
    }

    @Nullable
    public static IItemsSourceProviderBase getItemsSourceProvider(@NonNull View view) {
        Adapter adapter = ((AdapterView) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(@NonNull View view, @Nullable IResourceItemsSourceProvider provider) {
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

    public static void onDestroy(@NonNull View view) {
        setItemsSourceProvider(view, null);
    }
}
