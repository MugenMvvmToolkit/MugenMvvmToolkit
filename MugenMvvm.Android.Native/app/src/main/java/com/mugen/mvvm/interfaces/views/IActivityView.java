package com.mugen.mvvm.interfaces.views;

import android.content.Intent;

import androidx.annotation.NonNull;

public interface IActivityView extends IResourceView, IHasStateView, IHasLifecycleView, IHasContext {
    @NonNull
    Object getActivity();

    @NonNull
    Intent getIntent();

    boolean isFinishing();

    boolean isDestroyed();

    void setContentView(int layoutResID);

    void finish();
}
