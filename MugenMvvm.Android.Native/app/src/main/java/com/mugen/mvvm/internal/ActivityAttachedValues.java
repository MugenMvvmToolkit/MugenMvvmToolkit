package com.mugen.mvvm.internal;

import androidx.annotation.Nullable;

import java.util.ArrayList;

public class ActivityAttachedValues extends AttachedValues {
    private AttachedValues _actionBarAttachedValues;
    private Object _wrapper;
    private ArrayList<Object> _views;

    @Nullable
    public Object getWrapper() {
        return _wrapper;
    }

    public void setWrapper(@Nullable Object wrapper) {
        _wrapper = wrapper;
    }

    @Nullable
    public AttachedValues getActionBarAttachedValues() {
        return _actionBarAttachedValues;
    }

    public void setActionBarAttachedValues(@Nullable AttachedValues values) {
        _actionBarAttachedValues = values;
    }

    public ArrayList<Object> getViews(boolean required) {
        if (_views == null && required)
            _views = new ArrayList<>();
        return _views;
    }
}
