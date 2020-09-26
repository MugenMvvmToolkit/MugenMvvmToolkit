package com.mugen.mvvm.internal;

import android.content.res.TypedArray;

import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.views.IViewAttributeAccessor;

public final class ViewAttributeAccessor implements IViewAttributeAccessor {
    private TypedArray _typedArray;

    public void setTypedArray(TypedArray array) {
        _typedArray = array;
    }

    @Override
    public String getString(int index) {
        return _typedArray.getString(index);
    }

    @Override
    public int getResourceId(int index) {
        return _typedArray.getResourceId(index, 0);
    }

    @Override
    public String getBind() {
        return getString(R.styleable.Bind_bind);
    }

    @Override
    public int getItemTemplate() {
        return getResourceId(R.styleable.Bind_itemTemplate);
    }
}
