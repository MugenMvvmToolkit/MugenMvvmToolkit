package com.mugen.mvvm.interfaces.views;

public interface IActivityView extends IResourceView, IHasTagView {
    boolean isFinishing();

    void finish();
}
