package com.mugen.mvvm.views;

import android.content.Context;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;

public class ActivityWrapper implements IActivityView {
    private final IActivityView _target;

    public ActivityWrapper(INativeActivityView target) {
        _target = target;
    }

    @Override
    public Context getActivity() {
        return (Context) _target;
    }

    @Override
    public boolean isFinishing() {
        return _target.isFinishing();
    }

    @Override
    public void finish() {
        _target.finish();
    }

    @Override
    public Object getState() {
        return _target.getState();
    }

    @Override
    public void setState(Object state) {
        _target.setState(state);
    }

    @Override
    public int getViewId() {
        return _target.getViewId();
    }
}
