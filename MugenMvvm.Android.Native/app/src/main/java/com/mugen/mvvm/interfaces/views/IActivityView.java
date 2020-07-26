package com.mugen.mvvm.interfaces.views;

import android.content.Context;

public interface IActivityView extends IResourceView, IHasTagView {
    Context getActivity();

    boolean isFinishing();

    void finish();
}
