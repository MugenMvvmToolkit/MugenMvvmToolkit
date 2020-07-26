package com.mugen.mvvm.internal;

import java.util.ArrayList;

public class ActivityAttachedValues extends AttachedValues {
    private AttachedValues _actionBarAttachedValues;
    private Object _wrapper;
    private ArrayList<Object> _views;

    public Object getWrapper() {
        return _wrapper;
    }

    public void setWrapper(Object wrapper) {
        _wrapper = wrapper;
    }

    public AttachedValues getActionBarAttachedValues() {
        return _actionBarAttachedValues;
    }

    public void setActionBarAttachedValues(AttachedValues values) {
        _actionBarAttachedValues = values;
    }

    public ArrayList<Object> getViews(boolean required) {
        if (_views == null && required)
            _views = new ArrayList<>();
        return _views;
    }
}
