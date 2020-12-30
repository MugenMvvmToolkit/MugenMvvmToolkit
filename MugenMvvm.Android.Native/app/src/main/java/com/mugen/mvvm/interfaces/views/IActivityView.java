package com.mugen.mvvm.interfaces.views;

public interface IActivityView extends IResourceView, IHasStateView, IHasLifecycleView {
    Object getActivity();

    boolean isFinishing();

    boolean isDestroyed();

    void setContentView(int layoutResID);

    void finish();
}
