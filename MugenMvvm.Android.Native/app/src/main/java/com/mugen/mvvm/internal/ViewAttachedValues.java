package com.mugen.mvvm.internal;

import androidx.annotation.Nullable;

public class ViewAttachedValues extends AttachedValues {
    private Object _parent;
    private Object _fragment;
    private boolean _isParentObserverDisabled;
    private int _listResourceId;
    private boolean _isBindHandled;

    @Nullable
    public Object getParent() {
        return _parent;
    }

    public void setParent(@Nullable Object parent) {
        _parent = parent;
    }

    public boolean isParentObserverDisabled() {
        return _isParentObserverDisabled;
    }

    public void setParentObserverDisabled(boolean value) {
        _isParentObserverDisabled = value;
    }

    public int getListResourceId() {
        return _listResourceId;
    }

    public void setListResourceId(int id) {
        _listResourceId = id;
    }

    public boolean isBindHandled() {
        return _isBindHandled;
    }

    public void setBindHandled(boolean value) {
        _isBindHandled = value;
    }

    @Nullable
    public Object getFragment() {
        return _fragment;
    }

    public void setFragment(@Nullable Object fragment) {
        _fragment = fragment;
    }
}
