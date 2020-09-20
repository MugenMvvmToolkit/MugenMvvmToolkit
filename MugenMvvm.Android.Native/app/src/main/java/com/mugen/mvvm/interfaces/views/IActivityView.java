package com.mugen.mvvm.interfaces.views;

import android.content.Context;

public interface IActivityView extends IResourceView, IHasStateView, IHasLifecycleView {
    Context getActivity();

    boolean isFinishing();

    boolean isDestroyed();

    void finish();
}
