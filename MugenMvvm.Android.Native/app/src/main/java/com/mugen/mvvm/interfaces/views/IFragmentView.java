package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;

public interface IFragmentView extends IResourceView, IHasStateView, IHasLifecycleView {
    @NonNull
    Object getFragment();

    void setViewResourceId(int resourceId);
}
