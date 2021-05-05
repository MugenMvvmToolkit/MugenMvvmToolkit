package com.mugen.mvvm.views.activities;

import android.content.Context;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;

public class ActivityWrapper implements IActivityView {
    protected final IActivityView Target;

    public ActivityWrapper(INativeActivityView target) {
        Target = target;
    }

    @NonNull
    @Override
    public Object getActivity() {
        return (Context) Target;
    }

    @Override
    public boolean isFinishing() {
        return Target.isFinishing();
    }

    @Override
    public boolean isDestroyed() {
        return Target.isDestroyed();
    }

    @Override
    public void setContentView(int layoutResID) {
        Target.setContentView(layoutResID);
    }

    @Override
    public void finish() {
        Target.finish();
    }

    @Nullable
    @Override
    public Object getState() {
        return Target.getState();
    }

    @Override
    public void setState(@Nullable Object state) {
        Target.setState(state);
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
