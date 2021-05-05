package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;

public interface IActivityView extends IResourceView, IHasStateView, IHasLifecycleView, IHasContext {
    @NonNull
    Object getActivity();

    boolean isFinishing();

    boolean isDestroyed();

    void setContentView(int layoutResID);

    void finish();
}
