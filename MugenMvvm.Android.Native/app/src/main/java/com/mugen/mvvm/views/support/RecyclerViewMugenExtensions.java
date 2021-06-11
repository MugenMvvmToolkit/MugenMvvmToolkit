package com.mugen.mvvm.views.support;

import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.recyclerview.widget.RecyclerView;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.ItemSourceProviderType;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;

public final class RecyclerViewMugenExtensions {
    public static final int ItemsSourceProviderType = ItemSourceProviderType.Resource;

    private RecyclerViewMugenExtensions() {
    }

    public static boolean isSupported(@Nullable View view) {
        return MugenUtils.hasFlag(MugenInitializationFlags.RecyclerViewLib) && view instanceof RecyclerView;
    }

    @Nullable
    public static IItemsSourceProviderBase getItemsSourceProvider(@NonNull View view) {
        RecyclerView.Adapter adapter = ((RecyclerView) view).getAdapter();
        if (adapter instanceof IMugenAdapter)
            return ((IMugenAdapter) adapter).getItemsSourceProvider();
        return null;
    }

    public static void setItemsSourceProvider(@NonNull View v, @Nullable IResourceItemsSourceProvider provider) {
        RecyclerView view = (RecyclerView) v;
        if (getItemsSourceProvider(view) == provider)
            return;
        if (provider == null) {
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), provider));
    }

    public static void onDestroy(@NonNull View view) {
        setItemsSourceProvider(view, null);
    }
}
