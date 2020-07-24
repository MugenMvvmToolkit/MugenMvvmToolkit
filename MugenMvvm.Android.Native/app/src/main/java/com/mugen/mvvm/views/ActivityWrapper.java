package com.mugen.mvvm.views;

import android.app.Activity;
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
        return (Context)_target;
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
    public Object getTag(int id) {
        return _target.getTag(id);
    }

    @Override
    public void setTag(int id, Object state) {
        _target.setTag(id, state);
    }

    @Override
    public int getViewId() {
        return _target.getViewId();
    }
}
