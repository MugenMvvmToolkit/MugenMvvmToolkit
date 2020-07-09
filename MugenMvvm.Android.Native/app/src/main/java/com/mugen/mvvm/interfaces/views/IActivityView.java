package com.mugen.mvvm.interfaces.views;

import android.app.Activity;

public interface IActivityView extends IResourceView, IHasTagView {
    Activity getActivity();

    boolean isFinishing();

    void finish();
}
