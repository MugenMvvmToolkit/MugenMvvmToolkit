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
    public Object getTag() {
        return _target.getTag();
    }

    @Override
    public void setTag(Object state) {
        _target.setTag(state);
    }

    @Override
    public int getViewId() {
        return _target.getViewId();
    }
}
