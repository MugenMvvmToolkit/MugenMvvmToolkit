package com.mugen.mvvm.views.fragments;

import android.content.Context;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;

public class FragmentWrapper implements IFragmentView {
    protected final INativeFragmentView Target;

    public FragmentWrapper(INativeFragmentView target) {
        Target = target;
    }

    @NonNull
    @Override
    public Object getFragment() {
        return Target;
    }

    @Override
    public void setViewResourceId(int resourceId) {
        Target.setViewResourceId(resourceId);
    }

    @Nullable
    @Override
    public Object getState() {
        return Target.getState();
    }

    @Override
    public void setState(@Nullable Object value) {
        Target.setState(value);
    }

    @Override
    public int getViewId() {
        return Target.getViewId();
    }

    @Override
    public Context getContext() {
        return Target.getContext();
    }
}
