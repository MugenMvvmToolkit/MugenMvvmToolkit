package com.mugen.mvvm.views;

import android.content.Context;

import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;

public class ActivityWrapper implements IActivityView {
    protected final IActivityView Target;

    public ActivityWrapper(INativeActivityView target) {
        Target = target;
    }

    @Override
    public Context getActivity() {
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
    public void finish() {
        Target.finish();
    }

    @Override
    public Object getState() {
        return Target.getState();
    }

    @Override
    public void setState(Object state) {
        Target.setState(state);
    }

    @Override
    public int getViewId() {
        return Target.getViewId();
    }
}
