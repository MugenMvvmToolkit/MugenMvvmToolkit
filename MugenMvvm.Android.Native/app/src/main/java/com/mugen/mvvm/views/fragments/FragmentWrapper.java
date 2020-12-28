package com.mugen.mvvm.views.fragments;

import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;

public class FragmentWrapper implements IFragmentView {
    protected final INativeFragmentView Target;

    public FragmentWrapper(INativeFragmentView target) {
        Target = target;
    }

    @Override
    public Object getFragment() {
        return Target;
    }

    @Override
    public Object getState() {
        return Target.getState();
    }

    @Override
    public void setState(Object value) {
        Target.setState(value);
    }

    @Override
    public int getViewId() {
        return Target.getViewId();
    }
}
