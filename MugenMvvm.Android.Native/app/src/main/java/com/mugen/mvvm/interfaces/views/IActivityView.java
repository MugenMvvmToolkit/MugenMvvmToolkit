package com.mugen.mvvm.interfaces.views;

import android.content.Context;

public interface IActivityView extends IResourceView {
    Context getActivity();

    boolean isFinishing();

    void finish();

    Object getTag(int id);

    void setTag(int id, Object state);
}
