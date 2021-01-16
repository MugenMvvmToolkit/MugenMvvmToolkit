package com.mugen.mvvm.internal;

import androidx.annotation.Nullable;

public class FragmentAttachedValues extends AttachedValues {
    private Object _wrapper;

    @Nullable
    public Object getWrapper() {
        return _wrapper;
    }

    public void setWrapper(@Nullable Object wrapper) {
        _wrapper = wrapper;
    }
}
